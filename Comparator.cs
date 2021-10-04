using System;
using System.Collections.Generic;
using System.Linq;

namespace CatalogSyncher
{
    //internal or public?
    public class Comparator
    {

        public void CompareTwoFilesDictionaries(IDictionary<byte[], MyFile> source, IDictionary<byte[], MyFile> replic)
        {
            //CleanDictionaries(source, replic);
            //check not checked files
            // var sourceFiles = source.Where(i => i.Value.WasChecked = false).Select(i => i.Key, i.Value);
            // var replicFiles = replic.Where(i => i.Value.WasChecked = false);
            VerifyСorrectness(source, replic);
        }

        private void CleanDictionaries(IDictionary<byte[], MyFile> source, IDictionary<byte[], MyFile> replic)
        {
            //это листы
            var addableFiles = source.Except(replic).Select(i => i.Value).ToList();
            var removableFiles = replic.Except(source).Select(i => i.Value).ToList();
            MarkAsAddable(addableFiles);
            MarkAsRemovable(removableFiles);
        }

        private void VerifyСorrectness(IDictionary<byte[], MyFile> source, IDictionary<byte[], MyFile> replic)
        {
            foreach (var item in source)
            {
                if (!replic.TryGetValue(item.Key, out MyFile currentReplicItem))
                {
                    item.Value.WasChecked = true;
                    item.Value.CatalogItemAction = CatalogItemAction.Add;
                    continue;
                }
                //если файл перенесли или переименовали, то поменять положение
                if (item.Value.RelativePath != currentReplicItem.RelativePath)
                {
                    currentReplicItem.RelativePath = item.Value.RelativePath;
                    //currentReplicItem.CatalogItemAction = CatalogItemAction.Replace;
                    currentReplicItem.CatalogItemAction = CatalogItemAction.Rename;
                }
                //if(item.Value.Info.Name != currentReplicItem.Info.Name)
                // if(item.Value.RelativePath != currentReplicItem.RelativePath)
                // {
                //     //не здесь же переименовывать???
                //     currentReplicItem.CatalogItemAction = CatalogItemAction.Rename;
                // }
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
        public static void Compare(MyDirectory a, ref MyDirectory b)
        {
            if(a.Info.Name != b.Info.Name)
            {
                b.CatalogItemAction = CatalogItemAction.Rename;
            }
            var filesA = a.Files;
            var filesB = b.Files;
            CompareSetOfFiles(filesA, filesB);

            var dirs = a.Subdirectories;
            //?то же самое будет работать с папками
            
            
        }

        private static void CompareSetOfFiles(ICollection<MyFile> filesA, ICollection<MyFile> filesB)
        {
            if(filesA == null && filesB == null)
            {

            }
            if(filesA == null && filesB != null)
            {
                MarkAsRemovable(filesB);
            }
            if(filesA != null && filesB == null)
            {
                MarkAsAddable(filesA);
            }
            if(filesA != null && filesB != null)
            {
                CheckFiles(filesA, filesB);
            }
        }
        private static void CheckFiles(ICollection<MyFile> filesA, ICollection<MyFile> filesB)
        {
            var dicSource = new Dictionary<byte[], MyFile>();
            foreach (var item in filesA)
            {
                dicSource.Add(item.GetHashMD5(), item);
            }
            foreach (var item in filesB)
            {
                var itemHash = item.GetHashMD5();
                if(dicSource.ContainsKey(itemHash))
                {
                    if (dicSource[itemHash].Info.Name != item.Info.Name)
                    {
                        item.CatalogItemAction = CatalogItemAction.Rename;
                    }
                    dicSource[itemHash].WasChecked = true;
                    item.WasChecked = true;
                    dicSource.Remove(itemHash);
                }
            }
            MarkAsRemovable(filesB.Where(b => b.WasChecked == false));
            MarkAsAddable(dicSource.Values);
        }

        private static void MarkAsRemovable(IEnumerable<MyFile> collection)
        {
            foreach (var item in collection)
            {
                item.WasChecked = true;
                item.CatalogItemAction = CatalogItemAction.Delete;
            }
        }

        private static void MarkAsAddable(IEnumerable<MyFile> collection)
        {
            foreach (var item in collection)
            {
                item.WasChecked = true;
                item.CatalogItemAction = CatalogItemAction.Add;
            }
        }


        public void CompareTwoDirectories(MyDirectory source, MyDirectory replic)
        {
            if(source.RelativePath == replic.RelativePath)
            {
                var subSourceDirs = source.Subdirectories;
                var subReplicDirs = replic.Subdirectories;
                GoDeep(subSourceDirs, subReplicDirs);
            }
            FindRemovableDirectories(replic);
        }

        private void FindRemovableDirectories(MyDirectory replic)
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

        private void GoDeep(ICollection<MyDirectory> source, ICollection<MyDirectory> replic)
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
                        GoDeep(item.Subdirectories, currentDir.Subdirectories);
                    }
                    else
                    {
                        item.CatalogItemAction = CatalogItemAction.Add;
                        MarkAllSubdirectoriesAsAddable(item);
                    }
                    item.WasChecked = true;
                }
            }
            
        }

        private void MarkAllSubdirectoriesAsAddable(MyDirectory root)
        {
            var subDirs = root.Subdirectories;
            if(subDirs != null)
            {
                foreach (var item in subDirs)
                {
                    item.CatalogItemAction = CatalogItemAction.Add;
                    MarkAllSubdirectoriesAsAddable(item);
                }
            }
        }
    }
}