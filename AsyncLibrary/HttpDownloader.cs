using System;
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AsyncLibrary
{
    public class HttpDownloader : IDisposable
    {

        #region Private Members

        private HttpClient _client = new();
        private readonly object _logLock = new();

        public string DownloadLocation { get; }

        #endregion

        #region Constructor

        public HttpDownloader(string downloadLocation)
        {
            DownloadLocation = downloadLocation;

            if (!Directory.Exists(DownloadLocation))
            {
                Directory.CreateDirectory(DownloadLocation);
            }
        }

        #endregion

        #region Disposal

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                _client?.Dispose();
                _client = null;
            }

            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion


        #region Sync Image Download Methods

        public string DownloadImage(string imageURL, string imageName)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, imageURL);
                using var response = _client.Send(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                ThreadSafeLog($"\rThe image \"{imageName}\" has started downloading");


                using Stream responseStream = response.Content.ReadAsStream();
                using Stream fileStream = File.Create(PathByName(imageName));

                responseStream.CopyTo(fileStream);


                ThreadSafeLog($"\rThe image \"{imageName}\" has finished downloading");

                return PathByName(imageName);
            }
            catch (HttpRequestException)
            {
                ThreadSafeLog($"\rThe image \"{imageName}\" has failed downloading");
                throw;
            }
        }
        
        public string DownloadImageWithFeedback(string imageURL, string imageName, IProgress<ProgressReport> progress)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, imageURL);
                using var response = _client.Send(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                ThreadSafeLog($"\rThe image \"{imageName}\" has started downloading");


                var contentLength = response.Content.Headers.ContentLength.Value;

                using Stream responseStream = response.Content.ReadAsStream();
                using Stream fileStream = File.Create(PathByName(imageName));


                var downloadProgress = new Progress<long>(downloaded =>
                {
                    float percentage = (float)downloaded / contentLength * 100f;

                    progress.Report(new ProgressReport(percentage, imageName));
                });

                CopyWithProgress(responseStream, fileStream, downloadProgress);

                ThreadSafeLog($"\rThe image \"{imageName}\" has finished downloading");

                return PathByName(imageName);
            }
            catch (HttpRequestException)
            {
                ThreadSafeLog($"\rThe image \"{imageName}\" has failed downloading");
                throw;
            }
        }

        #endregion

        #region Async Image Download Methods

        public async Task<string> DownloadImageAsync(string imageURL, string imageName)
        {
            try
            {
                await using Stream responseStream = await _client.GetStreamAsync(imageURL);
                await using Stream fileStream = File.Create(PathByName(imageName));

                ThreadSafeLog($"\rThe image \"{imageName}\" has started downloading");

                await responseStream.CopyToAsync(fileStream);

                ThreadSafeLog($"\rThe image \"{imageName}\" has finished downloading");

                return PathByName(imageName);
            }
            catch (HttpRequestException)
            {
                ThreadSafeLog($"\rThe image \"{imageName}\" has failed downloading");
                throw;
            }
        }

        public async Task<string> DownloadImageWithFeedbackAsync(string imageURL, string imageName, IProgress<ProgressReport> progress, Action<string> errorCallback = null)
        {
            try
            {
                using var response = await _client.GetAsync(imageURL, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                ThreadSafeLog($"\rThe image \"{imageName}\" has started downloading");


                var contentLength = response.Content.Headers.ContentLength.Value;

                await using Stream responseStream = await response.Content.ReadAsStreamAsync();
                await using Stream fileStream = File.Create(PathByName(imageName));


                var downloadProgress = new Progress<long>(downloaded =>
                {
                    float percentage = (float)downloaded / contentLength * 100f;

                    progress.Report(new ProgressReport(percentage, imageName));
                });

                await CopyWithProgressAsync(responseStream, fileStream, downloadProgress);

                ThreadSafeLog($"\rThe image \"{imageName}\" has finished downloading");

                return PathByName(imageName);
            }
            catch (HttpRequestException)
            {
                ThreadSafeLog($"\rThe image \"{imageName}\" has failed downloading");

                errorCallback?.Invoke(imageName);
                throw;
            }
        }

        #endregion


        #region Helper Methods

        private string PathByName(string imageName) => $"{DownloadLocation}\\{imageName}";


        /// <summary>
        /// Thread-Safe logging to the console.
        /// </summary>
        public void ThreadSafeLog(string log, bool appendLine = true)
        {
            lock (_logLock)
            {
                Console.Write(log);
                if (appendLine)
                {
                    Console.WriteLine();
                }
            }
        }


        /// <summary>
        /// Copy data from source Stream to destination Stream in chunks to report progress back.
        /// </summary>
        /// <param name="source">The source Stream</param>
        /// <param name="destination">The destination Stream</param>
        /// <param name="progress">The progress reporting object</param>
        private static void CopyWithProgress(Stream source, Stream destination, IProgress<long> progress)
        {
            Span<byte> buffer = new byte[81920];

            int bytesRead;
            long totalRead = 0;

            while ((bytesRead = source.Read(buffer)) > 0)
            {
                destination.Write(buffer[..bytesRead]);

                totalRead += bytesRead;
                progress.Report(totalRead);
            }
        }

        /// <summary>
        /// Copy data from source Stream to destination Stream in chunks to report progress back asynchronously.
        /// </summary>
        /// <param name="source">The source Stream</param>
        /// <param name="destination">The destination Stream</param>
        /// <param name="progress">The progress reporting object</param>
        private static async Task CopyWithProgressAsync(Stream source, Stream destination, IProgress<long> progress)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent();

            int bytesRead;
            long totalRead = 0;

            var buffer = memoryOwner.Memory;

            while ((bytesRead = await source.ReadAsync(buffer)) > 0)
            {
                await destination.WriteAsync(buffer[..bytesRead]);

                totalRead += bytesRead;
                progress.Report(totalRead);
            }
        }

        #endregion


    }
}
