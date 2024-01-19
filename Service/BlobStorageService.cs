using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;

public class BlobStorageService
{
    private readonly string connectionString;

    public BlobStorageService(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task UploadFileAsync(string containerName, string filePath)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        await containerClient.CreateIfNotExistsAsync();

        var fileName = Path.GetFileName(filePath);
        var blobClient = containerClient.GetBlobClient(fileName);

        using var uploadFileStream = File.OpenRead(filePath);
        await blobClient.UploadAsync(uploadFileStream, true);
        uploadFileStream.Close();

        Console.WriteLine($"Uploaded {fileName} to Blob storage.");
    }
    public async Task DownloadFileAsync(string containerName, string blobName, string downloadFilePath)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        BlobDownloadInfo download = await blobClient.DownloadAsync();

        using (var downloadFileStream = File.OpenWrite(downloadFilePath))
        {
            await download.Content.CopyToAsync(downloadFileStream);
            downloadFileStream.Close();
        }

        Console.WriteLine($"Downloaded {blobName} to {downloadFilePath}");
}}