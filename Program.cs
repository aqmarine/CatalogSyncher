using System;
using System.IO;
using NLog;

namespace CatalogSyncher
{
    class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        static int Main(string[] args)
        {
            if (args.Length == 1 && IsHelpRequired(args[0]))
            {
                DisplayHelp();
                return 0;
            }
            if (args.Length != 4)
            {
                Console.WriteLine("Invalid arguments.");
                DisplayHelp();
                return 1;
            }
            string sourcePath = args[0];
            string replicaPath = args[1];
            TimeSpan.TryParse(args[2], out var interval); 
            string logPath = args[3];
            if(string.IsNullOrEmpty(logPath))
            {
                logPath = "Log.log";
            }
            InitLogger(logPath);
            try
            {
                LogArguments(sourcePath, replicaPath, interval, logPath);
                ValidateParams(sourcePath, replicaPath, interval);
                var syncher = new PeriodicSyncher(interval, new SyncManager(sourcePath, replicaPath));
                using (syncher)
                {
                    Console.WriteLine("Press Escape for exit...");
                    WaitForEscape();
                }
                if (syncher.SynchronizationRunning)
                {
                    FinishProgram(syncher);
                }
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.Message);
                return 1;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return 2;
            }
            finally
            {
                _logger.Trace("Shutdown logger");
                LogManager.Shutdown();
            }
            return 0;
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("usage: CatalogSyncher [sourcePath] [replicaPath] [interval] [logPath]\n");
            Console.WriteLine("sourcePath  \tSource directory path in format \"DiskN:/MyCatalogs/Source\"");
            Console.WriteLine("replicaPath \tReplica directory path in format \"DiskN:/MyCatalogs/Replica\"");
            Console.WriteLine("interval    \tThe period of updating Replica directory to the state of directory Source in Timespan format, e.g. 00:00:10 (hh:mm:ss)");
            Console.WriteLine("logPath     \tPath for the log file");
        }

        private static bool IsHelpRequired(string param)
        {
            return param == "-h" || param == "--help" || param == "/?";
        }

        private static void FinishProgram(PeriodicSyncher syncher)
        {
            Console.WriteLine("Warning: Now synchronization operation is running.");
            Console.WriteLine("If you want to interrupt synchonization process, press [Y]." +
            "\nIf you want to wait a completion of the operation, press [N]. ");
            ConsoleKeyInfo cki;
            do 
            {
                while (Console.KeyAvailable == false)
                {
                    System.Threading.Thread.Sleep(250);
                }
                cki = Console.ReadKey(true);
            } while (cki.Key != ConsoleKey.Y && cki.Key != ConsoleKey.N);

            Console.WriteLine(cki.Key);
            if (cki.Key == ConsoleKey.N)
            {
                Console.WriteLine("Awaiting running process to finish");
                while (syncher.SynchronizationRunning)
                {
                    System.Threading.Thread.Sleep(100);
                }
                Console.WriteLine("Synchronization was completed. The program are closing...");
            }
        }

        private static void WaitForEscape()
        {
            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                System.Threading.Thread.Sleep(250);
            }
        }

        private static void LogArguments(string sourcePath, string replicaPath, TimeSpan interval, string logPath)
        {
            _logger.Trace("Interval: {interval}", interval);
            _logger.Trace("Source path: {source}", sourcePath);
            _logger.Trace("Replica path: {replica}", replicaPath);
            _logger.Trace("Log path: {log}", logPath);
        }

        private static void ValidateParams(string sourcePath, string replicaPath, TimeSpan interval)
        {
            if (string.IsNullOrEmpty(sourcePath) || !Directory.Exists(sourcePath))
            {
                throw new ArgumentException($"The path \"{sourcePath}\" doesn't exist");
            }
            if (string.IsNullOrEmpty(replicaPath))
            {
                throw new ArgumentException("Replica path wasn't specifyed");
            }
            if (!Directory.Exists(replicaPath))
            {
                Directory.CreateDirectory(replicaPath);
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

    }
}
