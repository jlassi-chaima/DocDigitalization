using Domain.Templates.Enum;

namespace Application.Statistics.Model
{
    public sealed record DocumentsPerMonth(Dictionary<string, int> DocumentsCount)
    {
        // Factory method to create an instance
        public static DocumentsPerMonth Create(Dictionary<string, int> documentsCount) =>
            new DocumentsPerMonth(documentsCount);
    }
    public sealed record DocumentsSource(Dictionary<DocumentSource, int> DocumentsCount)
    {
        // Factory method to create an instance
        public static DocumentsSource Create(Dictionary<DocumentSource, int> documentsCount) =>
            new DocumentsSource(documentsCount);
    }

}
