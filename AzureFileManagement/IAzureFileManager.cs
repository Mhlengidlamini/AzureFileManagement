using AzureFileManagement.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureFileManagement
{
    public interface IAzureFileManager
    {
        Task UploadContractAsync(string directoryName, IFormFile file, int? version = null);
        Task<IActionResult> DownloadContractAsync(string directoryName, string fileName, int? version = null);
        Task<List<Contract>> GetContractVersionsAsync(string directoryName, string baseFileName);
        void StoreLog(string logCategory, string logFileName, string logContent);
        void ArchiveLog(string logCategory, string logFileName, string archiveDirectoryName);
    }
}
