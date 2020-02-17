using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AsyncStreams
{
    public interface IDownloader
    {
        string TempDownloadLocation { get; }

        Task DownloadImages(IEnumerable<string> urls, string location);
    }
}
