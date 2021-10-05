using System;
using System.Linq;
using System.Threading;
using NLog;

namespace CatalogSyncher
{
    public class SyncManager
    {
        private readonly string _sourcePath;
        private readonly string _replicPath;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public SyncManager(string sourcePath, string replicPath)
        {
            _sourcePath = sourcePath;
            _replicPath = replicPath;
        }

        public void Sync()
        {
            var dicA = CatalogReader.GetFilesDictionary(_sourcePath);
            var dicB = CatalogReader.GetFilesDictionary(_replicPath);

            var sourceDirs = CatalogReader.GetDirectories(_sourcePath);
            var replicDirs = CatalogReader.GetDirectories(_replicPath);
            //todo везде юзать static или экземплярные
            Comparator.CompareTwoDirectories(sourceDirs, replicDirs);
            Comparator.CompareTwoFileDictionaries(dicA, dicB);
            var atLeastOneItemWasChanged = false;
            atLeastOneItemWasChanged |= CatalogManager.CreateDirectories(sourceDirs, _replicPath);//
            atLeastOneItemWasChanged |= CatalogManager.MoveFiles(dicB.Select(i => i.Value), _replicPath);
            atLeastOneItemWasChanged |= CatalogManager.CreateFiles(dicA.Select(i => i.Value), _replicPath);
            atLeastOneItemWasChanged |= CatalogManager.DeleteFiles(dicB.Select(i => i.Value));
            atLeastOneItemWasChanged |= CatalogManager.DeleteDirectories(replicDirs);
            if(!atLeastOneItemWasChanged)
            {
                _logger.Info("Catalogs are identical");
            }
        }
    }
}