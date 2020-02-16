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
            Console.WriteLine();


            // The diagnostics data of the process of categorizing files in my "C" drive 
            // after collecting all of them together in an array shows that it took about 
            // an hour (70 minutes) to complete, and consumed about half a Gigabyte
            // of memory (460 MB), which is a disaster!
            ICategorizer arrayCategorizer = new ArrayCategorizer();

            // The diagnostics data of the same process, after adding all of the files to
            // a list instead of collecting them in an array, shows that it took about 
            // 70 seconds to complete, and consumed about 270 MB of memory, which is 
            // relatively fast but still consuming a lot of memory.
            ICategorizer listCategorizer = new ListCategorizer();

            // And when we applied the enumerators logic to collect the data required (file extension)
            // of each file at a time and group them as types and files count, without holding
            // all the data together in memory, the diagnostics data shows that it took only 37
            // seconds to complete, and consumed only 20 MB of memory, which is - in respect to the
            // other processes - very efficient way of doing this process.
            ICategorizer EnumerationCategorizer = new EnumerationCategorizer();

            await Categorize(EnumerationCategorizer);


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
                Console.WriteLine();

                if (!Directory.Exists(path))
                {
                    Console.WriteLine("The directory you entered doesn't exist!");
                    Console.WriteLine();

                    continue;
                }

                using (var cts = new CancellationTokenSource())
                {
                    Task awaiter = VisualizeWaitingAsync(cts.Token);

                    await categorizer.CategorizeFileTypesAsync(path);

                    cts.Cancel();
                }

                Console.WriteLine("\nDone.");
                Console.WriteLine("If you want to start another categorization process press 'Enter'.");

                if (Console.ReadKey().Key != ConsoleKey.Enter)
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
                    Console.Write("\r                      ");
                    Console.Write("\rCategorizing files ");
                }
                else looper++;
            }
        }


    }
}
