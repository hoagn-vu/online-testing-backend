using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;

namespace Backend_online_testing.Services
{
    public class S3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3Service(IConfiguration configuration)
        {
            var accessKey = configuration["AWS:AccessKey"];
            var secretKey = configuration["AWS:SecretKey"];
            var region = configuration["AWS:Region"];
            _bucketName = configuration["AWS:BucketName"];

            _s3Client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.GetBySystemName(region));
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);

            using (var stream = file.OpenReadStream())
            {
                var fileKey = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    Key = fileKey,
                    BucketName = _bucketName,
                    ContentType = file.ContentType,
                    //CannedACL = S3CannedACL.PublicRead
                };

                await fileTransferUtility.UploadAsync(uploadRequest);
                return $"https://{_bucketName}.s3-ap-southeast-2.amazonaws.com/{fileKey}";
            }
        }
    }
}
