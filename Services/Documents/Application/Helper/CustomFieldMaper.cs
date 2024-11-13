using Domain.DocumentManagement.CustomFields;

namespace Application.Helper
{
    public static class CustomFieldMaper
    {
        public static TypeField HandleCustomFieldType(string dataType)
        {
            switch (dataType.ToLower())
            {
                case "string":
                    return TypeField.STRING;

                case "url":
                    return TypeField.URL;

                case "date":
                    return TypeField.DATE;

                case "boolean":
                    return TypeField.BOOLEAN;

                case "integer":
                    return TypeField.INTEGER;

                case "float":
                    return TypeField.FLOAT;

                case "monetary":
                    return TypeField.MONETARY;

                default:
                    return TypeField.UNKNOWN; // Handle unknown or unexpected types
            }
        }
    }
}
