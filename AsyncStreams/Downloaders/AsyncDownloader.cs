using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AsyncStreams
{

    /// <summary>
    /// Although this specific example could be done with many different ways
    /// excluding "Async Streams", it still a good way to show that when we have
    /// a method that produces streams of data, with another methods that depend
    /// on it and are iterating through the data it produces, then it's possible 
    /// to provide asyncronous operations inside the function using "Async Streams"
    /// introduced in C# 8.0
    /// </summary>
    public class AsyncDownloader : IDownloader
    {
        // The location at which the images are temporarily stored.
        public string TempDownloadLocation => @"C:\Users\ahmed\Downloads\temp\";


        public async Task DownloadImages(IEnumerable<string> urls, string location)
        {
            // The "IAsyncEnumerable" function that once called, will initiate
            // the download processes and wait for the first finished download 
            // when "MoveNextAsync()" is called, and so on.
            var images = SaveImagesAsync(urls);

            // Download and move images to the given location asyncronously.
            await MoveDownloadedFilesAsync(images, location);
        }


        /// <summary>
        /// A function that shows the compiler translation of "await foreach" statement 
        /// with async streams, which uses the "IAsyncEnumerator.MoveNextAsync()" function 
        /// to fetch the next result.
        /// </summary>
        private async Task MoveDownloadedFilesDecomposedAsync(IAsyncEnumerable<string> files, string location)
        {
            var asyncEnumerator = files.GetAsyncEnumerator();

            try
            {
                while (await asyncEnumerator.MoveNextAsync())
                {
                    var file = asyncEnumerator.Current;


                    string fileName = file.Substring(TempDownloadLocation.Length);

                    File.Move(file, location + fileName);
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


        private async Task MoveDownloadedFilesAsync(IAsyncEnumerable<string> files, string location)
        {
            await foreach (var file in files)
            {
                string fileName = file.Substring(TempDownloadLocation.Length);

                File.Move(file, location + fileName);
            }
        }


        /// <summary>
        /// Initiates all the download processes asyncronously, then returns the
        /// path of each of the finished downloads once finished.
        /// </summary>
        private async IAsyncEnumerable<string> SaveImagesAsync(IEnumerable<string> urls)
        {
            var downloadTasks = new List<Task<string>>();

            foreach (var url in urls)
            {
                string image = url.Split('/').Last();

                downloadTasks.Add(SaveImageAsync(url, image));
            }

            while (downloadTasks.Count > 0)
            {
                var task = await Task.WhenAny(downloadTasks);

                yield return $"{TempDownloadLocation}{task.Result}";

                downloadTasks.Remove(task);
            }
        }

        private async Task<string> SaveImageAsync(string url, string fileName)
        {
            using WebClient client = new WebClient();

            await client.DownloadFileTaskAsync(new Uri(url), $"{TempDownloadLocation}{fileName}");

            Console.Write($"\rThe image \"{fileName}\" has finished downloading");
            Console.WriteLine();

            return fileName;
        }



    }
}
