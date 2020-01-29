using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aspnet.SlowApp.Controllers
{
    [ApiController]
    public class SlowController : ControllerBase
    {
        [HttpGet]
        [Route("compress")]
        public Task<IActionResult> Compress()
        {
            return Compress(10 * 1024 * 1024);
        }

        [HttpGet]
        [Route("compress/{byteCount}")]
        public async Task<IActionResult> Compress(int byteCount)
        {
            byte[] bytes = new byte[byteCount];

            using var randomGenerator = new RNGCryptoServiceProvider();
            randomGenerator.GetBytes(bytes);

            using var inputStream = new MemoryStream();
            await inputStream.WriteAsync(bytes, 0, bytes.Length);
            inputStream.Position = 0;

            using var outputStream = new MemoryStream();
            using (var zipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
            {
                await inputStream.CopyToAsync(zipStream);
            };

            var results = outputStream.ToArray();

            return Ok(new
            {
                hash = ComputeHash(results)
            });
        }


        static string ComputeHash(byte[] bytes)
        {
            using var hash = SHA1.Create();

            byte[] result = hash.ComputeHash(bytes);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                builder.Append(result[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}