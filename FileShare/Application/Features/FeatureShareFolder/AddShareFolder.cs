using Application.Dtos.ShareFolder;
using Application.Exceptions;
using Application.Repository;
using Domain.FileShare;
using Domain.Ports;
using MediatR;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;

namespace Application.Features.FeatureShareFolder
{
    public class AddShareFolder
    {
        public sealed record Command : IRequest<ShareFolder>
        {
            public readonly ShareFolderDto ShareFolder;

            public Command(ShareFolderDto shareFolder)
            {
                ShareFolder = shareFolder;
            }
        }

        public sealed class Handler : IRequestHandler<Command, ShareFolder>
        {
            private readonly IShareFolderRepository _shareFolderRepository;
            private readonly IUserGroupPort _userGroupPort;
            public Handler(IShareFolderRepository shareFolderRepository, IUserGroupPort userGroupPort)
            {
                _shareFolderRepository = shareFolderRepository;
                _userGroupPort = userGroupPort;
            }

            public async Task<ShareFolder> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {


                    bool folderExists = Directory.Exists(request.ShareFolder.FolderPath);

                    // Create the file share if the folder exists
                    if (folderExists)
                    {
                        // Create the file share
                        string commandText = $"net share {request.ShareFolder.ShareName}={request.ShareFolder.FolderPath} ";


                        try
                        {
                            //uses the command-line interface to share the folder over the network.
                            Process process = new Process();
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            startInfo.FileName = @"C:\Windows\System32\cmd.exe";
                            startInfo.Arguments = $"/c {commandText}"; // /c tells cmd to terminate after the command is executed
                            startInfo.Verb = "runas"; // Run as administrator if specified
                            startInfo.UseShellExecute = true;
                            process.StartInfo = startInfo;
                            process.Start();
                            await process.WaitForExitAsync(cancellationToken);
                            Guid groupId = await GetGroupForUser(request?.ShareFolder?.Owner??string.Empty);

                            var newShareFolder = new ShareFolder
                            {
                                FolderPath = request.ShareFolder.FolderPath,
                                ShareName = request.ShareFolder.ShareName,
                                Username = request.ShareFolder.Username,
                                Password = request.ShareFolder.Password,
                                Owner=request.ShareFolder.Owner,
                                GroupId=groupId,
                       
                                CreationTime = DateTime.UtcNow,
                            };
                            await _shareFolderRepository.AddAsync(newShareFolder);
                            return newShareFolder;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"An error occurred while executing the command: {ex.Message}");
                        }

                    }
                    else
                    {
                        Log.Error($"Folder '{request.ShareFolder.FolderPath}' does not exist.");
                        throw new ShareFolderException($"Folder '{request.ShareFolder.FolderPath}' does not exist.");
                    }


                }
                catch (ShareFolderException ex)
                {
                    Log.Error(ex.Message);
                    throw new Exception(ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error Message: {ex.Message}");
                    throw new Exception($"Error Message: {ex.Message}");
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
