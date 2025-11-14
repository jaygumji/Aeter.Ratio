using Aeter.Ratio.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary
{
    public class BinaryEntityStore(BinaryEntityStoreFileSystem fileSystem, BinaryStore store, Scheduler scheduler, BinaryEntityStoreToc toc)
    {
        public static async Task<BinaryEntityStore> CreateAsync(string path, BinaryBufferPool bufferPool)
        {
            var fileSystem = new BinaryEntityStoreFileSystem(path);
            var store = new BinaryStore(fileSystem.Store.Path, bufferPool);
            var scheduler = new Scheduler();
            var toc = await BinaryEntityStoreToc.CreateAsync(fileSystem.TableOfContent.Path, bufferPool, store, scheduler);
            return new BinaryEntityStore(fileSystem, store, scheduler, toc);
        }

        public async Task AddAsync(object entity)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(object entity)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(object entity)
        {
            throw new NotImplementedException();
        }

        public async Task<object> GetAsync(object id)
        {
            throw new NotImplementedException();
        }
    }

    public class BinaryEntityStoreToc(BinaryStore store, List<BinaryEntityStoreTocEntry> entries, BinaryBufferPool bufferPool)
    {
        public static async Task<BinaryEntityStoreToc> CreateAsync(string path, BinaryBufferPool bufferPool, BinaryStore entityStore, Scheduler scheduler)
        {
            var store = new BinaryStore(path, bufferPool);
            if (store.Size == 0) {
            }
            return new BinaryEntityStoreToc(store, new List<BinaryEntityStoreTocEntry>(), bufferPool);
        }
    }

    public class BinaryEntityStoreTocEntry
    {
    }

    public class BinaryEntityStoreFileSystem
    {
        private readonly List<BinaryEntityStoreFile> indexes;
        public BinaryEntityStoreFileSystem(string path)
        {
            Store = new BinaryEntityStoreFile(path);
            Folder = System.IO.Path.ChangeExtension(path, ".ar.cache");
            if (System.IO.Directory.Exists(Folder)) {
                indexes = System.IO.Directory.GetFiles(Folder, "*.index").Select(f => new BinaryEntityStoreFile(f)).ToList();
            }
            else {
                System.IO.Directory.CreateDirectory(Folder);
                indexes = new List<BinaryEntityStoreFile>();
            }
            TableOfContent = new BinaryEntityStoreFile(System.IO.Path.Combine(Folder, "ar.toc"));
        }

        public BinaryEntityStoreFile Store { get; }
        private string Folder { get; }
        public BinaryEntityStoreFile TableOfContent { get; }
        public IReadOnlyList<BinaryEntityStoreFile> Indexes => indexes;
    }
}
