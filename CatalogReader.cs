using System;
using System.Collections.Generic;
using System.IO;
using NLog;

namespace CatalogSyncher
{
    internal class CatalogReader
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public static IDictionary<byte[], MyFile> GetFilesDictionary(string filePath)
        {
            var filesDic = new Dictionary<byte[], MyFile>(new ByteArrayEqualityComparer());
            string[] paths = Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories);
            foreach (var path in paths)
            {
                try
                {
                    var newFile = new MyFile(new FileInfo(path));
                    newFile.CreateRelativePath(filePath, path);
                    filesDic.Add(newFile.GetHashMD5(), newFile);
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                }
            }
            return filesDic;
        }

        public static MyDirectory GetDirectories(string path)
        {
            var root = new MyDirectory(new DirectoryInfo(path));
            WalkDirectory(root, path);
            return root;
        }

        private static void WalkDirectory(MyDirectory directory, string initialPath)
        {
            var subDirs = directory.Info.GetDirectories();
            foreach (var dirInfo in subDirs)
            {
                try
                {
                    var folder = new MyDirectory(dirInfo);
                    folder.CreateRelativePath(initialPath, folder.Info.FullName);
                    directory.CreateDirectory(folder);
                    WalkDirectory(folder, initialPath);
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                }
            }
        }
    }
}