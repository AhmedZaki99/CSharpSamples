using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CollectionsMemoryUsage
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Press any key to start processing ..");
            Console.ReadKey(true);
            Console.WriteLine();


            await Categorize(null);


            Console.WriteLine();
            Console.WriteLine("Press Any key to continue ...");
            Console.ReadKey(true);
        }

        private static async Task Categorize(ICategorizer categorizer)
        {
            while (true)
            {
                Console.Write("Type in the directory you want to categorize its files : ");
                string path = Console.ReadLine();

                if (!Directory.Exists(path))
                {
                    Console.WriteLine("The directory you entered doesn't exist!");
                    Console.WriteLine();

                    continue;
                }

                using (var cts = new CancellationTokenSource())
                {
                    Task awaiter = VisualizeWaitingAsync(cts.Token);

                    await categorizer.CategorizeFileTypeAsync();

                    cts.Cancel();
                }

                Console.WriteLine("\nDone.");
                Console.WriteLine("If you want to start another categorization process press 'Enter'.");

                if (Console.ReadKey().Key == ConsoleKey.Enter)
                {
                    break;
                }
            }
        }

        private static async Task VisualizeWaitingAsync(CancellationToken token)
        {
            int looper = 0;
            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (looper > 0)
                {
                    Console.Write($"\rCategorizing files {(looper == 3 ? "..." : looper == 2 ? ".." : ".")}");
                }

                await Task.Delay(500);

                if (looper == 3)
                {
                    looper = 0;
                    Console.Write("\r                   ");
                    Console.Write("\rAnalyzing files ");
                }
                else looper++;
            }
        }


    }
}
