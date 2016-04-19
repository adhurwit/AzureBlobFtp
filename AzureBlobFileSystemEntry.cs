using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.Generic;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdamHurwitz.FtpServer.FileSystem.AzureBlob
{
    public class AzureBlobFileSystemEntry : IUnixFileSystemEntry
    {
        public AzureBlobFileSystemEntry(AzureBlobFileSystem fileSystem, IListBlobItem item)
        {
            FileSystem = fileSystem;
            Item = item;
            if (Item.GetType().Name == "CloudBlobDirectory")
                IsFolder  = true;
            else
                IsFolder  = false;

            Permissions = new GenericUnixPermissions(
                new GenericAccessMode(true, true, IsFolder),
                new GenericAccessMode(true, true, IsFolder),
                new GenericAccessMode(true, true, IsFolder));
        }

        public bool IsFolder { get; }
        public IListBlobItem Item { get; }
        public IUnixFileSystem FileSystem { get; }
        public string Group => "group";
        public long NumberOfLinks => 1;
        public string Owner => "owner";
        public IUnixPermissions Permissions { get; }

        public DateTimeOffset? CreatedTime
        {
            get
            {
                if (IsFolder)
                    return null;
                else
                    return ((CloudBlockBlob)Item).Properties.LastModified;
            }
        }

        public DateTimeOffset? LastWriteTime
        {
            get
            {
                if (IsFolder)
                    return null;
                else
                    return ((CloudBlockBlob)Item).Properties.LastModified;
            }
        }

        public string Name
        {
            get
            {
                if (IsFolder)
                {
                    var dir = (CloudBlobDirectory)Item;
                    return dir.Prefix.Replace(dir.Parent.Prefix,"").TrimEnd('/');
                }
                else
                {
                    var blob = (CloudBlockBlob)Item;
                    return blob.Name.Replace(blob.Parent.Prefix, "");
                }
            }
        }

    }
}
