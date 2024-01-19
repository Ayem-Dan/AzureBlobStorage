using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class BlobStorageController : ControllerBase
{
    private readonly BlobStorageService _blobStorageService;

    public BlobStorageController(BlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, string containerName)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is not selected");
        }

        var filePath = Path.GetTempFileName();

        using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        await _blobStorageService.UploadFileAsync(containerName, filePath);

        System.IO.File.Delete(filePath);

        return Ok($"File: {file.FileName} has been uploaded successfully.");
    }

    [HttpGet("download/{containerName}/{fileName}")]
    public async Task<IActionResult> DownloadFile(string containerName, string fileName)
    {
        var downloadFilePath = Path.GetTempFileName();
        await _blobStorageService.DownloadFileAsync(containerName, fileName, downloadFilePath);

        var memory = new MemoryStream();
        using (var stream = new FileStream(downloadFilePath, FileMode.Open))
        {
            await stream.CopyToAsync(memory);
        }
        memory.Position = 0;

        System.IO.File.Delete(downloadFilePath);

        return File(memory, GetContentType(downloadFilePath), fileName);
    }

    private string GetContentType(string path)
    {
        var types = GetMimeTypes();
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return types[ext];
    }

    private Dictionary<string, string> GetMimeTypes()
    {
        return new Dictionary<string, string>
        {
            {".txt", "text/plain"},
            {".pdf", "application/pdf"},
            {".doc", "application/vnd.ms-word"},
            {".docx", "application/vnd.ms-word"},
            {".xls", "application/vnd.ms-excel"},
            {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            // Add more mappings here
        };
    }
}