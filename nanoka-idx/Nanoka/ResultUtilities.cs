using Microsoft.AspNetCore.Mvc;

namespace Nanoka
{
    public static class ResultUtilities
    {
        public static ActionResult NotFound<T>(params object[] id)
            => new NotFoundObjectResult($"{typeof(T).Name} '{string.Join('/', id)}' not found.");
    }
}