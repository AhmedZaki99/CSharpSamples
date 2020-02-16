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
    public class EnumerationCategorizer : ICategorizer
    {

        public async Task CategorizeFileTypesAsync(string path)
        {
            var info = new DirectoryInfo(path);

            var watch = new Stopwatch();
            watch.Start();


            // Using "Enumerable.GroupBy()" here in this specific case loses the logic we're trying to apply
            // and starts by collecting the collection of "FileInfo" first to group and order them, which
            // is still expensive on memory.
            //--------------------------------------------------------
            //var fileTypes = await Task.Run(() =>
            //{
            //    return GetFileTree(info).GroupBy(f => f.Extension, (type, files) => new { Key = type, Value = files.Count() }).OrderByDescending(i => i.Value);
            //});
            //--------------------------------------------------------


            // So we made a grouping function that only cares about the type of data we're trying to collect (type and count).
            var fileTypes = (await GroupFilesByTypeAsync(info)).OrderByDescending(i => i.Value);


            foreach (var type in fileTypes)
            {
                Console.Write($"\n{type.Value} {(type.Value > 1 ? "files were" : "file was")} found of type {type.Key}.");
            }


            watch.Stop();
            Console.WriteLine($"\n\nTime elapsed : {watch.ElapsedMilliseconds / 1000.0} seconds.");
        }


        #region Enumerations

        /// <summary>
        /// A special method for grouping files by type asyncronously,
        /// since "Enumerable.GroupBy()" function is considered to be expensive process in this specific situation.
        /// </summary>
        private async Task<Dictionary<string, int>> GroupFilesByTypeAsync(DirectoryInfo directory)
        {
            // Saving the grouped data as type and the count of files of this type,
            // instead of returning "IEnumerable<IGrouping<string, FileInfo>>" since having a collection
            // of too many objects of type "FileInfo" is too heavy on memory.
            var dictionary = new Dictionary<string, int>();

            // Runs the files enumerating process on worker thread.
            await Task.Run(() =>
            {
                foreach (var file in GetFileTree(directory))
                {
                    if (dictionary.ContainsKey(file.Extension))
                    {
                        dictionary[file.Extension]++;
                    }
                    else dictionary[file.Extension] = 1;
                }
            });

            return dictionary;
        }

        /// <summary>
        /// Gets all the files and sub-files stored inside a specific directory.
        /// </summary>
        private IEnumerable<FileInfo> GetFileTree(DirectoryInfo directory)
        {
            // Here the compiler calls the "Enumerator.MoveNext()" function to reach the first yield return.
            foreach (var item in GetDirectoryTree(directory))
            {
                IEnumerable<FileInfo> files = null;

                // To avoid file security exceptions.
                try
                {
                    // Enumerate files in the current directory.
                    files = item.EnumerateFiles();
                }
                catch (Exception) { continue; }

                foreach (var file in files)
                {
                    yield return file;
                }
            }
        }

        /// <summary>
        /// Gets all the directories and sub-directories stored inside a specific directory.
        /// </summary>
        private IEnumerable<DirectoryInfo> GetDirectoryTree(DirectoryInfo directory)
        {
            IEnumerable<DirectoryInfo> directories;

            // To avoid directory security exceptions.
            try
            {
                // Returns only the reference for the IEnumerable function,
                // then when iterating through items, returns each item at a time.
                directories = directory.EnumerateDirectories();
            }
            catch (Exception) { yield break; }

            // Iterating through each directory here means to geat each one of them at a time,
            // then release it (the directory info object now is no longer in use) and go to the next one.
            foreach (var item in directories)
            {
                // Return the first encounered item and get out of the function until the next "Enumerator.MoveNext()" call.
                yield return item;

                foreach (var treeItem in GetDirectoryTree(item))
                {
                    yield return treeItem;
                }
            }
        }

        #endregion

    }

}
