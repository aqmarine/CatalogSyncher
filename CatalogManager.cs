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

        public static bool CreateDirectories(MyDirectory sourceDir, string rootPath)
        {
            var atLeastOneWasCreated = false;
            if(sourceDir.CatalogItemAction == CatalogItemAction.Create)
            {
                var path = Path.Combine(rootPath, sourceDir.RelativePath);
                try
                {
                    Directory.CreateDirectory(path);
                    atLeastOneWasCreated = true;
                    _logger.Info("The directory {name} was created", path);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "The process of creating directory {name} was failed: {message}", sourceDir.RelativePath, e.Message);
                }
            }
            atLeastOneWasCreated |= PropagateActionToSubdirectories(sourceDir, subDir => CreateDirectories(subDir, rootPath));
            return atLeastOneWasCreated;
        }    

        public static bool DeleteDirectories(MyDirectory sourceDir)
        {
            var atLeastOneWasDeleted = false;
            if(sourceDir.CatalogItemAction == CatalogItemAction.Delete)
            {
                try
                {
                    sourceDir.Info.Delete(true);
                    atLeastOneWasDeleted = true;
                    _logger.Info("The directory {name} was deleted", sourceDir.RelativePath);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "The process of deleting directory {name} was failed: {message}", sourceDir.RelativePath, e.Message);
                }
            }
            else
            {
                atLeastOneWasDeleted |= PropagateActionToSubdirectories(sourceDir, DeleteDirectories);
            }
            return atLeastOneWasDeleted;
        }

        private static bool PropagateActionToSubdirectories(MyDirectory directory, Func<MyDirectory, bool> actionForSubDir)
        {
            var atLeastOneWasProcessed = false;
            var subDirs = directory.Subdirectories;
            if (subDirs != null)
            {
                foreach (var subDir in subDirs)
                {
                    atLeastOneWasProcessed |= actionForSubDir(subDir);
                }
            }
            return atLeastOneWasProcessed;
        }

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
    }
}