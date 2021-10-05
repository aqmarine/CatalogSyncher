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


        public static void CreateFiles(IEnumerable<MyFile> files, string rootPath)
        {
            ProcessFile(files, rootPath, CatalogItemAction.Create, File.Copy);
        }

        public static void MoveFiles(IEnumerable<MyFile> files, string rootPath)
        {
            ProcessFile(files, rootPath, CatalogItemAction.Rename, File.Move);
        }

        public static void DeleteFiles(IEnumerable<MyFile> files)
        {
            var removableFiles = files.Where(i => i.CatalogItemAction == CatalogItemAction.Delete);
            foreach (var item in removableFiles)
            {
                try
                {
                    item.Info.Delete();
                    _logger.Info("The file {file} was deleted ", item.RelativePath);
                }
                catch(Exception e)
                {
                    _logger.Error(e, "Couldn't delete file {name}", item.RelativePath);
                }
            }
            
        }

        public static void CreateDirectories(MyDirectory sourceDir, string rootPath)
        {
            if(sourceDir.CatalogItemAction == CatalogItemAction.Create)
            {
                var path = Path.Combine(rootPath, sourceDir.RelativePath);
                try
                {
                    Directory.CreateDirectory(path);
                    _logger.Info("The directory {name} was created", path);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "The process of creating directory {name} was failed: {message}", sourceDir.RelativePath, e.Message);
                }
            }
            PropagateActionToSubdirectories(sourceDir, subDir => CreateDirectories(subDir, rootPath));
        }    

        public static void DeleteDirectories(MyDirectory sourceDir)
        {
            if(sourceDir.CatalogItemAction == CatalogItemAction.Delete)
            {
                try
                {
                    sourceDir.Info.Delete(true);
                    _logger.Info("The directory {name} was deleted", sourceDir.RelativePath);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "The process of deleting directory {name} was failed: {message}", sourceDir.RelativePath, e.Message);
                }
            }
            else
            {
                PropagateActionToSubdirectories(sourceDir, DeleteDirectories);
            }
        }

        private static void PropagateActionToSubdirectories(MyDirectory directory, Action<MyDirectory> actionForSubDir)
        {
            var subDirs = directory.Subdirectories;
            if (subDirs != null)
            {
                foreach (var subDir in subDirs)
                {
                    actionForSubDir(subDir);
                }
            }
        }

        // internal static void ManageDirectories(MyDirectory dir)
        // {
        //     ICollection<MyDirectory> dirs = null;
        //     FindAddableOrRemovableDirectories(dir, ref dirs);
        //     ProcessFolders(dirs);
        // }

        // private static void ProcessFolders(ICollection<MyDirectory> dirs)
        // {
        //     try
        //     {
        //         foreach (var item in dirs)
        //         {
        //             switch(item.CatalogItemAction)
        //             {
        //                 case CatalogItemAction.Add:
        //                     Directory.CreateDirectory(item.Info.FullName);
        //                     break;
        //                 case CatalogItemAction.Delete:
        //                     Directory.Delete(item.Info.FullName);
        //                     break;
        //             }
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         _logger.Error(e, "The process failed: {0}", e.Message);
        //     }
        // }

        private static void ProcessFile(IEnumerable<MyFile> files, string rootPath, CatalogItemAction action, Action<string, string> actionFunc)
        {
            var processableFiles = files.Where(i => i.CatalogItemAction == action);
            
            foreach (var item in processableFiles)
            {
                try
                {
                    var sourceFileName = item.Info.FullName;
                    var destFileName = Path.Combine(rootPath, item.RelativePath);
                    actionFunc(sourceFileName, destFileName);
                    _logger.Info("The file {file} was {operation}ed ", item.RelativePath, action);
                }
                catch(Exception e)
                {
                    _logger.Error(e, "Couldn't {action} the file {name}", action, item.RelativePath);
                }
            }
        }



        // private static void FindAddableOrRemovableDirectories(MyDirectory dir, ref ICollection<MyDirectory> foundDirs)
        // {
        //     var subDirs = dir.Subdirectories;
        //     if(subDirs != null)
        //     {
        //         if(dir.CatalogItemAction == CatalogItemAction.Create
        //                     || dir.CatalogItemAction == CatalogItemAction.Delete)
        //         {
        //             foundDirs.Add(dir);
        //         }
        //         foreach (var item in subDirs)
        //         {
        //             FindAddableOrRemovableDirectories(item, ref foundDirs);
        //         }
        //     }
        // }
    }
}