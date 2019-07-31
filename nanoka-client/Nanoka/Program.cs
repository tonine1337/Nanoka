using System.Threading.Tasks;
using Nanoka.Core;

namespace Nanoka
{
    static class Program
    {
        static async Task Main()
        {
            using (var program = new NanokaProgram())
                await program.RunAsync();
        }
    }
}