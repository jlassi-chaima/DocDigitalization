using Application.Dtos.MailAccount;
using Application.Repository;
using Domain.MailAccounts;
using MapsterMapper;
using MediatR;


namespace Application.Features.FeaturesMailAccount
{
    public class UpdateMailAccount
    {
        public sealed record Command : IRequest<MailAccount>
        {
            public readonly Guid MailAccountId;
            public readonly MailAccountDto MailAccountupdate;

            public Command(MailAccountDto mailaccountupdate, Guid id)
            {
                MailAccountupdate = mailaccountupdate;
                MailAccountId = id;
            }
        }

        public sealed class Handler : IRequestHandler<Command, MailAccount>
        {
            private readonly IMailAccountRepository _repository;
            private readonly IMapper _mapper;
            public Handler(IMailAccountRepository repository, IMapper mapper)
            {
                _repository = repository;
                _mapper = mapper;

            }

            public async Task<MailAccount> Handle(Command request, CancellationToken cancellationToken)
            {

                MailAccount mailaccounttypetoupdate = _repository.FindByIdAsync(request.MailAccountId, cancellationToken).GetAwaiter().GetResult();

                _mapper.Map(request.MailAccountupdate, mailaccounttypetoupdate);
                await _repository.UpdateAsync(mailaccounttypetoupdate);
                return mailaccounttypetoupdate;

            }
        }
    }
}