using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace kpeg.ProcessContainers
{
    public class VideoNameDownloader
    {
        private static VideoNameDownloader VideoNameDownloaderInstance;
        private VideoNameDownloader()
        {

        }
        public static VideoNameDownloader GetInstance()
        {
            if(VideoNameDownloaderInstance == null)
                VideoNameDownloaderInstance = new VideoNameDownloader();
            return VideoNameDownloaderInstance;
        }

    }
}
