using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CollectionsMemoryUsage
{
    public class ArrayCategorizer : ICategorizer
    {

        public async Task CategorizeFileTypesAsync(string path)
        {
            var info = new DirectoryInfo(path);

            var watch = new Stopwatch();
            watch.Start();


            // Grouping and ordering the files (Categorizing).
            var fileTypes = await Task.Run(() =>
            {
                return GetFileTree(info).GroupBy(f => f.Extension, (type, files) => new { Key = type, Value = files.Count() }).OrderByDescending(i => i.Value);
            });



            foreach (var type in fileTypes)
            {
                Console.Write($"\n{type.Value} {(type.Value > 1 ? "files were" : "file was")} found of type {type.Key}.");
            }


            watch.Stop();
            Console.WriteLine($"\n\nTime elapsed : {watch.ElapsedMilliseconds / 1000.0} seconds.");
        }


        #region Enumerations

        /// <summary>
        /// Gets all the files and sub-files stored inside a specific directory.
        /// </summary>
        private FileInfo[] GetFileTree(DirectoryInfo directory)
        {
            FileInfo[] output = new FileInfo[0];

            DirectoryInfo[] directories = GetDirectoryTree(directory);

            for (int i = 0; i < directories.Length; i++)
            {
                // To avoid file security exceptions.
                try
                {
                    // Stores all the existing files in an array.
                    output = output.Concat(directories[i].GetFiles()).ToArray();
                }
                catch (Exception) { continue; }
            }

            return output;
        }

        /// <summary>
        /// Gets all the directories and sub-directories stored inside a specific directory.
        /// </summary>
        private DirectoryInfo[] GetDirectoryTree(DirectoryInfo directory)
        {
            DirectoryInfo[] output = new DirectoryInfo[0];

            DirectoryInfo[] directories;

            // To avoid directory security exceptions.
            try
            {
                // Stores all the existing directories in an array.
                directories = directory.GetDirectories();
            }
            catch (Exception) { return output; }


            for (int i = 0; i < directories.Length; i++)
            {
                output = output.Append(directories[i]).ToArray();

                output = output.Concat(GetDirectoryTree(directories[i])).ToArray();
            }

            return output;
        }

        #endregion

    }
}
