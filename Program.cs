using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace CatalogSyncher
{
    class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        static int Main(string[] args)
        {
            if(args.Length != 4)
            {
                return 1;
            }
            string sourcePath = args[0]; //todo named params
            string replicPath = args[1];
            TimeSpan.TryParse(args[2], out var interval); 
            string logPath = args[3];
            if(logPath == null)
            {
                logPath = "Log.log";
            }
            InitLogger(logPath);
            try
            {
                LogArguments(sourcePath, replicPath, interval, logPath);
                ValidateParams(sourcePath, replicPath, interval);
                using (var syncher = new PeriodicSyncher(interval, new SyncManager(sourcePath, replicPath)))
                {
                    System.Console.WriteLine("Press Escape for exit...");
                    WaitForEscape();
                    
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            finally
            {
                LogManager.Shutdown();
            }
            return 0;
        }

        private static void WaitForEscape()
        {
            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                System.Threading.Thread.Sleep(250);
            }
        }

        private static void LogArguments(string sourcePath, string replicPath, TimeSpan interval, string logPath)
        {
            _logger.Trace("Interval: {interval}", interval);
            _logger.Trace("Source path: {source}", sourcePath);
            _logger.Trace("Replic path: {replic}", replicPath);
            _logger.Trace("Log path: {log}", logPath);
        }

        private static void ValidateParams(string sourcePath, string replicPath, TimeSpan interval)
        {
            if (sourcePath == null || !Directory.Exists(sourcePath))
            {
                throw new ArgumentException($"The path \"{sourcePath}\" doesn't exist");
            }
            if (replicPath == null)
            {
                throw new ArgumentException("Replic path wasn't specifyed");
            }
            if (!Directory.Exists(replicPath))
            {
                Directory.CreateDirectory(replicPath);
            }
            if (interval <= TimeSpan.Zero)
            {
                throw new ArgumentException("Invalid interval");
            }
        }

        private static void InitLogger(string logPath)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var traceLogfile = new NLog.Targets.FileTarget("traceLogfile") 
            {
                FileName = "TraceLog.log",
            };
            var logfile = new NLog.Targets.FileTarget("logfile") 
            { 
                FileName = GetLogPath(logPath),
                Layout = "${date}: ${message}", 
            };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole")
            {
                Layout = "${date}: ${message}"
            };
            config.AddRule(LogLevel.Info, LogLevel.Error, logconsole);
            config.AddRule(LogLevel.Info, LogLevel.Error, logfile);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, traceLogfile);
            LogManager.Configuration = config;
        }

        private static string GetLogPath(string logPath)
        {
            const string defaultFileName = "Log.log";
            string resultLogPath;
            if(File.Exists(logPath))
            {
               resultLogPath = logPath;
            }
            else if(Directory.Exists(logPath))
            {
                resultLogPath = Path.Combine(logPath, defaultFileName);
            }
            else if(logPath.EndsWith('/') || string.IsNullOrEmpty(Path.GetExtension(logPath)))
            {
                Directory.CreateDirectory(logPath);
                resultLogPath = Path.Combine(logPath, defaultFileName);
            }
            else
            {
                resultLogPath = logPath;
            }
            return resultLogPath;
        }

        // private static void GoToFolderCW(MyDirectory mdir)
        // {
        //     if(mdir.Files != null)
        //     {
        //         foreach (var item in mdir.Files)
        //         {
        //             _logger.Info("{0}:{1}",item.RelativePath, item.CatalogItemAction);
        //         }
        //     }
        //     var dirs = mdir.Subdirectories;
        //     if(dirs != null)
        //     {
        //         foreach (var item in dirs)
        //         {
        //             _logger.Info("{0}:{1}", item.RelativePath, item.CatalogItemAction);
        //             GoToFolderCW(item);
        //         }
        //     }
        // }
    
        // private static void WriteLineDic(IEnumerable<MyFile> files)
        // {
        //     foreach (var item in files)
        //     {
        //         _logger.Info("Uri: {0}\nWasChecked: {1}\nAction: {2}", 
        //                         item.RelativePath, item.WasChecked, item.CatalogItemAction);
        //     }
        // }


    }
}
