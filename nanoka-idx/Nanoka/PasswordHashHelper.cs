using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nanoka
{
    public class PasswordHashHelper
    {
        readonly NanokaOptions _options;
        readonly ILogger<PasswordHashHelper> _logger;

        public PasswordHashHelper(IOptions<NanokaOptions> options, ILogger<PasswordHashHelper> logger)
        {
            _options = options.Value;
            _logger  = logger;

            switch (_options.PasswordHashVersion)
            {
                // 0 simply implements Microsoft-recommend hashing method
                // https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing?view=aspnetcore-2.2
                case 0: break;

                default: throw new NotSupportedException($"{_options.PasswordHashVersion} is not a invalid hash version.");
            }
        }

        public string Hash(string password)
        {
            password = password ?? "";

            try
            {
                using (var memory = new MemoryStream())
                using (var writer = new BinaryWriter(memory))
                {
                    writer.Write(_options.PasswordHashVersion);

                    switch (_options.PasswordHashVersion)
                    {
                        case 0:
                            Hash0(writer, password);
                            break;
                    }

                    writer.Flush();

                    return Convert.ToBase64String(memory.ToArray());
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Exception while hashing password.");

                throw;
            }
        }

        public bool Test(string password, string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return false;

            password = password ?? "";

            try
            {
                using (var memory = new MemoryStream(Convert.FromBase64String(hash)))
                using (var reader = new BinaryReader(memory))
                {
                    var version = reader.ReadInt32();

                    switch (version)
                    {
                        case 0: return Test0(reader, password);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Exception while testing password.");
            }

            return false;
        }

#region v0

        static void Hash0(BinaryWriter writer, string password)
        {
            // salt
            var salt = new byte[128 / 8];

            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            writer.Write(salt.Length);
            writer.Write(salt);

            // hash
            var hash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA1, iterationCount: 10000, 256 / 8);

            writer.Write(hash.Length);
            writer.Write(hash);
        }

        static bool Test0(BinaryReader reader, string password)
        {
            // salt
            var len  = reader.ReadInt32();
            var salt = reader.ReadBytes(len);

            // hash
            len = reader.ReadInt32();

            var hash0 = reader.ReadBytes(len);
            var hash1 = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA1, iterationCount: 10000, 256 / 8);

            // test
            return ByteArrayCompare(hash0, hash1);
        }

#endregion

        // https://stackoverflow.com/a/48599119
        static bool ByteArrayCompare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y) => x.SequenceEqual(y);
    }
}