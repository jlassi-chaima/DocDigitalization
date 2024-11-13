using Application.Dtos.Correspondent;
using Application.Respository;
using Domain.DocumentManagement.Correspondent;
using Domain.DocumentManagement;
using Domain.Documents;
using MapsterMapper;
using MediatR;
using System.Text.RegularExpressions;
using RapidFuzz.Net;
using Domain.Logs;
using Serilog;

namespace Application.Features.FeaturesCorrespondent
{
    public class UpdateCorrespondent
    {
        public sealed record Command : IRequest<Correspondent>
        {
            public readonly Guid CorrespondentId;
            public readonly CorrespondentDto Correspondentupdate;

            public Command(CorrespondentDto correspondenttoupdate, Guid id)
            {
                Correspondentupdate = correspondenttoupdate;
                CorrespondentId = id;

            }
        }
        public sealed class Handler : IRequestHandler<Command, Correspondent>
        {
            private readonly IDocumentRepository _documentrepository;
            private readonly ICorrespondentRepository _repository;
            private readonly IMapper _mapper;
            private readonly ILogRepository _logRepository;

            public Handler(ICorrespondentRepository repository, IMapper mapper, IDocumentRepository documentrepository, ILogRepository logRepository)
            {
                _repository = repository;
                _mapper = mapper;
                _documentrepository = documentrepository;
                _logRepository = logRepository;
            }
            public async Task<Correspondent> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {


                    Logs new_correspondent = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $" Correspondent updated {request.Correspondentupdate.Name}");
                    await _logRepository.AddAsync(new_correspondent);

                    Correspondent oldcorrespondent = new Correspondent();
                    Correspondent correspondenttoupdate = _repository.FindByIdAsync(request.CorrespondentId, cancellationToken).GetAwaiter().GetResult();
                    oldcorrespondent.Matching_algorithm = correspondenttoupdate.Matching_algorithm;

                    if (correspondenttoupdate.UsersView != null || correspondenttoupdate.GroupsView != null ||
                     correspondenttoupdate.UsersChange != null || correspondenttoupdate.GroupsChange != null)
                    {
                        correspondenttoupdate.UsersView?.Clear();
                        correspondenttoupdate.GroupsView?.Clear();
                        correspondenttoupdate.UsersChange?.Clear();
                        correspondenttoupdate.GroupsChange?.Clear();
                    }


                    _mapper.Map(request.Correspondentupdate, correspondenttoupdate);
                    correspondenttoupdate.Slug = request.Correspondentupdate.Name;
                    if (request.Correspondentupdate.Set_permissions?.View?.Users != null)
                    {
                        if (correspondenttoupdate.UsersView == null)
                        {
                            correspondenttoupdate.UsersView = new List<string>();
                        }
                        foreach (var user in request.Correspondentupdate.Set_permissions.View.Users)
                        {
                            if (!correspondenttoupdate.UsersView.Contains(user))
                            {
                                correspondenttoupdate.UsersView.Add(user);
                            }

                        }

                    }
                    if (request.Correspondentupdate.Set_permissions?.View?.Groups != null)
                    {
                        if (correspondenttoupdate.GroupsView == null)
                        {
                            correspondenttoupdate.GroupsView = new List<string>();
                        }
                        foreach (var group in request.Correspondentupdate.Set_permissions.View.Groups)
                        {
                            if (!correspondenttoupdate.GroupsView.Contains(group))
                            {
                                correspondenttoupdate.GroupsView.Add(group);
                            }

                        }
                    }
                    if (request.Correspondentupdate.Set_permissions?.Change?.Users != null)
                    {
                        if (correspondenttoupdate.UsersChange == null)
                        {
                            correspondenttoupdate.UsersChange = new List<string>();
                        }
                        foreach (var user in request.Correspondentupdate.Set_permissions.Change.Users)
                        {
                            if (!correspondenttoupdate.UsersChange.Contains(user))
                            {
                                correspondenttoupdate.UsersChange.Add(user);
                            }

                        }
                    }
                    if (request.Correspondentupdate.Set_permissions?.Change?.Groups != null)
                    {
                        if (correspondenttoupdate.GroupsChange == null)
                        {
                            correspondenttoupdate.GroupsChange = new List<string>();
                        }

                        foreach (var group in request.Correspondentupdate.Set_permissions.Change.Groups)
                        {
                            if (!correspondenttoupdate.GroupsChange.Contains(group))
                            {
                                correspondenttoupdate.GroupsChange.Add(group);
                            }
                        }
                    }
                    await _repository.UpdateAsync(correspondenttoupdate);


                    List<Document> all_documents = (List<Document>)await _documentrepository.GetAllAsync();
                    if (correspondenttoupdate.Matching_algorithm == Matching_Algorithms.MATCH_NONE)
                    {
                        foreach (Document document in all_documents)
                        {
                            if (document.CorrespondentId == correspondenttoupdate.Id)
                            {
                                document.CorrespondentId = null;
                                document.Correspondent = null;
                                await _documentrepository.UpdateAsync(document);

                            }
                        }
                    }
                    else if (correspondenttoupdate.Matching_algorithm == Matching_Algorithms.MATCH_ANY)
                    {
                        if (oldcorrespondent.Matching_algorithm != Matching_Algorithms.MATCH_NONE)
                        {
                            foreach (Document document in all_documents)
                            {


                                if (document.CorrespondentId == correspondenttoupdate.Id)
                                {
                                    document.CorrespondentId = null;
                                    document.Correspondent = null;
                                    await _documentrepository.UpdateAsync(document);
                                }


                            }
                        }
                        if (correspondenttoupdate.Match != null && correspondenttoupdate.Match.Count > 0)
                        {
                            foreach (Document document in all_documents)
                            {
                                if (document.CorrespondentId == null)
                                {
                                    try
                                    {

                                        if (correspondenttoupdate.Is_insensitive == true)
                                        {
                                            var lowerCaseMatch = correspondenttoupdate.Match.Select(x => x.ToLower()).ToList();
                                            bool allConditionsMet = lowerCaseMatch.Any(x => document.Content.ToLower().Contains(x));
                                            Console.WriteLine(allConditionsMet);

                                            if (allConditionsMet)
                                            {
                                                document.CorrespondentId = correspondenttoupdate.Id;
                                                await _documentrepository.UpdateAsync(document);
                                            }
                                        }
                                        else
                                        {
                                            bool allConditionsMet = correspondenttoupdate.Match.Any(x => document.Content.ToLower().Contains(x));


                                            Console.WriteLine(allConditionsMet);

                                            if (allConditionsMet)
                                            {
                                                document.CorrespondentId = correspondenttoupdate.Id;
                                                await _documentrepository.UpdateAsync(document);
                                            }

                                        }


                                    }
                                    catch (Exception ex)
                                    {

                                        Console.WriteLine($"Exception: {ex.Message}");
                                    }
                                }

                            }
                        }


                    }
                    else if (correspondenttoupdate.Matching_algorithm == Matching_Algorithms.MATCH_ALL)
                    {
                        if (oldcorrespondent.Matching_algorithm != Matching_Algorithms.MATCH_NONE)
                        {
                            foreach (Document document in all_documents)
                            {


                                if (document.CorrespondentId == correspondenttoupdate.Id)
                                {
                                    document.CorrespondentId = null;
                                    document.Correspondent = null;
                                    await _documentrepository.UpdateAsync(document);
                                }


                            }
                        }
                        if (correspondenttoupdate.Match != null && correspondenttoupdate.Match.Count > 0)
                        {
                            foreach (Document document in all_documents)
                            {

                                if (document.CorrespondentId == null)
                                {
                                    if (correspondenttoupdate.Is_insensitive == true)
                                    {
                                        var lowerCaseMatch = correspondenttoupdate.Match.Select(x => x.ToLower()).ToList();
                                        bool allConditionsMet = lowerCaseMatch.Any(x => document.Content.ToLower().Contains(x));
                                        Console.WriteLine(allConditionsMet);

                                        if (allConditionsMet)
                                        {
                                            document.CorrespondentId = correspondenttoupdate.Id;
                                            await _documentrepository.UpdateAsync(document);
                                        }
                                    }
                                    else
                                    {
                                        bool allConditionsMet = correspondenttoupdate.Match.All(x => document.Content.ToLower().Contains(x));


                                        Console.WriteLine(allConditionsMet);

                                        if (allConditionsMet)
                                        {
                                            document.CorrespondentId = correspondenttoupdate.Id;
                                            await _documentrepository.UpdateAsync(document);
                                        }

                                    }
                                }


                            }
                        }
                    }
                    else if (correspondenttoupdate.Matching_algorithm == Matching_Algorithms.MATCH_LITERAL)
                    {

                        //in case it's not none because none has no effect on the document but other algos do (there is no storagepathids to delete)
                        if (oldcorrespondent.Matching_algorithm != Matching_Algorithms.MATCH_NONE)
                        {

                            foreach (Document document in all_documents)
                            {


                                if (document.CorrespondentId == correspondenttoupdate.Id)
                                {
                                    document.CorrespondentId = null;
                                    document.Correspondent = null;
                                    await _documentrepository.UpdateAsync(document);
                                }


                            }
                        }
                        if (correspondenttoupdate.Match != null && correspondenttoupdate.Match.Count > 0)
                        {
                            foreach (Document document in all_documents)
                            {
                                if (document.CorrespondentId == null)
                                {
                                    try
                                    {
                                        if (correspondenttoupdate.Is_insensitive == true)
                                        {

                                            if (correspondenttoupdate.Match.Count() == 1 && document.Content.ToLower().Contains(correspondenttoupdate.Match[0].ToLower()))
                                            {
                                                document.CorrespondentId = correspondenttoupdate.Id;
                                                await _documentrepository.UpdateAsync(document);
                                            }
                                        }
                                        else
                                        {
                                            if (correspondenttoupdate.Match.Count() == 1 && document.Content.ToLower().Contains(correspondenttoupdate.Match[0]))
                                            {
                                                document.CorrespondentId = correspondenttoupdate.Id;
                                                await _documentrepository.UpdateAsync(document);
                                            }

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // Log the exception details
                                        Console.WriteLine($"Exception: {ex.Message}");
                                    }
                                }

                            }

                        }
                    }
                    else if (correspondenttoupdate.Matching_algorithm == Matching_Algorithms.MATCH_REGEX)
                    {

                        //in case it's not none because none has no effect on the document but other algos do (there is no storagepathids to delete)
                        if (oldcorrespondent.Matching_algorithm != Matching_Algorithms.MATCH_NONE)
                        {
                            foreach (Document document in all_documents)
                            {


                                if (document.CorrespondentId == correspondenttoupdate.Id)
                                {
                                    document.CorrespondentId = null;
                                    document.Correspondent = null;
                                    await _documentrepository.UpdateAsync(document);
                                }


                            }
                        }
                        if (correspondenttoupdate.Match != null && correspondenttoupdate.Match.Count > 0)
                        {
                            foreach (Document document in all_documents)
                            {
                                if (document.CorrespondentId == null)
                                {
                                    try
                                    {

                                        if (correspondenttoupdate.Is_insensitive == true)
                                        {
                                            foreach (string regexPattern in correspondenttoupdate.Match.Select(x => x.ToLower()).ToList())
                                            {
                                                if (Regex.IsMatch(document.Content, regexPattern))
                                                {
                                                    document.CorrespondentId = correspondenttoupdate.Id;
                                                    await _documentrepository.UpdateAsync(document);
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (string regexPattern in correspondenttoupdate.Match)
                                            {
                                                if (Regex.IsMatch(document.Content, regexPattern))
                                                {
                                                    document.CorrespondentId = correspondenttoupdate.Id;
                                                    await _documentrepository.UpdateAsync(document);
                                                    break;
                                                }
                                            }

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // Log the exception details
                                        Console.WriteLine($"Exception: {ex.Message}");
                                    }
                                }

                            }
                        }
                    }
                    else if (correspondenttoupdate.Matching_algorithm == Matching_Algorithms.MATCH_FUZZY)
                    {

                        //in case it's not none because none has no effect on the document but other algos do (there is no storagepathids to delete)
                        if (oldcorrespondent.Matching_algorithm != Matching_Algorithms.MATCH_NONE)
                        {
                            foreach (Document document in all_documents)
                            {


                                if (document.CorrespondentId == correspondenttoupdate.Id)
                                {
                                    document.CorrespondentId = null;
                                    document.Correspondent = null;
                                    await _documentrepository.UpdateAsync(document);
                                }


                            }
                        }

                        int threshold = 90;
                        if (correspondenttoupdate.Match != null && correspondenttoupdate.Match.Count > 0)
                        {
                            foreach (Document document in all_documents)
                            {
                                if (document.CorrespondentId == null)
                                {
                                    try
                                    {
                                        if (correspondenttoupdate.Is_insensitive == true)
                                        {
                                            foreach (string matchWord in correspondenttoupdate.Match.Select(x => x.ToLower()).ToList())
                                            {
                                                Console.WriteLine("test " + FuzzyMatcher.PartialRatio(matchWord, document.Content));
                                                if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                                {
                                                    document.CorrespondentId = correspondenttoupdate.Id;
                                                    await _documentrepository.UpdateAsync(document);
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (string matchWord in correspondenttoupdate.Match)
                                            {
                                                Console.WriteLine("test " + FuzzyMatcher.PartialRatio(matchWord, document.Content));
                                                if (FuzzyMatcher.PartialRatio(matchWord, document.Content) >= threshold)
                                                {
                                                    document.CorrespondentId = correspondenttoupdate.Id;
                                                    await _documentrepository.UpdateAsync(document);
                                                    break;
                                                }
                                            }
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // Log the exception details
                                        Console.WriteLine($"Exception: {ex.Message}");
                                    }
                                }
                            }
                        }


                    }

                    return correspondenttoupdate;
                }
                catch(Exception ex)
                {
                    Log.Error(ex.Message);
                    throw new Exception(ex.Message);
                }



            }

        }
    }
}


