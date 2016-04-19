using FubarDev.FtpServer.FileSystem;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdamHurwitz.FtpServer.FileSystem.AzureBlob
{
    public class AzureBlobFileSystemProvider : IFileSystemClassFactory
    {
        private readonly string _rootPath;

        private readonly bool _useUserIdAsSubFolder;
        
        /// <summary>
        /// Gets or sets a value indicating whether deletion of non-empty directories is allowed.
        /// </summary>
        public bool AllowNonEmptyDirectoryDelete { get; set; }

        public AzureBlobFileSystemProvider(string accountName, string accountKey, string container)
        {
            StorageCredentials creds = new StorageCredentials(accountName, accountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(creds, true);
            CloudBlobClient blob = storageAccount.CreateCloudBlobClient();
            Container = blob.GetContainerReference(container);
            Container.CreateIfNotExists();
        }

        private CloudBlobContainer Container;

        /// <inheritdoc/>
        public Task<IUnixFileSystem> Create(string userId, bool isAnonymous)
        {
            var path = _rootPath;
            if (_useUserIdAsSubFolder)
            {
                if (isAnonymous)
                    userId = "anonymous";
                path = Path.Combine(path, userId);
            }

            return Task.FromResult<IUnixFileSystem>(new AzureBlobFileSystem(Container, userId));
        }
    }
}
