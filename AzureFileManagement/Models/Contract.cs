using System;
namespace AzureFileManagement.Models
{
    public class Contract
    {
        public string FileName { get; set; }
        public string DirectoryName { get; set; }
        public int Version { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
