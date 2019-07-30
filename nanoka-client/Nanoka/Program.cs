using System;
using System.Threading.Tasks;
using Nanoka.Core;
using Newtonsoft.Json;

namespace Nanoka
{
    static class Program
    {
        static async Task Main()
        {
            NanokaCore.Initialize();

            var options = await NanokaOptions.LoadAsync(JsonSerializer.CreateDefault());

            await NanokaCore.RunAsync(options);
        }
    }
}
