using System.Linq;
using NLog;

namespace CatalogSyncher
{
    public class SyncManager
    {
        private readonly string _sourcePath;
        private readonly string _replicaPath;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public SyncManager(string sourcePath, string replicaPath)
        {
            if (string.IsNullOrEmpty(sourcePath))
            {
                throw new System.ArgumentException($"\"{nameof(sourcePath)}\" не может быть неопределенным или пустым.", nameof(sourcePath));
            }
            if (string.IsNullOrEmpty(replicaPath))
            {
                throw new System.ArgumentException($"\"{nameof(replicaPath)}\" не может быть неопределенным или пустым.", nameof(replicaPath));
            }

            _sourcePath = sourcePath;
            _replicaPath = replicaPath;
        }

        public void Sync()
        {
            var sourceDict = CatalogReader.GetFilesDictionary(_sourcePath);
            var replicaDict = CatalogReader.GetFilesDictionary(_replicaPath);
            var sourceDirs = CatalogReader.GetDirectories(_sourcePath);
            var replicaDirs = CatalogReader.GetDirectories(_replicaPath);

            Comparator.CompareTwoDirectories(sourceDirs, replicaDirs);
            Comparator.CompareTwoFileDictionaries(sourceDict, replicaDict);

            var atLeastOneItemWasChanged = false;
            atLeastOneItemWasChanged |= CatalogManager.CreateDirectories(sourceDirs, _replicaPath);
            atLeastOneItemWasChanged |= CatalogManager.MoveFiles(replicaDict.Values, _replicaPath);
            atLeastOneItemWasChanged |= CatalogManager.CreateFiles(sourceDict.Values, _replicaPath);
            atLeastOneItemWasChanged |= CatalogManager.DeleteFiles(replicaDict.Values);
            atLeastOneItemWasChanged |= CatalogManager.DeleteDirectories(replicaDirs);
            if(!atLeastOneItemWasChanged)
            {
                _logger.Info("Catalogs are identical");
            }
        }
    }
}