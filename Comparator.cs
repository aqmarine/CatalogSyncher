using System.Collections.Generic;
using System.Linq;

namespace CatalogSyncher
{
    internal class Comparator
    {
        public static void CompareTwoFileDictionaries(IDictionary<byte[], MyFile> source, IDictionary<byte[], MyFile> replica)
        {
            foreach (var item in source)
            {
                if (!replica.TryGetValue(item.Key, out MyFile currentReplicaItem))
                {
                    item.Value.WasChecked = true;
                    item.Value.CatalogItemAction = CatalogItemAction.Create;
                    continue;
                }
                //если файл перенесли или переименовали, то поменять положение
                if (item.Value.RelativePath != currentReplicaItem.RelativePath)
                {
                    currentReplicaItem.RelativePath = item.Value.RelativePath;
                    currentReplicaItem.CatalogItemAction = CatalogItemAction.Rename;
                }
                item.Value.WasChecked = true;
                currentReplicaItem.WasChecked = true;
            }
            var removableFiles = replica.Where(i => i.Value.WasChecked == false).Select(i => i.Value);
            foreach (var item in removableFiles)
            {
                item.CatalogItemAction = CatalogItemAction.Delete;
                item.WasChecked = true;
            }
        }

        public static void CompareTwoDirectories(MyDirectory source, MyDirectory replica)
        {
            if(source.RelativePath == replica.RelativePath)
            {
                var subSourceDirs = source.Subdirectories;
                var subReplicaDirs = replica.Subdirectories;
                CompareSubdirectories(subSourceDirs, subReplicaDirs);
            }
            FindRemovableDirectories(replica);
        }

        private static void FindRemovableDirectories(MyDirectory replica)
        {
            var subDirs = replica.Subdirectories;
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

        private static void CompareSubdirectories(IReadOnlyCollection<MyDirectory> source, IReadOnlyCollection<MyDirectory> replica)
        {
            if(source != null)
            {
                HashSet<MyDirectory> hsReplica = null;
                if(replica != null)
                {
                    hsReplica = new(new RelativePathEqualityComparer());
                    foreach (var item in replica)
                    {
                        hsReplica.Add(item);
                    }
                }
                foreach (var item in source)
                {
                    if(hsReplica != null && hsReplica.TryGetValue(item, out MyDirectory currentDir))
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