using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Nanoka.Core.Installer
{
    public static class NanokaCrt
    {
        const string _name = "Nanoka.crt";

        public static X509Certificate Load()
        {
            // prefer loading from current directory
            if (File.Exists(_name))
                return new X509Certificate(_name);

            var assembly = typeof(NanokaCrt).Assembly;

            using (var stream = assembly.GetManifestResourceStream($"{typeof(NanokaCrt).Namespace}.{_name}"))
            {
                if (stream == null)
                    throw new FileNotFoundException("Self-signed certificate could not be loaded.");

                using (var memory = new MemoryStream((int) stream.Length))
                {
                    stream.CopyTo(memory);

                    return new X509Certificate(memory.ToArray());
                }
            }
        }
    }
}
