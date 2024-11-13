using Application.Dtos.ArchivenSerialNumber;
using Application.Features.FeaturesCustomField;
using Application.Parameters;
using Azure.Core;
using Core.Database;
using DD.Core.Pagination;
using Domain.Documents;


namespace Application.Respository
{
    public interface IArchiveSerialNumberRepository : IRepository<ArchiveSerialNumbers, Guid>
    {
        Task<PagedList<ArchiveSerialNumberDto>> GetArchiveSerialNumberNameAsync<ArchiveSerialNumberDto>(ArchiveSerialNumberParameters archiveSerialNumberParameters, string owner,string? nameIcontains);
        //Task<ArchiveSerialNumbers> GetArchiveNumberByGroupId(Guid groupId);
        Task<ArchiveSerialNumbers> GetArchiveNumberByGroupId(Guid groupId);

    }
}
