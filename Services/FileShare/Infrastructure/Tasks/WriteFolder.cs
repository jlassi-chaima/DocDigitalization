using Application.Repository;
using Domain.FileShare;
using Quartz;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Infrastructure.Tasks
{
    public class WriteFolder : IJob
    {
        private readonly IShareFolderRepository _shareFolderRepository;

        public WriteFolder(IShareFolderRepository shareFolderRepository)
        {
            _shareFolderRepository = shareFolderRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // Get all folders from the repository
            IEnumerable<ShareFolder> folders = await _shareFolderRepository.GetAllAsync();

            foreach (ShareFolder folder in folders)
            {
                try
                {
                    // Get all files in the current folder
                    IEnumerable<FileInfo> files = GetFilesInFolder(folder.FolderPath);

                    // Check for new files and update LastWriteTime
                    await CheckForNewFilesAndUpdateTime(folder, files);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing folder '{folder.FolderPath}': {ex.Message}");
                }
            }
        }
       
        private IEnumerable<FileInfo> GetFilesInFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                return new DirectoryInfo(folderPath).GetFiles();
            }
            else
            {
                throw new Exception($"Folder '{folderPath}' does not exist.");
            }
        }
        private async Task CheckForNewFilesAndUpdateTime(ShareFolder folder, IEnumerable<FileInfo> files)
        {
            List<string> usedfiles = new List<string>();
            foreach (FileInfo file in files)
            {
                // Get the previous LastWriteTime for the file from the repository
                //ShareFolder folder = await _shareFolderRepository.GetByFolderPathAsync(folder.FolderPath);

                // Check if filename doesn't contain "new_"
                if (!file.Name.Contains("check"))
                {


                    Console.WriteLine($"New file detected: {file.FullName}");
                    DateTime utcLastWriteTime = file.CreationTime.ToUniversalTime();
                    Console.WriteLine($"file added at time :{utcLastWriteTime}");

                    using (var formData = new MultipartFormDataContent())
                    {
                        string fileExtension = Path.GetExtension(file.Name);
                        string contentType = GetContentTypeByExtension(fileExtension);
                        // Add the file to the FormDataContent
                        using (var fileStream = File.OpenRead(file.FullName))
                        {
                            //formData.Add(new StreamContent(fileStream), "formData", file.Name);
                            var streamContent = new StreamContent(fileStream);
                            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                            formData.Add(streamContent, "formData", file.Name);
                           // formData.DefaultRequestHeaders.Add(new StringContent("FileShare"), "type");
                            // Determine the MIME type based on the file extension

                            // Make a POST request to the endpoint with the file content
                            using (var httpClient = new HttpClient
                            {
                                Timeout = TimeSpan.FromMinutes(5) // Set timeout to 5 minutes
                            })
                            {
                                httpClient.DefaultRequestHeaders.Add("Type", "FileShare");

                                var response = await httpClient.PostAsync($"http://localhost:5046/document/Upload?id={folder.Owner}", formData);
                                 var content = response.Content.ReadAsStringAsync();
                                if (response.IsSuccessStatusCode)
                                {
                                    usedfiles.Add(file.FullName);
                                    Console.WriteLine("File uploaded successfully.");
                                }
                                else
                                {
                                    Console.WriteLine($"Failed to upload file. Status code: {response.StatusCode}");
                                }
                            }
                        }
                    }
                }

                
            }
            Thread.Sleep(300);
           
            foreach (string filePath in usedfiles)
            {
                // Get the directory path and file name
                string directoryPath = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);

                // Generate the new file name
                string newFileName = "check " + fileName; 

                // Generate the new file path
                string newFilePath = Path.Combine(directoryPath, newFileName);

                // Rename the file
                File.Move(filePath, newFilePath);
            }
        }
        private static string GetContentTypeByExtension(string extension)
        {
            switch (extension.ToLower())
            {
                case ".pdf":
                    return "application/pdf";
                case ".png":
                    return "image/png";
                case ".jpg":
                    return "image/jpeg";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".txt":
                    return "text/plain";
                default:
                    return "application/octet-stream"; // Default for unknown types
            }
        }

    }
}
