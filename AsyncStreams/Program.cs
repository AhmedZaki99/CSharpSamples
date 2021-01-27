using System;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncStreams
{
    class Program
    {

        /// <summary>
        /// Change to control download type.
        /// </summary>
        private static readonly bool _isAsync = true;


        private static string WebsiteURL { get; } = "https://wallpaperaccess.com/full";

        /// <summary>
        /// Ids starting with the letter 'f' are intended to fail downloading, remove the letter 'f' to download.
        /// </summary>
        private static string[] ImagesIds { get; } = { "405499", "f405461", "405470", "f405489", "7281", "f405538", "38193", "207509", "39640", "405435" };


        static async Task Main()
        {
            Console.WriteLine("Press any key to start processing ...");
            Console.ReadKey(true);
            Console.WriteLine();


            if (_isAsync)
            {
                var downloader = new AsyncDownloader();
                await downloader.DownloadImagesAsync(ImagesIds.Select(id => UrlById(id)));

                downloader.CleanUpTemp();
            }
            else
            {
                var downloader = new SyncDownloader();
                downloader.DownloadImages(ImagesIds.Select(id => UrlById(id)));

                downloader.CleanUpTemp();
            }


            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey(true);
        }


        private static string UrlById(string id) => $"{WebsiteURL}/{id}.jpg";

    }
}
