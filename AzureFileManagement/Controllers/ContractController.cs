using AzureFileManagement; // Ensure this namespace is correct
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class ContractController : Controller
{
    private readonly IAzureFileManager _azureFileManager;

    public ContractController(IAzureFileManager azureFileManager)
    {
        _azureFileManager = azureFileManager;
    }

    // Action method for displaying the upload form
    [HttpGet]
    [Route("Contract/Upload")]
    public IActionResult Upload()
    {
        return View();
    }

    // Action method for processing the uploaded file
    [HttpPost]
    [Route("Contract/Upload")]
    public async Task<IActionResult> Upload(IFormFile file, int? version = null)
    {
        if (file != null && file.Length > 0)
        {
            await _azureFileManager.UploadContractAsync("Contracts", file, version);
            return RedirectToAction("Index");
        }
        return View();
    }

    // Action method for downloading a file
    [HttpGet]
    [Route("Contract/Download")]
    public async Task<IActionResult> Download(string directoryName, string fileName, int? version = null)
    {
        var result = await _azureFileManager.DownloadContractAsync(directoryName, fileName, version);
        return result;
    }

    // Action method for displaying contract versions
    [HttpGet]
    [Route("Contract/Versions")]
    public async Task<IActionResult> Versions(string baseFileName)
    {
        var versions = await _azureFileManager.GetContractVersionsAsync("Contracts", baseFileName);
        return View(versions);
    }

    // Action method for displaying the index page
    [HttpGet]
    [Route("Contract/Index")]
    public IActionResult Index()
    {
        return View();
    }
}

