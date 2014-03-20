using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using N2.Edit;
using N2.Edit.FileSystem;
using N2.Engine;

namespace N2.AzureFileSystem
{
    [Service(typeof(IFileSystem), Replaces = typeof(MappedFileSystem))]
    public class AzureFileSystemProvider : IFileSystem
    {
        private const string DIRECTORY_PLACE_HOLDER = "dir";

        private static readonly string CONTAINER_NAME = ContainerNameParser.Create();
        private static readonly string CONNECTION_STRING = ConfigurationManager.ConnectionStrings[AppSettingsKeys.N2StorageConnectionString].ConnectionString;

        private CloudBlobContainer container;

        private CloudBlobContainer Container
        {
            get
            {
                if (container == null)
                {
                    var storageAccount = CloudStorageAccount.Parse(CONNECTION_STRING);
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    container = blobClient.GetContainerReference(CONTAINER_NAME);
                    container.CreateIfNotExists(BlobContainerPublicAccessType.Container);
                }
                return container;
            }
        }

        public bool DirectoryExists(string path)
        {
            return ListItems(path).Any();
        }

        public void MoveDirectory(string fromVirtualPath, string destinationVirtualPath)
        {
            throw new NotImplementedException();

            //if (DirectoryMoved != null)
            //    DirectoryMoved.Invoke(this, new FileEventArgs(destinationVirtualPath, fromVirtualPath));
        }

        public void DeleteDirectory(string virtualPath)
        {
            GetCloudBlobDirectory(virtualPath);

            if (DirectoryDeleted != null)
                DirectoryDeleted.Invoke(this, new FileEventArgs(virtualPath, null));
        }

        public void CreateDirectory(string virtualPath)
        {
            var path = UnEscapedPath(virtualPath);
            Container.GetDirectoryReference(path);

            if (DirectoryCreated != null)
                DirectoryCreated.Invoke(this, new FileEventArgs(virtualPath, null));
        }

        public DirectoryData GetDirectory(string virtualPath)
        {
            var directory = GetCloudBlobDirectory(virtualPath);
            return GetDirectoryData(directory);
        }

        public void ReadFileContents(string virtualPath, Stream outputStream)
        {
            var blobFile = GetCloudBlockBlobFile(virtualPath);
            var bufferSize = blobFile.StreamMinimumReadSizeInBytes;

            using (var file = blobFile.OpenRead())
            {
                var buffer = new byte[bufferSize];
                int read;
                while ((read = file.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outputStream.Write(buffer, 0, read);
                }
            }
        }

        public bool FileExists(string path)
        {
            var blob = GetCloudBlockBlobFile(path);
            return blob.Exists();
        }

        public void MoveFile(string fromVirtualPath, string destinationVirtualPath)
        {
            CopyFileInternal(fromVirtualPath, destinationVirtualPath);
            DeleteFileInternal(fromVirtualPath);

            if (FileMoved != null)
                FileMoved.Invoke(this, new FileEventArgs(destinationVirtualPath, fromVirtualPath));
        }

        public void DeleteFile(string virtualPath)
        {
            DeleteFileInternal(virtualPath);

            if (FileDeleted != null)
                FileDeleted.Invoke(this, new FileEventArgs(virtualPath, null));
        }

        private void DeleteFileInternal(string virtualPath)
        {
            GetCloudBlockBlobFile(virtualPath).Delete();
        }

        public void CopyFile(string fromVirtualPath, string destinationVirtualPath)
        {
            CopyFileInternal(fromVirtualPath, destinationVirtualPath);

            if (FileCopied != null)
                FileCopied.Invoke(this, new FileEventArgs(destinationVirtualPath, fromVirtualPath));
        }

        private void CopyFileInternal(string fromVirtualPath, string destinationVirtualPath)
        {
            var from = GetCloudBlockBlobFile(fromVirtualPath);
            var destination = GetCloudBlockBlobFile(destinationVirtualPath);

            destination.StartCopyFromBlob(from);
        }

        public Stream OpenFile(string virtualPath, bool readOnly = false)
        {
            if (FileExists(virtualPath))
                return GetCloudBlockBlobFile(virtualPath).OpenRead();
            else if (!readOnly)
                return GetCloudBlockBlobFile(virtualPath).OpenWrite();
            else
                throw new Exception("Not found");
        }

        public void WriteFile(string virtualPath, Stream inputStream)
        {
            GetCloudBlockBlobFile(virtualPath).UploadFromStream(inputStream);

            if (FileWritten != null)
                FileWritten.Invoke(this, new FileEventArgs(virtualPath, null));
        }

        public event EventHandler<FileEventArgs> FileWritten;
        public event EventHandler<FileEventArgs> FileCopied;
        public event EventHandler<FileEventArgs> FileMoved;
        public event EventHandler<FileEventArgs> FileDeleted;
        public event EventHandler<FileEventArgs> DirectoryCreated;
        public event EventHandler<FileEventArgs> DirectoryMoved;
        public event EventHandler<FileEventArgs> DirectoryDeleted;


        public FileData GetFile(string path)
        {
            var blob = GetCloudBlockBlobFile(path);
            FileData fileData = null;

            if (blob.Exists())
            {
                blob.FetchAttributes();
                fileData = GetFileData(blob);
            }

            return fileData;
        }

        public IEnumerable<FileData> GetFiles(string path)
        {
            return ListItems(path)
                .Where(blob => blob is CloudBlockBlob)
                .Select(blob => GetFileData((CloudBlockBlob)blob))
                .Where(fileData => fileData.Name != DIRECTORY_PLACE_HOLDER);
        }

        public IEnumerable<DirectoryData> GetDirectories(string path)
        {
            return ListItems(path)
                .Where(blob => blob is CloudBlobDirectory)
                .Select(blob => GetDirectoryData((CloudBlobDirectory)blob));
        }

        private FileData GetFileData(ICloudBlob blob)
        {
            if (!blob.Exists())
            {
                return new FileData();
            }

            var lastModifiedDate = blob.Properties.LastModified;
            var lastModified = lastModifiedDate.HasValue ? lastModifiedDate.Value.LocalDateTime : DateTime.MinValue;

            return new FileData
            {
                Name = Path.GetFileName(blob.Name),
                Created = lastModified,
                Length = blob.Properties.Length,
                Updated = lastModified,
                VirtualPath = UnEscapedPath(blob.Name)
            };
        }

        private DirectoryData GetDirectoryData(CloudBlobDirectory directory)
        {
            var path = directory.Prefix;

            return new DirectoryData
            {
                Name = new DirectoryInfo(path).Name,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                VirtualPath = UnEscapedPath(path)
            };
        }

        public CloudBlockBlob GetCloudBlockBlobFile(string path)
        {
            var escapedPath = EscapedPath(path);
            return Container.GetBlockBlobReference(escapedPath);
        }

        public CloudBlobDirectory GetCloudBlobDirectory(string path)
        {
            var escapedPath = EscapedPath(path);
            return Container.GetDirectoryReference(escapedPath);
        }

        public IEnumerable<IListBlobItem> ListItems(string path)
        {
            return GetCloudBlobDirectory(path).ListBlobs();
        }

        private static string EscapedPath(string virtualPath)
        {
            if (virtualPath.StartsWith("~"))
            {
                virtualPath = virtualPath.Substring(1);
            }

            if (virtualPath.StartsWith("/"))
            {
                virtualPath = virtualPath.Substring(1);
            }

            return virtualPath.Replace("//", "/").ToLower().Trim();
        }

        public string UnEscapedPath(string path)
        {
            return string.Format("/{0}", path);
        }
    }
}