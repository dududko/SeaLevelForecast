using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using FloodForecasting.utils;
using GFSConverter;

namespace FloodForecasting.download.gfs
{
	static class GFSDownloader
	{
		private static string URLTemplate =
				"http://nomads.ncep.noaa.gov/cgi-bin/filter_gfs_0p50.pl?file=gfs.t{0}z.pgrb2full.0p50.f{2}&" +
				"lev_10_m_above_ground=on&lev_mean_sea_level=on&var_PRMSL=on&var_UGRD=on&var_VGRD=on&subregion=&leftlon=7&rightlon=32&toplat=68&bottomlat=52&dir=%2Fgfs.{1}{0}";

		public static string DownloadGfsForTime(DateTime date, string folder, int leadTime)
		{
            String filename = "gfs_4_" + date.ToString("yyyyMMdd") + (6 * (date.Hour / 6)).ToString("00") + "00+" + leadTime.ToString("000") + "H00M";

            string gfsUrl = System.String.Format(URLTemplate, (6 * (date.Hour / 6)).ToString("00"), date.ToString("yyyyMMdd"), leadTime.ToString("000"));

			WebClient webClient = new WebClient();                                                          // Creates a webclient
            webClient.DownloadFile(new Uri(gfsUrl), folder + filename);           // Defines the URL and destination directory for the downloaded file
            webClient.Dispose();
		    if (new FileInfo(folder + filename).Length == 0)
		    {
		        throw new Exception("Downloaded file is broken. Please download it again");
		    }
			return filename;
		}

        public static Tuple<List<string>, int> DownloadGfsForDate(DateTime date, string folder, int startTime, int leadTime)
        {
            List<string> files = new List<string>();
            int time = startTime;

            for (int i = startTime; i <= leadTime; i += 3)
            {
                try
                {
                    string s = GFSDownloader.DownloadGfsForTime(date, folder, i);
                    files.Add(s);
                    time = i;
                }
                catch (Exception e)
                {
                    e.ToString();
                    break;
                }
            }
            return new Tuple<List<string>, int>(files, time);
        }
	}
}
