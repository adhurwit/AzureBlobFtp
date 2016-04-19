using FubarDev.FtpServer.FileSystem;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdamHurwitz.FileSystem.AzureBlob
{
    class AzureBlobFileEntry : AzureBlobFileSystemEntry, IUnixFileEntry
    {
        public AzureBlobFileEntry(AzureBlobFileSystem fileSystem, CloudBlockBlob item, long? fileSize)
            : base(fileSystem, item)
        {
            Size = fileSize ?? (long?)item.Properties.Length ?? 0;
        }

        public long Size { get; }
    }
}
