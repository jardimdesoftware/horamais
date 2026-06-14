using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Back.Application.Interfaces.Services;
using Back.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;

namespace Back.Infrastructure.Services;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;

    public S3FileStorageService(IOptions<FileStorageSettings> options)
    {
        var settings = options.Value;
        _bucketName = settings.BucketName;

        var credentials = new BasicAWSCredentials(settings.AccessKey, settings.SecretKey);
        var config = new AmazonS3Config
        {
            ServiceURL = settings.Endpoint,
            ForcePathStyle = true,  // obrigatório para MinIO
        };

        _s3 = new AmazonS3Client(credentials, config);
    }

    public async Task<string> UploadAsync(IFormFile file, string objectKey)
    {
        await EnsureBucketExistsAsync();

        using var stream = file.OpenReadStream();

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            InputStream = stream,
            ContentType = file.ContentType,
            AutoCloseStream = false
        };

        await _s3.PutObjectAsync(request);
        return objectKey;
    }

    public async Task<(Stream Content, string ContentType)> DownloadAsync(string objectKey)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey
        };

        var response = await _s3.GetObjectAsync(request);
        return (response.ResponseStream, response.Headers.ContentType);
    }

    public async Task DeleteAsync(string objectKey)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey
            };
            await _s3.DeleteObjectAsync(request);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Arquivo já não existe, sem ação necessária
        }
    }

    private async Task EnsureBucketExistsAsync()
    {
        var buckets = await _s3.ListBucketsAsync();
        if (buckets.Buckets == null || !buckets.Buckets.Exists(b => b.BucketName == _bucketName))
        {
            await _s3.PutBucketAsync(new PutBucketRequest
            {
                BucketName = _bucketName,
                UseClientRegion = true
            });
        }
    }
}
