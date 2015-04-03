using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FloodForecasting.download.gfs;
using FloodForecasting.utils;
using log4net;

namespace FloodForecasting
{
	class Program
	{
        private DateTime StartDate;
	    private readonly string downloadDir;
        private readonly string outputDir;
        private double sleepDuration;
        private readonly int ensurance;
	    private IniConfig config;

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


	    public Program()
	    {
            config = new IniConfig("GFSConverter.ini");
            downloadDir = Utils.GetValueAsPath(config, "Storage", "DownloadDir", true, @"download\gfs\");
            outputDir = Utils.GetValueAsPath(config, "Storage", "OutputDir", true, @"output\grib1\");
            sleepDuration = Utils.GetValueAsInt(config, "Main", "SleepDuration(mins)", true, 5);
            ensurance = Utils.GetValueAsInt(config, "Main", "Ensurance", true, 5);
            StartDate= Utils.GetValueAsDateTime(config, "Main", "StartTime", true, DateTime.MinValue);
	    }

	    public void StartDownloadProcess()
	    {
            log.Info("Process is started");

            Directory.CreateDirectory(downloadDir);
            Directory.CreateDirectory(outputDir);

            DateTime endDate = DateTime.UtcNow;
            endDate = endDate.AddHours(-endDate.Hour % 6);

            for (DateTime date = StartDate; date <= endDate; date = date.AddHours(6))
            {
                int downloaded = 0;
                int total = 61;

                string downloadSubpath = downloadDir + string.Format("{0}\\{1}\\", date.Year, date.Month.ToString("00"));
                string outputSubpath = outputDir + string.Format("{0}\\{1}\\", date.Year, date.Month.ToString("00"));
                Directory.CreateDirectory(downloadSubpath);
                Directory.CreateDirectory(outputSubpath);

                log.Info("Start download for date " + date.ToString("yyyyMMddHH"));
                Tuple<List<string>, int> tuple = GFSDownloader.DownloadGfsForDate(date, downloadSubpath, 0, 180);

                downloaded += tuple.Item1.Count;
                log.Info(string.Format("Total downloaded files: {0} of {1}", downloaded, total));
                log.Info(string.Format("Total downloaded files: {0}", tuple.Item1.ToString()));
                GribConverter.ConvertToGrib1(downloadSubpath, outputSubpath, tuple.Item1.ToArray());

                while (tuple.Item2 != ensurance)
                {
                    sleepDuration = (downloaded > 0) ? 0.5 : sleepDuration;
                    log.Info("Start sleeping for 5 minutes");
                    Thread.Sleep(TimeSpan.FromMinutes(sleepDuration));
                    log.Info("Thread is woke up");
                    log.Info(downloaded * 3);

                    tuple = GFSDownloader.DownloadGfsForDate(date, downloadSubpath, downloaded * 3, 180);
                    downloaded += tuple.Item1.Count;
                    GribConverter.ConvertToGrib1(downloadSubpath, outputSubpath, tuple.Item1.ToArray());
                    log.Info(string.Format("Total downloaded files: {0} of {1}", downloaded, total));
                    log.Info(string.Format("Total downloaded files: {0}", tuple.Item1.ToString()));
                }
                log.Info(string.Format("Download for date {0} is complete \n", date));
            }

	        config["Main", "StartTime"] = endDate.AddHours(6).ToString("yyyyMMddHH");
            config.SaveTo(File.OpenWrite("GFSConverter.ini"));

            log.Info("Clean download directory");
            Utils.Empty(downloadDir);

            log.Info("Process has finished all jobs");
            log.Info("Process is closed \n\n");
	    }


	    private static void Notify()
	    {
            var request = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:8888");
            var response = (HttpWebResponse)request.GetResponse();
	    }

	    static void Main(string[] args)
		{
            log4net.Config.XmlConfigurator.Configure();

            var handle = GetConsoleWindow();
            // Hide
            ShowWindow(handle, SW_HIDE);

            Program p = new Program();
	        p.StartDownloadProcess();
            Notify();
		}

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
	}
}
