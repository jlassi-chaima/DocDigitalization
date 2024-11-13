using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Application.Dtos.MailAccount;
using System.Security.Cryptography;
using System.Text;
using Application.Repository;
using Domain.MailAccounts;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MapsterMapper;
using MediatR;
using Application.Features;
using Newtonsoft.Json;
using Serilog;
using Domain.Ports;

namespace Application.Features.FeaturesMailAccount
{
    public class AddMailAccount
    {
        public sealed record Command : IRequest<MailAccount>
        {
            public readonly MailAccountDto MailAccount;

            public Command(MailAccountDto mailaccount)
            {
                MailAccount = mailaccount;
            }
        }

        public sealed class Handler : IRequestHandler<Command, MailAccount>
        {
            private readonly IMailAccountRepository _mailAccountRepository;
         
            private readonly IUserGroupPort _userGroupPort;


            public Handler(IMailAccountRepository mailAccountRepository, IUserGroupPort userGroupPort)
            {
                
                _mailAccountRepository = mailAccountRepository;
                _userGroupPort = userGroupPort;
            }
            public async Task<MailAccount> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Connect to mail server, retrieve messages, etc. (Existing code)

                    // Hash the password
                    string hashedPassword = CryptoHelper.EncodePasswordToBase64(request.MailAccount.Password);
                    Guid groupId = await GetGroupForUser(request.MailAccount.Owner ?? string.Empty);
                    // Create a new MailAccount instance
                    var mailAccount = new MailAccount
                    {
                        Name = request.MailAccount.Name,
                        IMAP_Server = request.MailAccount.IMAP_Server,
                        IMAP_Port = request.MailAccount.IMAP_Port,
                        IMAP_Security = request.MailAccount.IMAP_Security,
                        Username = request.MailAccount.Username,
                        Password = hashedPassword,
                        Character_set = request.MailAccount.Character_set,
                        GroupId = groupId,
                    };

                    // Add the MailAccount to the database
                    await _mailAccountRepository.AddAsync(mailAccount, cancellationToken);

                    return mailAccount;
                }

                catch (HttpRequestException ex)
                {
                    Log.Error(ex.ToString());
                    throw new HttpRequestException(ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    throw new Exception($"Exception: {ex.Message}");
                }
            }

            private async Task<Guid> GetGroupForUser(string idOwner)
            {
                try
                {
                    var res = await _userGroupPort.GetFirstGRoupForUser(idOwner);
                    var responseContent = await res.Content.ReadAsStringAsync();
                    if (res.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Log.Error($"Error Message : {responseContent}");
                        throw new HttpRequestException("An error has occured, please try again later");
                    }
                    var JSONObj = JsonConvert.DeserializeObject<Guid>(responseContent)!;
                    return JSONObj;
                }

                catch (HttpRequestException ex)
                {
                    Log.Error(ex.ToString());
                    throw new HttpRequestException(ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    throw new Exception($"Exception: {ex.Message}");
                }

            }


        }
    }
}
