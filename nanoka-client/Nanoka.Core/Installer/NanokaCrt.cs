using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Nanoka.Core.Installer
{
    public static class NanokaCrt
    {
        public static X509Certificate2 Load(string name = "Nanoka.pfx")
        {
            // prefer loading from current directory
            if (File.Exists(name))
                return new X509Certificate2(name);

            var assembly = typeof(NanokaCrt).Assembly;

            using (var stream = assembly.GetManifestResourceStream($"{typeof(NanokaCrt).Namespace}.{name}"))
            {
                if (stream == null)
                    throw new FileNotFoundException("Self-signed certificate could not be loaded.");

                using (var memory = new MemoryStream((int) stream.Length))
                {
                    stream.CopyTo(memory);

                    // our certificate password is "nanoka"
                    return new X509Certificate2(memory.ToArray(), "nanoka");
                }
            }
        }
    }
}
