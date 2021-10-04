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
            InitLogger();
            string sourcePath = args[0];
            string replicPath = args[1];
            TimeSpan interval = TimeSpan.Parse(args[2]); 
            string logPath = args[3];
            _logger.Info(interval);
            _logger.Info(interval.Seconds);
            _logger.Info("{0}\n{1}\n{2}\n{3}", sourcePath, replicPath, interval, logPath);

            _logger.Info("Hello World!");
            

            // var catalogSource = CatalogReader.WalkDirectoryTree2(new MyDirectory(new System.IO.DirectoryInfo(sourcePath)));
            // GoToFolderCW(catalogSource);

            // var catalogReplic = CatalogReader.WalkDirectoryTree2(new MyDirectory(new System.IO.DirectoryInfo(replicPath)));
            // GoToFolderCW(catalogReplic);

            // Comparator.Compare(catalogSource, ref catalogReplic);

            
            var dicA = CatalogReader.GetFilesDictionary(sourcePath);
            var dicB = CatalogReader.GetFilesDictionary(replicPath);
            var sourceDirs = CatalogReader.GetDirectories(sourcePath);
            var replicDirs = CatalogReader.GetDirectories(replicPath);
            // var dirsReplic = CatalogReader.GetDirectories(replicPath);
            //везде юзать static или экземплярные
            new Comparator().CompareTwoDirectories(sourceDirs, replicDirs);
            new Comparator().CompareTwoFilesDictionaries(dicA, dicB);
            // _logger.Info("--------------SOURCE---------");
            // WriteLineDic(dicA.Select(i => i.Value));
            // _logger.Info("--------------Replic---------");
            // WriteLineDic(dicB.Select(i => i.Value));
            CatalogManager.AddDirectories(sourceDirs, replicPath);
            CatalogManager.MoveFiles(dicB.Select(i => i.Value), replicPath);
            //CatalogManager.ManageDirectories(replicDirs);
            CatalogManager.AddFiles(dicA.Select(i => i.Value), replicPath);
            CatalogManager.DeleteFiles(dicB.Select(i => i.Value));
            CatalogManager.DeleteDirectories(replicDirs);
            Console.ForegroundColor = ConsoleColor.DarkCyan;

            LogManager.Shutdown();
        }

        private static void InitLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "file.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            logconsole.Layout = "${date}: ${message}";
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
        }

        private static void GoToFolderCW(MyDirectory mdir)
        {
            if(mdir.Files != null)
            {
                foreach (var item in mdir.Files)
                {
                    _logger.Info("{0}:{1}",item.RelativePath, item.CatalogItemAction);
                }
            }
            var dirs = mdir.Subdirectories;
            if(dirs != null)
            {
                foreach (var item in dirs)
                {
                    _logger.Info("{0}:{1}", item.RelativePath, item.CatalogItemAction);
                    GoToFolderCW(item);
                }
            }
        }
    
        private static void WriteLineDic(IEnumerable<MyFile> files)
        {
            foreach (var item in files)
            {
                _logger.Info("Uri: {0}\nWasChecked: {1}\nAction: {2}", 
                                item.RelativePath, item.WasChecked, item.CatalogItemAction);
            }
        }
    }
}
