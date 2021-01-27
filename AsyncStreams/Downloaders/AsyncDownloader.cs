using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using AsyncLibrary;

namespace AsyncStreams
{

    /// <summary>
    /// Although tasks in this specific example can be accomplished in many different ways
    /// excluding "Async Streams", it's still a good way to make a representation on how
    /// "Async Streams" allowed us to build pipelines that operate asynchronously,
    /// passing processed data to the next node once done, which opens the possibility
    /// for concurrent processing to take place.
    /// </summary>
    public class AsyncDownloader : BaseDownloader
    {


        #region Public Methods

        public async Task DownloadImagesAsync(IEnumerable<string> urls)
        {
            // The "AsyncEnumerable" function that once called, will initiate
            // the download processes and wait for the first finished download 
            // when "MoveNextAsync()" is called, and so on.
            var downloadedImages = InitializeDownloadAsync(urls);

            // Download and move images to the given location asynchronously.
            await MoveDownloadedImagesAsync(downloadedImages);

            Console.WriteLine();
            Console.WriteLine("\nAll done!\n");
        }

        #endregion


        #region Download Pipelines

        /// <summary>
        /// A function that shows the compiler translation of "await foreach" statement 
        /// with async streams, which uses the "IAsyncEnumerator.MoveNextAsync()" function 
        /// to fetch the next result.
        /// </summary>
        private async Task MoveDownloadedImagesDecomposedAsync(IAsyncEnumerable<string> imagesPaths)
        {
            var asyncEnumerator = imagesPaths.GetAsyncEnumerator();

            try
            {
                while (await asyncEnumerator.MoveNextAsync())
                {
                    var imagePath = asyncEnumerator.Current;


                    string imageName = imagePath[TempDownloadLocation.Length..];

                    File.Move(imagePath, DownloadLocation + imageName);
                }
            }
            finally
            {
                if (asyncEnumerator != null)
                {
                    await asyncEnumerator.DisposeAsync();
                }
            }
        }

        /// <summary>
        /// Move images to the download location once they've finished downloading.
        /// </summary>
        private async Task MoveDownloadedImagesAsync(IAsyncEnumerable<string> imagesPaths)
        {
            await foreach (var imagePath in imagesPaths)
            {
                string imageName = imagePath[TempDownloadLocation.Length..];

                File.Move(imagePath, DownloadLocation + imageName);
            }
        }


        /// <summary>
        /// Initiates all the download processes asynchronously, then returns the
        /// path of each of the finished downloads once finished.
        /// </summary>
        private async IAsyncEnumerable<string> InitializeDownloadAsync(IEnumerable<string> urls)
        {
            using var downloader = new HttpDownloader(TempDownloadLocation);

            // The Key-Value Pair Collection that holds progress for each image.
            var downloadProgress = new Dictionary<string, float>();
            // The list of running download tasks.
            var downloadTasks = new List<Task<string>>();

            foreach (var url in urls)
            {
                // Get image name from url.
                string imageName = url.Split('/').Last();

                // Initialize Dictionary.
                downloadProgress[imageName] = 0;

                // Inistantiate a Progress object to notify download progress.
                var progress = new Progress<ProgressReport>(report =>
                {
                    // Update Progress.
                    downloadProgress[report.FileName] = report.Percentage;
                    // Print Average.
                    downloader.ThreadSafeLog(string.Format("\rDownloading {0:N2}%", downloadProgress.Values.Average()), false);
                });

                // Start & Add download tasks to the list.
                downloadTasks.Add(downloader.DownloadImageWithFeedbackAsync(url, imageName, progress, name => downloadProgress.Remove(name)));
            }

            // Loop through remaining tasks.
            while (downloadTasks.Count > 0)
            {
                var task =  await Task.WhenAny(downloadTasks);

                downloadTasks.Remove(task);

                string result = null;
                try
                {
                    result = await task;
                }
                catch (HttpRequestException)
                {
                    continue;
                }
                yield return result;
            }
        }

        #endregion

    }
}
