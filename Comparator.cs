using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace CatalogSyncher
{
    //internal or public?
    public class Comparator
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static void CompareTwoFileDictionaries(IDictionary<byte[], MyFile> source, IDictionary<byte[], MyFile> replic)
        {
            foreach (var item in source)
            {
                if (!replic.TryGetValue(item.Key, out MyFile currentReplicItem))
                {
                    item.Value.WasChecked = true;
                    item.Value.CatalogItemAction = CatalogItemAction.Create;
                    continue;
                }
                //если файл перенесли или переименовали, то поменять положение
                if (item.Value.RelativePath != currentReplicItem.RelativePath)
                {
                    currentReplicItem.RelativePath = item.Value.RelativePath;
                    currentReplicItem.CatalogItemAction = CatalogItemAction.Rename;
                }
                item.Value.WasChecked = true;
                currentReplicItem.WasChecked = true;
            }
            var removableFiles = replic.Where(i => i.Value.WasChecked == false).Select(i => i.Value);
            foreach (var item in removableFiles)
            {
                item.CatalogItemAction = CatalogItemAction.Delete;
                item.WasChecked = true;
            }
        }

        public static void CompareTwoDirectories(MyDirectory source, MyDirectory replic)
        {
            if(source.RelativePath == replic.RelativePath)
            {
                var subSourceDirs = source.Subdirectories;
                var subReplicDirs = replic.Subdirectories;
                CompareSubdirectories(subSourceDirs, subReplicDirs);
            }
            FindRemovableDirectories(replic);
        }

        private static void FindRemovableDirectories(MyDirectory replic)
        {
            var subDirs = replic.Subdirectories;
            if (subDirs != null)
            {
                foreach (var item in subDirs)
                {
                    if(item.WasChecked == false)
                    {
                        item.CatalogItemAction = CatalogItemAction.Delete;
                    }
                    FindRemovableDirectories(item);
                }
            }
        }

        private static void CompareSubdirectories(IReadOnlyCollection<MyDirectory> source, IReadOnlyCollection<MyDirectory> replic)
        {
            if(source != null)
            {
                HashSet<MyDirectory> hsReplic = null;
                if(replic != null)
                {
                    hsReplic = new(new RelativePathEqualityComparer());
                    foreach (var item in replic)
                    {
                        hsReplic.Add(item);
                    }
                }
                foreach (var item in source)
                {
                    if(hsReplic.TryGetValue(item, out MyDirectory currentDir))
                    {
                        currentDir.WasChecked = true;
                        CompareSubdirectories(item.Subdirectories, currentDir.Subdirectories);
                    }
                    else
                    {
                        item.CatalogItemAction = CatalogItemAction.Create;
                        MarkAllSubdirectoriesAsAddable(item);
                    }
                    item.WasChecked = true;
                }
            }
            
        }

        private static void MarkAllSubdirectoriesAsAddable(MyDirectory root)
        {
            var subDirs = root.Subdirectories;
            if(subDirs != null)
            {
                foreach (var item in subDirs)
                {
                    item.CatalogItemAction = CatalogItemAction.Create;
                    MarkAllSubdirectoriesAsAddable(item);
                }
            }
        }
    }
}