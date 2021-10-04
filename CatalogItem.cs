using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;

namespace CatalogSyncher
{
    public class CatalogItem
    {
        public string Path { get; protected set; }
        public bool WasChecked {get; set;}

        public CatalogItemAction CatalogItemAction { get; set; }

        public string RelativePath { get; set; }

        public CatalogItem(string path)
        {
            Path = path;
            CatalogItemAction = 0;
        }

        public void CreateRelativePath(string source, string filePath)
        {
            RelativePath = System.IO.Path.GetRelativePath(source, filePath);
        }

    }

    public class MyDirectory: CatalogItem
    {
        public ICollection<MyDirectory> Subdirectories { get; private set; }
        public ICollection<MyFile> Files { get; private set; }
        public DirectoryInfo Info { get; private set; }
        public MyDirectory(string path): base(path) { }

        public MyDirectory(DirectoryInfo di): this (di.FullName)
        {
            Info = di;
        }

        public void AddDirectory(MyDirectory item)
        {
            //лучшее тут или в ctor?
            if(Subdirectories == null)
                Subdirectories = new List<MyDirectory>();
            Subdirectories.Add(item);
        }

        public void AddFile(MyFile item)
        {
            if(Files == null)
                Files = new List<MyFile>();
            Files.Add(item);
        }
    }

    public class MyFile: CatalogItem
    {
        private static readonly Lazy<MD5> _hash = new Lazy<MD5>(MD5.Create);
        
        public FileInfo Info { get; private set; }
        public MyFile(string path): base(path) { }

        public MyFile(FileInfo fi): base(fi.FullName)
        {
            Info = fi;
        }

        public byte[] GetHashMD5()
        {
            using var fileData = Info.OpenRead();
            return _hash.Value.ComputeHash(fileData);
        }

       
    }

    public enum CatalogItemAction
    {
        None = 0,
        Rename = 1,
        Delete = 2,
        Add = 3,
        Replace = 5
    }

    public class RelativePathEqualityComparer : IEqualityComparer<MyDirectory>
    {
        public bool Equals(MyDirectory x, MyDirectory y)
        {
            if(ReferenceEquals(x, y))
            {
                return true;
            }
            if(x == null || y == null)
            {
                return false;
            }
            return x.RelativePath == y.RelativePath;
        }

        public int GetHashCode([DisallowNull] MyDirectory obj)
        {
            return obj?.RelativePath?.GetHashCode() ?? 0;
        }
    }
}