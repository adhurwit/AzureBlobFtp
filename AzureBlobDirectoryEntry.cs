using FubarDev.FtpServer.FileSystem;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdamHurwitz.FileSystem.AzureBlob
{
    public class AzureBlobDirectoryEntry : AzureBlobFileSystemEntry, IUnixDirectoryEntry
    {
        public AzureBlobDirectoryEntry(AzureBlobFileSystem fileSystem, CloudBlobDirectory directory, bool isRoot)
            : base(fileSystem, directory)
        {
            IsRoot = isRoot;

        }

        public bool IsDeletable
        {
            get
            {
                if (IsRoot)
                    return false;
                else
                    return true;
            }
        }

        public bool IsRoot { get; }

    }
}
