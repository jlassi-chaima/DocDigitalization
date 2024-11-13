﻿namespace Application.Consumers.RestAPIDocuments.Dtos
{
    public class DocumentType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public List<string>? Match { get; set; }
        public Matching_Algorithms Matching_algorithm { get; set; } = Matching_Algorithms.MATCH_NONE;
        public bool Is_insensitive { get; set; } = true;
        public string Owner { get; set; }
        public int Document_count { get; set; } = 0;
    }
}
