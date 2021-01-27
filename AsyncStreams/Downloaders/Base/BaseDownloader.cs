using System;
using System.IO;

namespace AsyncStreams
{
    public abstract class BaseDownloader
    {

        #region Properties

        /// <summary>
        /// The location at which images are temporarily stored.
        /// </summary>
        public string TempDownloadLocation { get; }

        /// <summary>
        /// The location at which images are saved after a successful download.
        /// </summary>
        public string DownloadLocation { get; init; }

        #endregion


        #region Constructor

        public BaseDownloader()
        {
            // Here you can change locations if needed.

            TempDownloadLocation = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Temp\\TestImages";
            DownloadLocation = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Downloads\\TestImages";

            if (!Directory.Exists(TempDownloadLocation))
            {
                Directory.CreateDirectory(TempDownloadLocation);
            }
            if (!Directory.Exists(DownloadLocation))
            {
                Directory.CreateDirectory(DownloadLocation);
            }
        }

        #endregion

        #region CleanUp

        private bool cleaned = false;

        public void CleanUpTemp()
        {
            if (cleaned) return;

            if (Directory.Exists(TempDownloadLocation))
            {
                try
                {
                    Directory.Delete(TempDownloadLocation);
                }
                catch (SystemException ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }

            cleaned = true;
        }

        #endregion

    }
}
