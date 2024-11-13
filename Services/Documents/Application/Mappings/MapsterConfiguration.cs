using Application.Dtos.Correspondent;
using Application.Dtos.CustomField;
using Application.Dtos.DocumentNote;
using Application.Dtos.Documents;
using Application.Dtos.DocumentType;
using Application.Dtos.Tag;
using Application.Dtos.Templates;
using Aspose.Pdf.Drawing;
using Aspose.Pdf.Forms;
using Domain.DocumentManagement.CustomFields;
using Domain.DocumentManagement.DocumentNote;
using Domain.Documents;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappings
{
    public class MapsterConfiguration
    {
        public static void Configure()
        {
            

            
            TypeAdapterConfig<Document, DocumentUpdate>.NewConfig()
           .Map(dest => dest.Tags, src => src.Tags.Select(t => t.TagId).ToList())
            .Map(dest => dest.set_permissions.View.Users, src => src.UsersView)
            .Map(dest => dest.set_permissions.View.Groups, src => src.GroupsView)
            .Map(dest => dest.set_permissions.Change.Users, src => src.UsersChange)
            .Map(dest => dest.set_permissions.Change.Groups, src => src.GroupsChange);

            
            TypeAdapterConfig<Document, DocumentDetailsDTO>
            .NewConfig()
                .Map(dest=>dest.Archive_serial_number , src => src.Archive_Serial_Number)
                .Map(dest => dest.Tags, src => src.Tags.Select(t => t.TagId))
                .Map(dest => dest.Custom_fields, src => src.DocumentsCustomFields.Select(dc => new DocumentCustomFieldDTO
                {
                    Field = dc.CustomFieldId,
                    Value = dc.Value
                }))
                .Map(dest => dest.Tags, src => src.Tags.Select(t => t.TagId).ToList())
             .Map(dest => dest.Permissions.View.Users, src => src.UsersView)
             .Map(dest => dest.Permissions.View.Groups, src => src.GroupsView)
             .Map(dest => dest.Permissions.Change.Users, src => src.UsersChange)
             .Map(dest => dest.Permissions.Change.Groups, src => src.GroupsChange)
             .Map(dest => dest.Archive_serial_number, src => src.Archive_Serial_Number)
             .Map(dest => dest.CreatedOn, src => src.CreatedOn)
                .Map(dest => dest.Notes, src => src.Notes.Select(note => new DocumentNoteDto
                {
                    Note = note.Note,
                    Created = note.CreatedAt,
                    User = note.User,
                    Id = note.Id
                }));
        }
    }
}
