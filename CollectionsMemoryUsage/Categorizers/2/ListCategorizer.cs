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
    public class ListCategorizer : ICategorizer
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
        private List<FileInfo> GetFileTree(DirectoryInfo directory)
        {
            var output = new List<FileInfo>();

            foreach (var item in GetDirectoryTree(directory))
            {
                FileInfo[] files = null;

                // To avoid file security exceptions.
                try
                {
                    // Stores all the existing files in an array.
                    files = item.GetFiles();
                }
                catch (Exception) { continue; }

                foreach (var file in files)
                {
                    output.Add(file);
                }
            }

            return output;
        }

        /// <summary>
        /// Gets all the directories and sub-directories stored inside a specific directory.
        /// </summary>
        private List<DirectoryInfo> GetDirectoryTree(DirectoryInfo directory)
        {
            var output = new List<DirectoryInfo>();

            DirectoryInfo[] directories;

            // To avoid directory security exceptions.
            try
            {
                // Stores all the existing directories in an array.
                directories = directory.GetDirectories();
            }
            catch (Exception) { return output; }


            foreach (var item in directories)
            {
                output.Add(item);

                var subDirectories = GetDirectoryTree(item);

                foreach (var subItem in subDirectories)
                {
                    output.Add(subItem);
                }
            }

            return output;
        }

        #endregion


    }
}
