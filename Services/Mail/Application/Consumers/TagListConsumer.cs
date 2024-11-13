using Bus;
using MassTransit;

namespace Application.Consumers
{
    public class TagListConsumer : IConsumer<TagListEvent>
    {
       public Task Consume(ConsumeContext<TagListEvent> context)
       {
        var tagListEvent = context.Message;

        // Traiter chaque tag dans l'événement
        
            // Loguer ou effectuer toute autre action appropriée avec chaque tag
            Console.WriteLine($"Received Tag: TagId={tagListEvent.TagId}, Name={tagListEvent.Name}");
        

        // Indiquer que la consommation de l'événement est terminée
        return Task.CompletedTask;
       }
    }
}
