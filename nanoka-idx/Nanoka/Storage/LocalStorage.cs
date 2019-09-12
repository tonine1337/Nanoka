using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Nanoka.Storage
{
    /// <summary>
    /// Fully thread-safe asynchronous filesystem-backed storage engine with basic data deduplication.
    /// </summary>
    public class LocalStorage : IStorage
    {
        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        readonly JsonSerializer _serializer;
        readonly ILogger<LocalStorage> _logger;

        readonly DirectoryInfo _contentDir;
        readonly DirectoryInfo _indexDir;

        public LocalStorage(IHostingEnvironment environment, IOptions<LocalStorageOptions> options, JsonSerializer serializer, ILogger<LocalStorage> logger)
        {
            _serializer = serializer;
            _logger     = logger;

            _contentDir = Directory.CreateDirectory(options.Value.ContentPath == null
                                                        ? Path.Combine(environment.ContentRootPath, "data_storage", "content")
                                                        : Path.GetFullPath(options.Value.ContentPath));

            _indexDir = Directory.CreateDirectory(options.Value.IndexPath == null
                                                      ? Path.Combine(environment.ContentRootPath, "data_storage", "index")
                                                      : Path.GetFullPath(options.Value.IndexPath));
        }

        Task IStorage.InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        sealed class Entry
        {
            [JsonProperty("hash")]
            public string Hash { get; set; }

            [JsonProperty("type")]
            public string MediaType { get; set; }
        }

        public async Task<StorageFile> ReadAsync(string name, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                var entryPath = GetEntryPath(name);

                if (!File.Exists(entryPath))
                    return null;

                // read entry
                Entry entry;

                using (var stream = File.OpenRead(entryPath))
                using (var reader = new StreamReader(stream))
                    entry = _serializer.Deserialize<Entry>(reader);

                // read content stream
                return new StorageFile
                {
                    Name      = name,
                    Stream    = File.OpenRead(GetHashPath(entry.Hash)),
                    MediaType = entry.MediaType
                };
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Failed to read file '{name}'.", e);
                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> WriteAsync(StorageFile file, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                var entryPath = GetEntryPath(file.Name);

                if (File.Exists(entryPath))
                    return true;

                byte[] buffer;

                if (file.Stream is MemoryStream memory)
                    buffer = memory.ToArray();
                else
                    using (var memory2 = new MemoryStream())
                    {
                        await file.Stream.CopyToAsync(memory2, cancellationToken);
                        buffer = memory2.ToArray();
                    }

                var entry = new Entry
                {
                    MediaType = file.MediaType
                };

                // compute content hash
                using (var sha = SHA256.Create())
                    entry.Hash = BitConverter.ToString(sha.ComputeHash(buffer)).Replace("-", "");

                var contentPath = GetHashPath(entry.Hash);

                try
                {
                    // write content
                    if (File.Exists(contentPath))
                    {
                        contentPath = null;
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(contentPath));

                        using (var stream = File.Create(contentPath))
                            await stream.WriteAsync(buffer, cancellationToken);
                    }

                    try
                    {
                        // write entry
                        Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                        using (var stream = File.Create(entryPath))
                        using (var writer = new StreamWriter(stream))
                            _serializer.Serialize(writer, entry);
                    }
                    catch
                    {
                        File.Delete(entryPath);

                        throw;
                    }

                    return true;
                }
                catch
                {
                    if (contentPath != null)
                        File.Delete(contentPath);

                    throw;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Failed to write file '{file.Name}'.", e);
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> DeleteAsync(string name, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                var entryPath = GetEntryPath(name);

                if (!File.Exists(entryPath))
                    return false;

                File.Delete(entryPath);

                // content file is not deleted because it may be referenced by other entries

                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Failed to delete file '{name}'.", e);
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        string GetEntryPath(string name) => Path.Combine(_indexDir.FullName, name.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
        string GetHashPath(string hash) => Path.Combine(_contentDir.FullName, hash.Substring(0, 2), hash.Substring(2, 2), hash.Substring(4));

        public void Dispose() => _semaphore.Dispose();
    }
}