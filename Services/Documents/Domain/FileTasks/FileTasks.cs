using Core.Domain;
using Domain.Documents;
using Domain.Templates.Enum;



namespace Domain.FileTasks
{
    public class FileTasks : BaseEntity
    {
        public DocumentSource Source { get; set; }
        public Problem Task_problem { get; set; }
        public Document Task_document { get; set; }

        public static FileTasks Create(
           DocumentSource source,
           Problem task_problem,
           Document task_document
           

          )
        {
            FileTasks filetask = new()
            {
                Source = source,
                Task_problem = task_problem,
                Task_document = task_document,
                

            };

           

            return filetask;
        }
    }
}
