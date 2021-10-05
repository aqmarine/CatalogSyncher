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

    public class MyDirectory : CatalogItem
    {
        private List<MyDirectory> _subdirectories;
        private List<MyFile> _files;

        public IReadOnlyCollection<MyDirectory> Subdirectories => _subdirectories;
        public IReadOnlyCollection<MyFile> Files => _files;
        public DirectoryInfo Info { get; private set; }
        public MyDirectory(string path) : base(path) { }

        public MyDirectory(DirectoryInfo di) : this(di.FullName)
        {
            Info = di;
        }

        public void CreateDirectory(MyDirectory item)
        {
            if (_subdirectories == null)
                _subdirectories = new List<MyDirectory>();
            _subdirectories.Add(item);
        }

        public void CreateFile(MyFile item)
        {
            if (_files == null)
                _files = new List<MyFile>();
            _files.Add(item);
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
        Create = 3
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