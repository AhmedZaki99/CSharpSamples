using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using AsyncLibrary;

namespace AsyncStreams
{
    public class SyncDownloader : BaseDownloader
    {


        #region Public Methods

        public void DownloadImages(IEnumerable<string> urls)
        {
            Console.WriteLine("\nDownloading...\n");

            // The "Enumerable" function that will download images each at a time
            // when "MoveNext()" is called.
            var downloadedImages = InitializeDownload(urls);

            // Move downloaded images to the given location.
            // But first, download them by calling "foreach" on the "Enumerable".
            MoveDownloadedImages(downloadedImages);

            Console.WriteLine();
            Console.WriteLine("\nAll done!\n");
        }

        #endregion


        #region Download Pipelines

        /// <summary>
        /// A function that shows the compiler translation of "foreach" statement,
        /// which uses the "IEnumerator.MoveNext()" function to fetch the next result.
        /// </summary>
        private void MoveDownloadedImagesDecomposed(IEnumerable<string> imagesPaths)
        {
            var enumerator = imagesPaths.GetEnumerator();

            try
            {
                while (enumerator.MoveNext())
                {
                    var imagePath = enumerator.Current;


                    string imageName = imagePath[TempDownloadLocation.Length..];

                    File.Move(imagePath, DownloadLocation + imageName);
                }
            }
            finally
            {
                enumerator?.Dispose();
            }
        }

        /// <summary>
        /// Move images to the download location once they've finished downloading.
        /// </summary>
        private void MoveDownloadedImages(IEnumerable<string> imagesPaths)
        {
            foreach (var imagePath in imagesPaths)
            {
                string imageName = imagePath[TempDownloadLocation.Length..];

                File.Move(imagePath, DownloadLocation + imageName);
            }
        }


        /// <summary>
        /// Starting all the download operations synchronously one after the other.
        /// </summary>
        private IEnumerable<string> InitializeDownload(IEnumerable<string> urls)
        {
            using var downloader = new HttpDownloader(TempDownloadLocation);

            // The Key-Value Pair Collection that holds progress for each image.
            var downloadProgress = new Dictionary<string, float>(urls.Select(url =>
            {
                // Get image name from url.
                string imageName = url.Split('/').Last();

                // Initialize Dictionary.
                return new KeyValuePair<string, float>(imageName, 0);
            }));

            foreach (var url in urls)
            {
                // Get image name from url.
                string imageName = url.Split('/').Last();

                // Inistantiate a Progress object to notify download progress.
                var progress = new Progress<ProgressReport>(report =>
                {
                    // Update Progress.
                    downloadProgress[report.FileName] = report.Percentage;
                    // Print Average.
                    downloader.ThreadSafeLog(string.Format("\rDownloading {0:N2}%", downloadProgress.Values.Average()), false);
                });

                string downloadedImage = null;
                try
                {
                    // Start downloading the image.
                    downloadedImage = downloader.DownloadImageWithFeedback(url, imageName, progress);
                }
                catch (HttpRequestException)
                {
                    downloadProgress.Remove(imageName);
                    continue;
                }
                yield return downloadedImage;
            }
        }

        #endregion

    }
}
