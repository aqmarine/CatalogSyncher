using System;
using System.Collections.Generic;
using System.IO;

namespace CatalogSyncher
{
    public class CatalogReader
    {
        public static MyDirectory WalkDirectoryTree(System.IO.DirectoryInfo root)
        {
            System.IO.FileInfo[] files;
            System.IO.DirectoryInfo[] subDirs;
            //Console.WriteLine(root.FullName);
            var catalog = new MyDirectory(root);
            files = root.GetFiles("*.*");
            //var tempCatalog = catalog;
            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    //Console.WriteLine(fi.FullName);
                    catalog.AddFile(new MyFile(fi));
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();
               

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
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
            //Console.WriteLine(root.FullName);
            var catalog = root;
            files = catalog.Info.GetFiles("*.*");
            //var tempCatalog = catalog;
            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    catalog.AddFile(new MyFile(fi));
                }

                // Now find all the subdirectories under this directory.
                subDirs = catalog.Info.GetDirectories();
               

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
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
    
    }
}