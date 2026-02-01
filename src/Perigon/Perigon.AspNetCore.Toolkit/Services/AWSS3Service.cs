using Amazon.S3;
using Amazon.S3.Model;
using System.Net;

namespace Perigon.AspNetCore.Toolkit.Services;

public class AWSS3Service
{
    private readonly AmazonS3Client _client;
    private readonly AWSS3Option _options;
    public string BucketName { get; private set; }

    public AWSS3Service(IOptions<AWSS3Option> options)
    {
        _options = options.Value;
        if (
            _options.Endpoint.IsEmpty()
            || _options.AccessKeyId.IsEmpty()
            || _options.AccessKeySecret.IsEmpty()
        )
        {
            throw new ArgumentNullException(nameof(options), "AWSS3Option is null");
        }
        BucketName = _options.BucketName;

        _client = new AmazonS3Client(
            _options.AccessKeyId,
            _options.AccessKeySecret,
            new AmazonS3Config { ServiceURL = _options.Endpoint }
        );
    }

    public void SetBucketName(string bucketName)
    {
        BucketName = bucketName;
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="key"></param>
    /// <param name="stream"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<bool> UploadAsync(
        string key,
        Stream stream,
        CancellationToken token = default
    )
    {
        var request = new PutObjectRequest
        {
            BucketName = BucketName,
            Key = key,
            InputStream = stream,
        };
        var response = await _client.PutObjectAsync(request, token);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    /// <summary>
    /// 获取签名URL
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiresSeconds">有效期:秒</param>
    /// <returns></returns>
    public string GetSignedUrl(string key, int expiresSeconds = 600)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = BucketName,
            Key = key,
            Expires = DateTime.Now.AddSeconds(expiresSeconds),
        };
        return _client.GetPreSignedURL(request);
    }

    /// <summary>
    /// 删除对象
    /// </summary>
    /// <param name="key"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(
        string key,
        CancellationToken token = default
    )
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = BucketName,
                Key = key,
            };
            var response = await _client.DeleteObjectAsync(request, token);
            return response.HttpStatusCode == HttpStatusCode.OK 
                || response.HttpStatusCode == HttpStatusCode.NoContent;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 检查对象是否存在
    /// </summary>
    /// <param name="key"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<bool> ExistsAsync(
        string key,
        CancellationToken token = default
    )
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = BucketName,
                Key = key,
            };
            await _client.GetObjectAsync(request, token);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 获取对象
    /// </summary>
    /// <param name="key"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<GetObjectResponse?> GetObjectAsync(
        string key,
        CancellationToken token = default
    )
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = BucketName,
                Key = key,
            };
            return await _client.GetObjectAsync(request, token);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
