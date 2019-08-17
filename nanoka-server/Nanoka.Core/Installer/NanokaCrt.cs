using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Nanoka.Core.Installer
{
    public static class NanokaCrt
    {
        static bool IsInstalled(ref X509Certificate2 certificate)
        {
            using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);

                var certificates = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName,
                                                           certificate.Subject,
                                                           false);

                if (certificates.Count == 0)
                    return false;

                certificate?.Dispose();
                certificate = certificates[0];
                return true;
            }
        }

        static void Install(X509Certificate2 certificate)
        {
            using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadWrite);
                store.Add(certificate);
            }
        }

        static X509Certificate2 Load(string name)
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

        public static X509Certificate2 GetCertificate(string name = "Nanoka.pfx")
        {
            // load certificate
            var certificate = Load(name);

            // install certificate if not installed already
            if (!IsInstalled(ref certificate))
                Install(certificate);

            return certificate;
        }
    }
}
