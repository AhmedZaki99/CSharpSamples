using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AsyncStreams
{
    public class AsyncDownloader : IDownloader
    {
        public string TempDownloadLocation => @"C:\Users\ahmed\Downloads\temp\";


        public async Task DownloadImages(IEnumerable<string> urls, string location)
        {
            var images = SaveImagesAsFilesAsync(urls);

            await MoveDownloadedFilesAsync(images, location);
        }


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


        private async IAsyncEnumerable<string> SaveImagesAsFilesAsync(IEnumerable<string> urls)
        {
            using WebClient client = new WebClient();

            foreach (var url in urls)
            {
                string image = url.Split('/').Last();

                await client.DownloadFileTaskAsync(new Uri(url), $"{TempDownloadLocation}{image}");

                Console.Write($"\rThe image \"{image}\" has finished downloading");
                Console.WriteLine();

                yield return $"{TempDownloadLocation}{image}";
            }
        }


    }
}
