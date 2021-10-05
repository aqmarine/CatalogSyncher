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

        static void Main(string[] args)
        {
            string sourcePath = args[0];
            string replicPath = args[1];
            TimeSpan interval = TimeSpan.Parse(args[2]); 
            string logPath = args[3];
            InitLogger(logPath);
            _logger.Trace("Interval: {interval}",interval);
            _logger.Trace("Source path: {source}",sourcePath);
            _logger.Trace("Replic path: {replic}",replicPath);
            _logger.Trace("Log path: {log}",logPath);
            //todo
            if(!Directory.Exists(sourcePath) || !Directory.Exists(replicPath))
            {
                _logger.Error("The path is not correct");
            }
            using (var syncher = new PeriodicSyncher(interval, new SyncManager(sourcePath, replicPath)))
            {
                Console.ReadLine();
            }
            LogManager.Shutdown();
        }

        private static void InitLogger(string logPath)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var debugLogfile = new NLog.Targets.FileTarget("debugLogfile") 
            {
                FileName = "file.txt",
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
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, debugLogfile);
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
