using System;
using System.Collections.Generic;
using System.IO;
using NLog;

namespace CatalogSyncher
{
    public class CatalogReader
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public static MyDirectory WalkDirectoryTree(System.IO.DirectoryInfo root)
        {
            System.IO.FileInfo[] files;
            System.IO.DirectoryInfo[] subDirs;
            //_logger.Info(root.FullName);
            var catalog = new MyDirectory(root);
            files = root.GetFiles("*.*");
            //var tempCatalog = catalog;
            if (files != null)
            {
                foreach (var fi in files)
                {
                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    //_logger.Info(fi.FullName);
                    catalog.AddFile(new MyFile(fi));
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();
               

                foreach (var dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    catalog.AddDirectory(new MyDirectory(dirInfo));
                    WalkDirectoryTree(dirInfo);
                }
            }
            return catalog;
        }

        public static MyDirectory WalkDirectoryTree2(MyDirectory root)
        {
            System.IO.FileInfo[] files;
            System.IO.DirectoryInfo[] subDirs;
            //_logger.Info(root.FullName);
            var catalog = root;
            files = catalog.Info.GetFiles("*.*");
            //var tempCatalog = catalog;
            if (files != null)
            {
                foreach (var fi in files)
                {
                    catalog.AddFile(new MyFile(fi));
                }

                // Now find all the subdirectories under this directory.
                subDirs = catalog.Info.GetDirectories();
               

                foreach (var dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    var folder = new MyDirectory(dirInfo);
                    catalog.AddDirectory(folder);
                    WalkDirectoryTree2(folder);
                }
            }
            return catalog;
        }
    
        public static IDictionary<byte[], MyFile> GetFilesDictionary(string filePath)
        {
            var filesDic = new Dictionary<byte[], MyFile>(new ByteArrayEqualityComparer());
            string[] paths = Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories);
            foreach (var path in paths)
            {
                var newFile = new MyFile(new FileInfo(path));
                newFile.CreateRelativePath(filePath, path);
                filesDic.Add(newFile.GetHashMD5(), newFile);
            }
            return filesDic;                    
        }
    
        public static MyDirectory GetDirectories(string path)
        {
            var root = new MyDirectory(new DirectoryInfo(path));
            WalkDirectory3(root, path);
            return root;
        }

        private static void WalkDirectory3(MyDirectory directory, string initialPath)
        {
            var subDirs = directory.Info.GetDirectories();
            foreach (var dirInfo in subDirs)
            {
                var folder = new MyDirectory(dirInfo);
                folder.CreateRelativePath(initialPath, folder.Info.FullName);
                directory.AddDirectory(folder);
                WalkDirectory3(folder, initialPath);
            }
        }
    }
}