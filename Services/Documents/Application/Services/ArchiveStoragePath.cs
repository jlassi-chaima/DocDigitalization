using Application.Dtos.StoragePath;
using Domain.DocumentManagement.StoragePath;
using Domain.Documents;
using Microsoft.Extensions.Configuration;


namespace Application.Services
{
    public class ArchiveStoragePath
    {
        private readonly IConfiguration _configuration;
        public ArchiveStoragePath(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GetBaseDestinationPath()
        {
            return _configuration["ArchiveStoragePathSettings:StoragePathOutputFolder"];
        }


        public void addArchiveStoragePath(UpdateStoragePathDto storagepath, Document document)
        {
            string baseDestinationPath = GetBaseDestinationPath();
            // Copy PDF files to the destination folder
            string[] pathParts = storagepath.Path.Split('/', '-');

            // Create folder structure
            string currentPath = baseDestinationPath;
            foreach (string part in pathParts)
            {
                currentPath = Path.Combine(currentPath, part);
                Directory.CreateDirectory(currentPath);
            }

            // Destination folder for the PDF
            string fileName = Path.GetFileName(document.FileData);
            string destinationPath = Path.Combine(currentPath, fileName);

            // Copy the file
            File.Copy(document.FileData, destinationPath, true);
        }

      

        public void deleteArchiveStoragePath(StoragePath storagepath, Document document)
        {
            string baseDestinationPath = GetBaseDestinationPath();
            string name_of_file_to_delete = Path.GetFileName(document.FileData);
            string[] pathParts = storagepath.Path.Split('/', '-');

            // Combine all path parts 
            string path_to_delete = baseDestinationPath;
            foreach (string part in pathParts)
            {
                path_to_delete = Path.Combine(path_to_delete, part);
            }
            path_to_delete = Path.Combine(path_to_delete, name_of_file_to_delete);
            Console.WriteLine("path: " + path_to_delete);

            // Delete the file
            if (File.Exists(path_to_delete))
            {
                File.Delete(path_to_delete);
                Console.WriteLine("File deleted successfully.");

                // Check and delete empty parent folders recursively
                DeleteEmptyParentFolders(Path.GetDirectoryName(path_to_delete));
               
            }
            else
            {
                Console.WriteLine("File does not exist at the specified path.");
            }

        }

        private void DeleteEmptyParentFolders(string directoryPath)
        {
            if (directoryPath == null || directoryPath == GetBaseDestinationPath())
                return; // Base destination folder reached or invalid directory path

            if (IsDirectoryEmpty(directoryPath))
            {
                Directory.Delete(directoryPath);
                Console.WriteLine($"Folder '{directoryPath}' deleted successfully.");

                // Recursively check and delete parent folders
                DeleteEmptyParentFolders(Path.GetDirectoryName(directoryPath));
            }
        }

        public static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }


        public void updateArchiveStoragePath(StoragePath storagepath, Document document)
        {
            string baseDestinationPath = GetBaseDestinationPath();
            // Copy PDF files to the destination folder
            string[] pathParts = storagepath.Path.Split('/', '-');

            // Create folder structure
            string currentPath = baseDestinationPath;
            foreach (string part in pathParts)
            {
                currentPath = Path.Combine(currentPath, part);
                Directory.CreateDirectory(currentPath);
            }

            // Destination folder for the PDF
            string fileName = Path.GetFileName(document.FileData);
            string destinationPath = Path.Combine(currentPath, fileName);

            // Copy the file
            File.Copy(document.FileData, destinationPath, true);
        }
    }
}