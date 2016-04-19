using FubarDev.FtpServer.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AdamHurwitz.FileSystem.AzureBlob
{
    public class AzureBlobFileSystem : IUnixFileSystem
    {
        string DirectoryHolderFileName = "___dirholder___.txt";
        string DirectoryHolderText = "This is just a placeholder for the directory.";
        public AzureBlobFileSystem(CloudBlobContainer container, string userId)
        {
            Container = container;
            CreateAzureBlobDirectory("FTPROOT");
            Root = new AzureBlobDirectoryEntry(this, Container.GetDirectoryReference("FTPROOT"), true);
        }

        void CreateAzureBlobDirectory(string name)
        {
            CloudBlockBlob dirblock = Container.GetBlockBlobReference(name + Delimiter + DirectoryHolderFileName);
            dirblock.UploadText(DirectoryHolderText);
        }

        private CloudBlobContainer Container;
        public bool SupportsNonEmptyDirectoryDelete { get { return true; } }
        public IUnixDirectoryEntry Root { get; }
        public bool SupportsAppend => false;
        public StringComparer FileSystemEntryComparer => StringComparer.OrdinalIgnoreCase;
        string Delimiter = "/";


        public void Dispose()
        {
            Dispose(true);
        }
        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // Nothing to dispose
                }
                _disposedValue = true;
            }
        }


        public async Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(IUnixDirectoryEntry directoryEntry, CancellationToken cancellationToken)
        {
            var dir = (CloudBlobDirectory)((AzureBlobDirectoryEntry)directoryEntry).Item;

            var result = new List<IUnixFileSystemEntry>();
            foreach (IListBlobItem item in dir.ListBlobs())
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    // hide the directory holder file from the client
                    if (blob.Name.Replace(blob.Parent.Prefix, "") == DirectoryHolderFileName)
                        continue;
                    result.Add(new AzureBlobFileEntry(this, blob, blob.Properties.Length));
                }
                else if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory directory = (CloudBlobDirectory)item;
                    result.Add(new AzureBlobDirectoryEntry(this, directory, false));
                }
            }
            return result;
        }

        public async Task<IUnixFileSystemEntry> GetEntryByNameAsync(IUnixDirectoryEntry directoryEntry, string name, CancellationToken cancellationToken)
        {
            var dir = (CloudBlobDirectory)((AzureBlobDirectoryEntry)directoryEntry).Item;
            foreach (IListBlobItem item in dir.ListBlobs())
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    var abf = new AzureBlobFileEntry(this, blob, blob.Properties.Length);
                    if (abf.Name == name)
                        return (IUnixFileSystemEntry)abf;
                }
                else if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory directory = (CloudBlobDirectory)item;
                    var abd = new AzureBlobDirectoryEntry(this, directory, false);
                    if(abd.Name == name)
                        return (IUnixFileSystemEntry)abd;
                }
            }
            return null;

        }

        public async Task<IUnixFileSystemEntry> MoveAsync(IUnixDirectoryEntry parent, IUnixFileSystemEntry source, IUnixDirectoryEntry target, string fileName, CancellationToken cancellationToken)
        {
            // just file first
            if (((AzureBlobFileSystemEntry)source).IsFolder)
                throw new NotImplementedException();

            var file = (CloudBlockBlob)((AzureBlobFileEntry)source).Item;

            var dir = (CloudBlobDirectory)((AzureBlobDirectoryEntry)target).Item;
            var blockblob = dir.GetBlockBlobReference(fileName);
            await blockblob.StartCopyAsync(file);

            while (blockblob.CopyState.Status == CopyStatus.Pending)
                await Task.Delay(100);

            if (blockblob.CopyState.Status != CopyStatus.Success)
                throw new ApplicationException("Move failed: " + blockblob.CopyState.Status);

            await file.DeleteAsync();

            return new AzureBlobFileEntry(this, blockblob, blockblob.Properties.Length);
        }

        public Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            if(((AzureBlobFileSystemEntry)entry).IsFolder)
            {
                var dir = (CloudBlobDirectory)((AzureBlobDirectoryEntry)entry).Item;
                foreach (IListBlobItem item in dir.ListBlobs(true))
                    ((CloudBlockBlob)item).Delete();
            }
            else
            {
                var file = (CloudBlockBlob)((AzureBlobFileEntry)entry).Item;
                file.Delete();
            }
            return Task.FromResult(0);
        }

        public async Task<IUnixDirectoryEntry> CreateDirectoryAsync(IUnixDirectoryEntry targetDirectory, string directoryName, CancellationToken cancellationToken)
        {
            var dir = (CloudBlobDirectory)((AzureBlobDirectoryEntry)targetDirectory).Item;
            CreateAzureBlobDirectory(dir.Prefix + directoryName);
            var newdir = dir.GetDirectoryReference(directoryName);
            return new AzureBlobDirectoryEntry(this, newdir, false);
        }

        public Task<Stream> OpenReadAsync(IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken)
        {
            var file = (CloudBlockBlob)((AzureBlobFileEntry)fileEntry).Item;
            
            return Task.FromResult<Stream>(file.OpenRead());
        }

        public async Task<IBackgroundTransfer> CreateAsync(IUnixDirectoryEntry targetDirectory, string fileName, Stream data, CancellationToken cancellationToken)
        {
            var dir = (CloudBlobDirectory)((AzureBlobDirectoryEntry)targetDirectory).Item;
            var blockblob = dir.GetBlockBlobReference(fileName);
            blockblob.UploadFromStream(data);

            return null;
        }

        public async Task<IBackgroundTransfer> ReplaceAsync(IUnixFileEntry fileEntry, Stream data, CancellationToken cancellationToken)
        {
            var file = (CloudBlockBlob)((AzureBlobFileEntry)fileEntry).Item;
            file.UploadFromStream(data);

            return null;
        }



        public Task<IUnixFileSystemEntry> SetMacTimeAsync(IUnixFileSystemEntry entry, DateTimeOffset? modify, DateTimeOffset? access, DateTimeOffset? create, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IBackgroundTransfer> AppendAsync(IUnixFileEntry fileEntry, long? startPosition, Stream data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


    }
}
