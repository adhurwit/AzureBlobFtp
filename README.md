# Azure Blob file system for portable FTP server  #

This is a File System implementation to use Azure Blob Storage with [Fubar Development's FTP Server](https://github.com/FubarDevelopment/FtpServer). 

This library provides real FTP support for Azure Blob. Files and directories can be created, deleted, and moved. The experience in your FTP client will be the same as FTP to a standard file system. 


## Get Started ##

FubarDev's project contains a TestFtpServer that can be easily modified to try this code out. Simply use this line in place of the fsProvider 

	var abProvider = new AdamHurwitz.FileSystem.AzureBlob.AzureBlobFileSystemProvider("storage_account_name", "storage_account_key", "container_name");


And make sure that you use it in the FtpServer ctor

	using (var ftpServer = new FtpServer(abProvider, membershipProvider, "127.0.0.1", Port, commandFactory)

With these changes, you can actually run the TestFtpServer from a console on your dev machine and FTP to 127.0.0.1 with a standard FTP client. The TestFtpServer is currently set-up to take anonymous logins. User is "anonymous" and password is an email address. 


This library is available on NuGet. 


## How it works ##

Azure Blob is composed of three components: Account, Container, and Blobs. It does not explicitly support a folder structure, but it does support a virtual folder structure based on the use of slashes ("/") in the blob name. See the following article for more information - [Get started with Azure Blob storage using .NET](https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-blobs/)

This FTP file system implementation utilizes Azure Blob's virtual folder structure seamlessly for FTP clients. To support empty virtual directories, this library creates a dummy file named "\_\_\_dirholder\_\_\_.txt". This file will be visible in a storage explorer, but is hidden for an FTP client. This library also creates a top level folder called, FTPROOT. 



## Sample Code ##
This project should be considered as sample code created by Adam Hurwitz to demonstrate ways that you can use Azure. It is not intended for production use and is not an official Microsoft product. There is a License file included in the repository. 


## TODO ##
If you would like to move this forward, please get in touch. 




