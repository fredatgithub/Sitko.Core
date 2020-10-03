using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitko.Core.Storage.Cache;

namespace Sitko.Core.Storage
{
    public abstract class Storage<T> : IStorage<T>, IAsyncDisposable where T : StorageOptions
    {
        protected readonly ILogger<Storage<T>> Logger;
        private readonly IStorageCache? _cache;
        private readonly T _options;
        private StorageFolder? _tree;
        private DateTimeOffset? _treeLastBuild;

        protected Storage(T options, ILogger<Storage<T>> logger, IStorageCache? cache)
        {
            Logger = logger;
            _cache = cache;
            _options = options;
        }

        public async Task<StorageItem> SaveFileAsync(Stream file, string fileName, string path)
        {
            string destinationPath = GetDestinationPath(fileName, path);

            var storageItem = CreateStorageItem(file, fileName, destinationPath);

            var result = await SaveStorageItemAsync(file, path, destinationPath, storageItem);
            await BuildStorageTreeAsync();
            return result;
        }


        private async Task<StorageItem> SaveStorageItemAsync(Stream file, string path, string destinationPath,
            StorageItem storageItem)
        {
            file.Seek(0, SeekOrigin.Begin);
            await DoSaveAsync(destinationPath, file);
            Logger.LogInformation("File saved to {Path}", path);
            if (_cache != null && storageItem.FilePath != null)
            {
                await _cache.RemoveItemAsync(storageItem.FilePath);
            }

            return storageItem;
        }

        private string GetDestinationPath(string fileName, string path)
        {
            var destinationName = GetStorageFileName(fileName);
            var destinationPath = PreparePath($"{path}/{destinationName}")!;
            return destinationPath;
        }

        private StorageItem CreateStorageItem(Stream file, string fileName, string destinationPath)
        {
            var storageItem = new StorageItem
            {
                FileName = fileName,
                FileSize = file.Length,
                FilePath = destinationPath,
                Path = PreparePath(Path.GetDirectoryName(destinationPath))!,
            };
            return storageItem;
        }

        protected abstract Task<bool> DoSaveAsync(string path, Stream file);
        protected abstract Task<bool> DoDeleteAsync(string filePath);

        protected abstract Task<bool> DoIsFileExistsAsync(StorageItem item);
        protected abstract Task DoDeleteAllAsync();
        protected abstract Task<StorageItem?> DoGetFileAsync(string path);

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            if (_cache != null)
            {
                await _cache.RemoveItemAsync(filePath);
            }

            var result  = await DoDeleteAsync(filePath);
            await BuildStorageTreeAsync();
            return result;
        }

        public Task<StorageItem?> GetFileAsync(string path)
        {
            return GetFileInternalAsync(path);
        }

        protected virtual Task<StorageItem?> GetFileInternalAsync(string path)
        {
            if (_cache != null)
            {
                return _cache.GetOrAddItemAsync(path, () => DoGetFileAsync(path));
            }

            return DoGetFileAsync(path);
        }


        public async Task<bool> IsFileExistsAsync(string path)
        {
            var result = await GetFileInternalAsync(path);
            return result != null;
        }

        public async Task DeleteAllAsync()
        {
            if (_cache != null)
            {
                await _cache.ClearAsync();
            }

            await DoDeleteAllAsync();
            _tree = null;
            _treeLastBuild = null;
        }


        public async Task<IEnumerable<IStorageNode>> GetDirectoryContentsAsync(string path)
        {
            if (_tree == null || _treeLastBuild < DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(30)))
            {
                await BuildStorageTreeAsync();
            }

            if (_tree == null) { return new List<IStorageNode>(); }

            var parts = PreparePath(path.Trim('/'))!.Split("/");
            var current = _tree;
            foreach (var part in parts)
            {
                current = current?.Children.OfType<StorageFolder>().FirstOrDefault(f => f.Name == part);
            }

            return current?.Children ?? new IStorageNode[0];
        }

        private async Task BuildStorageTreeAsync()
        {
            _tree = await DoBuildStorageTreeAsync();
            _treeLastBuild = DateTimeOffset.UtcNow;
        }

        protected abstract Task<StorageFolder?> DoBuildStorageTreeAsync();

        public Uri PublicUri(StorageItem item)
        {
            return new Uri($"{_options.PublicUri}/{item.FilePath}");
        }

        private string GetStorageFileName(string fileName)
        {
            var extension = fileName.Substring(fileName.LastIndexOf('.'));
            return Guid.NewGuid() + extension;
        }

        public virtual ValueTask DisposeAsync()
        {
            return new ValueTask();
        }

        protected string? PreparePath(string? path)
        {
            return path?.Replace("\\", "/").Replace("//", "/");
        }
    }
}
