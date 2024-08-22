using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using AzureFileManagement.Models; // Ensure this namespace is correct
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure;
using AzureFileManagement;

public class AzureFileManager : IAzureFileManager
{
    private readonly ShareClient _contractShareClient;
    private readonly ShareClient _logShareClient;

    public AzureFileManager(IOptions<AzureStorageOptions> options)
    {
        var opts = options.Value;
        _contractShareClient = new ShareClient(opts.ConnectionString, opts.ContractFileShareName);
        _logShareClient = new ShareClient(opts.ConnectionString, opts.LogFileShareName);
    }

    public async Task UploadContractAsync(string directoryName, IFormFile file, int? version = null)
    {
        var directory = _contractShareClient.GetDirectoryClient(directoryName);
        await directory.CreateIfNotExistsAsync();

        string versionedFileName = version.HasValue
            ? $"{Path.GetFileNameWithoutExtension(file.FileName)}_v{version}{Path.GetExtension(file.FileName)}"
            : file.FileName;

        var fileClient = directory.GetFileClient(versionedFileName);

        using var stream = file.OpenReadStream();
        await fileClient.CreateAsync(stream.Length);
        await fileClient.UploadRangeAsync(new HttpRange(0, stream.Length), stream);
    }

    public async Task<IActionResult> DownloadContractAsync(string directoryName, string fileName, int? version = null)
    {
        var directory = _contractShareClient.GetDirectoryClient(directoryName);

        string versionedFileName = version.HasValue
            ? $"{Path.GetFileNameWithoutExtension(fileName)}_v{version}{Path.GetExtension(fileName)}"
            : fileName;

        var fileClient = directory.GetFileClient(versionedFileName);

        if (await fileClient.ExistsAsync())
        {
            var download = await fileClient.DownloadAsync();
            var stream = download.Value.Content;
            return new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = fileName
            };
        }

        return new NotFoundResult();
    }

    public async Task<List<Contract>> GetContractVersionsAsync(string directoryName, string baseFileName)
    {
        var directory = _contractShareClient.GetDirectoryClient(directoryName);
        var files = directory.GetFilesAndDirectoriesAsync(prefix: baseFileName);

        var contractVersions = new List<Contract>();
        await foreach (var file in files)
        {
            if (file.IsDirectory) continue;

            var version = ExtractVersion(file.Name);
            contractVersions.Add(new Contract
            {
                FileName = file.Name,
                DirectoryName = directoryName,
                Version = version,
                UploadedAt = file.Properties.LastModified?.DateTime ?? DateTime.MinValue
            });
        }

        return contractVersions;
    }

    private int ExtractVersion(string fileName)
    {
        var versionPart = Path.GetFileNameWithoutExtension(fileName)?.Split('_').LastOrDefault();
        if (int.TryParse(versionPart?.Replace("v", ""), out var version))
        {
            return version;
        }

        return 1; // Default version if none is found
    }

    // Implement the missing StoreLog method
    public void StoreLog(string logCategory, string logFileName, string logContent)
    {
        var directory = _logShareClient.GetDirectoryClient(logCategory);
        directory.CreateIfNotExists();

        var fileClient = directory.GetFileClient($"{logFileName}.txt");
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(logContent));
        fileClient.Create(stream.Length);
        fileClient.UploadRange(new HttpRange(0, stream.Length), stream);
    }

    // Implement the missing ArchiveLog method
    public void ArchiveLog(string logCategory, string logFileName, string archiveDirectoryName)
    {
        var sourceDirectory = _logShareClient.GetDirectoryClient(logCategory);
        var destinationDirectory = _logShareClient.GetDirectoryClient(archiveDirectoryName);
        destinationDirectory.CreateIfNotExists();

        var fileClient = sourceDirectory.GetFileClient($"{logFileName}.txt");
        if (fileClient.Exists())
        {
            var newFileClient = destinationDirectory.GetFileClient($"{logFileName}.txt");
            fileClient.StartCopy(newFileClient.Uri);
            fileClient.Delete();
        }
    }
}

