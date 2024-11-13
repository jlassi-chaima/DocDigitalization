using Application.Dtos.ShareFolder;
using Application.Repository;
using Domain.FileShare;
using MapsterMapper;
using MediatR;


namespace Application.Features.FeatureShareFolder
{
    public class UpdateShareFile
    {
        public sealed record Command : IRequest<ShareFolder>
        {
            public readonly Guid SharefolderId;
            public readonly ShareFolderDto Sharefolderupdate;

            public Command(ShareFolderDto mailRuleupdate, Guid id)
            {
                Sharefolderupdate = mailRuleupdate;
                SharefolderId = id;
            }
        }

        public sealed class Handler : IRequestHandler<Command, ShareFolder>
        {
            private readonly IShareFolderRepository _repository;
            private readonly IMapper _mapper;
            public Handler(IShareFolderRepository repository, IMapper mapper)
            {
                _repository = repository;
                _mapper = mapper;

            }

            public async Task<ShareFolder> Handle(Command request, CancellationToken cancellationToken)
            {
                ShareFolder sahrefiletoupdate = _repository.FindByIdAsync(request.SharefolderId, cancellationToken).GetAwaiter().GetResult();

                _mapper.Map(request.Sharefolderupdate, sahrefiletoupdate);
                await _repository.UpdateAsync(sahrefiletoupdate);
                return sahrefiletoupdate;

            }
        }
    }
}
