using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace CatalogSyncher
{
    public class CatalogManager
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        //?
        //path - подразумевается root
        //relative path уже есть в файле
        public static void AddFiles(IEnumerable<MyFile> files, string path)
        {
            ProcessFile(files, path, CatalogItemAction.Add, File.Copy);
        }    

        private static void ProcessFile(IEnumerable<MyFile> files, string rootPath, CatalogItemAction action, Action<string, string> actionFunc)
        {
            var processableFiles = files.Where(i => i.CatalogItemAction == action);
            try
            {
                foreach (var item in processableFiles)
                {
                    var sourceFileName = item.Info.FullName;
                    var destFileName = Path.Combine(rootPath, item.RelativePath);
                    actionFunc(sourceFileName, destFileName);
                    _logger.Info("С файлом {file} была проведена операция {operation}", item.Info.Name, action);
                }
            }
            catch(Exception e)
            {
                _logger.Error(e, "couldn't add files");
            }
        }

        public static void MoveFiles(IEnumerable<MyFile> files, string path)
        {
            ProcessFile(files, path, CatalogItemAction.Rename, File.Move);
        }

        public static void DeleteFiles(IEnumerable<MyFile> files)
        {
            var removableFiles = files.Where(i => i.CatalogItemAction == CatalogItemAction.Delete);
            try
            {
                foreach (var item in removableFiles)
                {
                    item.Info.Delete();
                }
            }
            catch(Exception e)
            {
                _logger.Error(e, "todo");
            }
        }

        internal static void ManageDirectories(MyDirectory dir)
        {
            ICollection<MyDirectory> dirs = null;
            FindAddableOrRemovableDirectories(dir, ref dirs);
            ProcessFolders(dirs);
        }

        private static void ProcessFolders(ICollection<MyDirectory> dirs)
        {
            try
            {
                foreach (var item in dirs)
                {
                    switch(item.CatalogItemAction)
                    {
                        case CatalogItemAction.Add:
                            Directory.CreateDirectory(item.Info.FullName);
                            break;
                        case CatalogItemAction.Delete:
                            Directory.Delete(item.Info.FullName);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "The process failed: {0}", e.Message);
            }
        }

     

        public static void AddDirectories(MyDirectory sourceDir, string rootPath)
        {
            try
            {
                if(sourceDir.CatalogItemAction == CatalogItemAction.Add)
                {
                    var path = Path.Combine(rootPath, sourceDir.RelativePath);
                    Directory.CreateDirectory(path);
                }
                var subDirs = sourceDir.Subdirectories;
                if (subDirs != null)
                {
                    foreach (var item in subDirs)
                    {
                        AddDirectories(item, rootPath);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "The process failed: {0}", e.Message);
            }
        }

        public static void DeleteDirectories(MyDirectory sourceDir)
        {
            try
            {
                if(sourceDir.CatalogItemAction == CatalogItemAction.Delete)
                {
                    sourceDir.Info.Delete(true);
                }
                else
                {
                    var subDirs = sourceDir.Subdirectories;
                    if (subDirs != null)
                    {
                        foreach (var item in subDirs)
                        {
                            DeleteDirectories(item);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "The process failed: {0}", e.Message);
            }
        } 

        private static void FindAddableOrRemovableDirectories(MyDirectory dir, ref ICollection<MyDirectory> foundDirs)
        {
            var subDirs = dir.Subdirectories;
            if(subDirs != null)
            {
                if(dir.CatalogItemAction == CatalogItemAction.Add
                            || dir.CatalogItemAction == CatalogItemAction.Delete)
                {
                    foundDirs.Add(dir);
                }
                foreach (var item in subDirs)
                {
                    FindAddableOrRemovableDirectories(item, ref foundDirs);
                }
            }
        }
    }
}