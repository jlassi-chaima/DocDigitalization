using Application.Dtos.Correspondent;
using Application.Dtos.CustomField;
using Application.Dtos.DocumentNote;
using Application.Dtos.Documents;
using Application.Dtos.DocumentType;
using Application.Dtos.Permission;
using Application.Dtos.Tag;
using Application.Dtos.Templates;
using Domain.DocumentManagement.Correspondent;
using Domain.DocumentManagement.CustomFields;
using Domain.DocumentManagement.DocumentNote;
using Domain.DocumentManagement.DocumentTypes;
using Domain.DocumentManagement.tags;
using Domain.Documents;
using Domain.Templates;
using Mapster;

namespace Application.Mappings
{
    public class DocumentMappings : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Document, DocumentDto>();
            config.NewConfig<Document, DocumentDetailsDTO>()
             .Map(dest => dest.Tags, src => src.Tags.Select(t => t.TagId).ToList())
             .Map(dest => dest.Permissions.View.Users, src => src.UsersView)
             .Map(dest => dest.Permissions.View.Groups, src => src.GroupsView)
             .Map(dest => dest.Permissions.Change.Users, src => src.UsersChange)
             .Map(dest => dest.Permissions.Change.Groups, src => src.GroupsChange)
             .Map(dest => dest.Archive_serial_number, src => src.Archive_Serial_Number)
             .Map(dest => dest.CreatedOn, src => src.CreatedOn)
             .BeforeMapping((src, dest) => {
                 Console.WriteLine("Before mapping UsersView: " + string.Join(",", src.UsersView ?? new List<string>()));
             })
             .AfterMapping((src, dest) => {
                 Console.WriteLine("After mapping View Users: " + string.Join(",", dest.Permissions.View.Users ?? new List<string>()));
             });

            ;
            TypeAdapterConfig<Document, DocumentUpdate>.NewConfig()
           .Map(dest => dest.Tags, src => src.Tags.Select(t => t.TagId).ToList())
            .Map(dest => dest.set_permissions.View.Users, src => src.UsersView)
            .Map(dest => dest.set_permissions.View.Groups, src => src.GroupsView)
            .Map(dest => dest.set_permissions.Change.Users, src => src.UsersChange)
            .Map(dest => dest.set_permissions.Change.Groups, src => src.GroupsChange);

            config.NewConfig<Tag, TagDto>();
            config.NewConfig<Tag, TagDtoDetails>();
            config.NewConfig<DocumentTags, DocumentTagsDTO>();
            config.NewConfig<DocumentType, DocumentTypeDto>();
            config.NewConfig<CustomField, CustomFieldDto>();
            config.NewConfig<Correspondent, CorrespondentDto>();
            config.NewConfig<Correspondent, CorrespondentDetailDto>();
            config.NewConfig<DocumentNote, DocumentNoteDto>();
            config.NewConfig<Template, TemplateDto>();
        }
    }
}
