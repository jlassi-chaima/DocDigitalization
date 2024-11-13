using Application.Respository;
using Domain.DocumentManagement.tags;
using Domain.Documents;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace Infrastructure
{
    public class BackgroundJob 
    {
        //private readonly IDocumentRepository _document;
        //private readonly ITagRepository _tagRepository;
        //private readonly DBContext _dbContext;
        //private readonly IServiceProvider _serviceProvider;

        //public BackgroundJob(IDocumentRepository document, ITagRepository tagRepository, DBContext dbContext, IServiceProvider serviceProvider)
        //{
        //    _document = document;
        //    _tagRepository = tagRepository;
        //    _dbContext = dbContext;
        //    _serviceProvider = serviceProvider;

        //}

        //public async Task Execute(IJobExecutionContext context)
        //{
        //    // Fetch all documents and tags
        //    List<Document> documents = (List<Document>)await _document.GetAllAsync();
        //    List<Tag> tags = (List<Tag>)await _tagRepository.GetAllAsync();


        //    foreach (Document document in documents)
        //    {
        //        if (document.Created == DateTime.Now)
        //        {
        //            foreach (Tag tag in tags)
        //            {
        //                foreach (string word in tag.Match)
        //                    //check word exist or not
        //                    if (!document.FileData.Contains(word))
        //                    {
        //                        Console.WriteLine("Word not found in document: " + word);
        //                        continue;

        //                    }
        //                    // check word
        //                    else if (document.FileData.Contains(word))
        //                    {
        //                        using (var scope = _serviceProvider.CreateScope())
        //                        {
        //                            var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
        //                            // Use dbContext here...
        //                            if (_dbContext.DocumentTags.Any(dt => dt.Document.Id == document.Id))
        //                                break;
        //                            // check document id not found

        //                            else
        //                            {

        //                                Console.WriteLine("en cours ..!");
        //                                // Create a new DocumentTags object
        //                                DocumentTags documentTag = new DocumentTags
        //                                {
        //                                    Document = document,
        //                                    DocumentId = document.Id,
        //                                    Tag = tag,
        //                                    TagId = tag.Id
        //                                };

        //                                if (document.Tags == null)
        //                                {
        //                                    document.Tags = new List<DocumentTags>();
        //                                }
        //                                document.Tags.Add(documentTag);
        //                                _document.UpdateAsync(document);
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        break;
        //                    }

        //            }
        //        }
        //        else
        //        {
        //            break;
        //        }


        //    }


        //}



    }
}

