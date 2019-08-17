using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nanoka.Core;
using Nanoka.Core.Models;

namespace Nanoka.Web.Controllers
{
    [ApiController]
    [Route("uploads")]
    public class UploadController : AuthorizedControllerBase
    {
        readonly UploadManager _uploadManager;

        public UploadController(UploadManager uploadManager)
        {
            _uploadManager = uploadManager;
        }

        [HttpGet("{workerId}")]
        public Result<UploadState> GetState(Guid workerId)
        {
            var worker = _uploadManager.FindWorker(workerId);

            if (worker == null)
                return Result.NotFound<UploadWorker>(workerId);

            return worker.CreateState();
        }

        [HttpGet("{workerId}/next")]
        public async Task<Result<UploadState>> GetNextStateAsync(Guid workerId)
        {
            var worker = _uploadManager.FindWorker(workerId);

            if (worker == null)
                return Result.NotFound<UploadWorker>(workerId);

            // timeout after 1 minute (circumvents CloudFlare 100 second 524 timeout)
            using (var timeoutSource = new CancellationTokenSource(TimeSpan.FromMinutes(1)))
            using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, HttpContext.RequestAborted))
            {
                try
                {
                    // wait for state change
                    return await worker.WaitForStateChangeAsync(linkedSource.Token);
                }
                catch (TaskCanceledException)
                {
                    // return current state after timeout
                    return worker.CreateState();
                }
            }
        }
    }
}