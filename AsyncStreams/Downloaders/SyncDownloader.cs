using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AsyncStreams
{
    public class SyncDownloader : IDownloader
    {
        public string TempDownloadLocation => @"C:\Users\ahmed\Downloads\temp\";


        public async Task DownloadImages(IEnumerable<string> urls, string location)
        {
            var images = SaveImagesAsFiles(urls);

            await Task.Run(() => MoveDownloadedFiles(images, location));
        }


        private void MoveDownloadedFilesDecomposed(IEnumerable<string> files, string location)
        {
            var enumerator = files.GetEnumerator();

            try
            {
                while (enumerator.MoveNext())
                {
                    var file = enumerator.Current;


                    string fileName = file.Substring(TempDownloadLocation.Length);

                    File.Move(file, location + fileName);
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    enumerator.Dispose(); 
                }
            }
        }


        private void MoveDownloadedFiles(IEnumerable<string> files, string location)
        {
            foreach (var file in files)
            {
                string fileName = file.Substring(TempDownloadLocation.Length);

                File.Move(file, location + fileName);
            }
        }


        private IEnumerable<string> SaveImagesAsFiles(IEnumerable<string> urls)
        {
            using WebClient client = new WebClient();

            foreach (var url in urls)
            {
                string image = url.Split('/').Last();

                client.DownloadFile(new Uri(url), $"{TempDownloadLocation}{image}");

                Console.Write($"\rThe image \"{image}\" has finished downloading");
                Console.WriteLine();

                yield return $"{TempDownloadLocation}{image}";
            }
        }


    }
}
