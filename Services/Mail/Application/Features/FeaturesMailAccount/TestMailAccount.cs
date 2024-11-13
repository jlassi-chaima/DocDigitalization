using Application.Dtos.MailAccount;
using MailKit.Net.Imap;
using MapsterMapper;
using MediatR;
using Application.Dtos.Test;
using Application.Repository;
using Domain.MailAccounts;

namespace Application.Features.FeaturesMailAccount
{
    public class TestMailAccount
    {
        public sealed record Command : IRequest<TestDto>
        {
            public readonly MailAccountDto MailAccount;

            public Command(MailAccountDto mailaccount)
            {
                MailAccount = mailaccount;
            }
        }

        public sealed class Handler : IRequestHandler<Command, TestDto>
        {
            private readonly ImapClient _client;
            private readonly IMailAccountRepository _repository;

            public Handler(ImapClient client, IMailAccountRepository repository)
            {
                _client = client;
                _repository = repository;
            }

            public async Task<TestDto> Handle(Command request, CancellationToken cancellationToken)
            {
                var result = new TestDto();
                MailAccount mail = _repository.FindByUsernameAsync(request.MailAccount.Username).Result;
                if (mail == null)
                {
                    _client.Connect(request.MailAccount.IMAP_Server, request.MailAccount.IMAP_Port, true);
                  
                    _client.Authenticate(request.MailAccount.Username, request.MailAccount.Password);
                    Console.WriteLine("Connected to the mail server successfully.");

                    result.Success = true;


                    if (_client.IsConnected)
                        _client.Disconnect(true);
                    Console.WriteLine("Disconnected from the mail server.");
                }
                else
                {
                    _client.Connect(request.MailAccount.IMAP_Server, request.MailAccount.IMAP_Port, true);
                    var passwrddecrypt = CryptoHelper.DecodeFrom64(request.MailAccount.Password);
                    _client.Authenticate(request.MailAccount.Username, passwrddecrypt);
                    Console.WriteLine("Connected to the mail server successfully.");

                    result.Success = true;


                    if (_client.IsConnected)
                        _client.Disconnect(true);
                    Console.WriteLine("Disconnected from the mail server.");
                }
                   
                

                return result;
            }

        }
    }
}
