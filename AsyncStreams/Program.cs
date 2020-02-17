using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AsyncStreams
{
    class Program
    {

        private const string WebsiteURL = "https://wallpaperaccess.com/full/";

        private const string SaveLocation = @"E:\Pictures\Downloaded Images\";


        public static string[] ImagesIds => new string[] { "405499", "405461", "405470", "405489", "7281", "405538", "38193", "207509", "39640", "405435" };

        private static string UrlById(string id) => $"{WebsiteURL}{id}.jpg";


        static async Task Main()
        {
            Console.WriteLine("Press any key to start processing ...");
            Console.ReadKey(true);
            Console.WriteLine();



            var imagesUrls = ImagesIds[0..3].Select(i => UrlById(i));


            IDownloader syncDownloader = new SyncDownloader();

            IDownloader asyncDownloader = new AsyncDownloader();


            await WaitForDownloadAsync(asyncDownloader.DownloadImages(imagesUrls, SaveLocation));


            Console.WriteLine("\nAll done!\n");



            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey(true);
        }


        private static async Task WaitForDownloadAsync(params Task[] downloadTasks)
        {
            await Task.Run(async () =>
            {
                int interval = 500;

                while (!downloadTasks.All(task => task.IsCompleted))
                {
                    Console.Write("\rDownloading.");
                    await Task.Delay(interval);
                    Console.Write("\rDownloading..");
                    await Task.Delay(interval);
                    Console.Write("\rDownloading...");
                    await Task.Delay(interval);
                    Console.Write("\rDownloading   ");
                }
                Console.Write("\r              ");
            });
        }

    }
}
