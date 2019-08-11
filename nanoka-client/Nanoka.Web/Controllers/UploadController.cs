using System;
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
    }
}