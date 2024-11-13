using Application.Dtos.Correspondent;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using MediatR;
using Serilog;


namespace Application.Features.FeaturesCorrespondent
{
    public class FilterCorrespondentByName
    {
        public sealed record Query : IRequest<PagedList<CorrespondentListDTO>>
        {
            public CorrespondentParameters correspondentParameters;
            public readonly string Name;
            public readonly string Owner;
            public Query(string namefilter, CorrespondentParameters parameters, string owner)
            {
                Name = namefilter;
                correspondentParameters = parameters;
                Owner = owner;
            }
        }
        public class Handler : IRequestHandler<Query, PagedList<CorrespondentListDTO>>
        {
            private readonly ICorrespondentRepository _correspondentRepository;

            public Handler(ICorrespondentRepository correspondentRepository)
            {
                _correspondentRepository = correspondentRepository;
            }

            public async Task<PagedList<CorrespondentListDTO>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {

                    return await _correspondentRepository.GetCorrespondentsByNameAsync(request.correspondentParameters, request.Name, request.Owner);
                }


                catch (Exception ex)
                {
                    Log.Error($"Error Message : {ex.Message}");
                    throw new Exception(ex.Message, ex);
                }
            }
        }
    }
}
