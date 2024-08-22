using AzureFileManagement; // Adjust if the namespace is different
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly IAzureFileManager _azureFileManager;

    public HomeController(IAzureFileManager azureFileManager)
    {
        _azureFileManager = azureFileManager;
    }

    [HttpGet]
    public IActionResult UploadContract()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadContract(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            await _azureFileManager.UploadContractAsync("Contracts", file);
            return RedirectToAction("Index");
        }
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> DownloadContract(string fileName)
    {
        var result = await _azureFileManager.DownloadContractAsync("Contracts", fileName);
        return result;
    }

    [HttpPost]
    public IActionResult StoreLog(string logCategory, string logFileName, string logContent)
    {
        _azureFileManager.StoreLog(logCategory, logFileName, logContent);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult ArchiveLog(string logCategory, string logFileName, string archiveDirectoryName)
    {
        _azureFileManager.ArchiveLog(logCategory, logFileName, archiveDirectoryName);
        return RedirectToAction("Index");
    }

    public IActionResult Index()
    {
        return View();
    }
}
