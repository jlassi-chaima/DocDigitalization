
namespace Application.Statistics.Model
{
    public class Statistics
    {
        public int documents_total { get; set; }
        public int character_count { get; set; }
        public int tag_count { get; set; }
        public int correspondent_count { get; set; }
        public int document_type_count { get; set; }
        public int storage_path_count { get; set; }
        public int users_count { get; set; }
        public int groups_count { get; set; }

        public static Statistics Create
        (
            int documentsTotal,
            int characterCount,
            int tagCount,
            int correspondentCount,
            int documentTypeCount,
            int storagePathCount,
            int usersCount,
            int groupsCount
        )
        {
            Statistics statistics = new Statistics()
            {
                documents_total = documentsTotal,
                character_count = characterCount,
                tag_count = tagCount,
                correspondent_count = correspondentCount,
                document_type_count = documentTypeCount,
                storage_path_count = storagePathCount,
                users_count = usersCount,
                groups_count = groupsCount
            };
            return statistics;
        }
    }
}
