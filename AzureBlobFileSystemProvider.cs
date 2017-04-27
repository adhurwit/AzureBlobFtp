using System.Threading.Tasks;
using FubarDev.FtpServer.FileSystem;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AdamHurwitz.FtpServer.FileSystem.AzureBlob
{
    public class AzureBlobFileSystemProvider : IFileSystemClassFactory
    {
        private readonly CloudBlobContainer _container;
        private readonly string _rootPath = string.Empty;

        public AzureBlobFileSystemProvider(string accountName, string accountKey, string container, string rootFolder) : this(accountName, accountKey, container)
        {
            _rootPath = rootFolder;
        }

        public AzureBlobFileSystemProvider(string accountName, string accountKey, string container)
        {
            var creds = new StorageCredentials(accountName, accountKey);
            var storageAccount = new CloudStorageAccount(creds, true);
            var blob = storageAccount.CreateCloudBlobClient();

            _container = blob.GetContainerReference(container);
            _container.CreateIfNotExists();
        }

        /// <summary>
        ///     Gets or sets a value indicating whether deletion of non-empty directories is allowed.
        /// </summary>
        public bool AllowNonEmptyDirectoryDelete { get; set; }

        /// <inheritdoc />
        public Task<IUnixFileSystem> Create(string userId, bool isAnonymous)
        {
            if (_rootPath == string.Empty)
            {
                return Task.FromResult<IUnixFileSystem>(new AzureBlobFileSystem(_container));
            }
            ;

            return Task.FromResult<IUnixFileSystem>(new AzureBlobFileSystem(_container, _rootPath));
        }
    }
}