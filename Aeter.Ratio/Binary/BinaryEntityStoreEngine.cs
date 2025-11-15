using Aeter.Ratio.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary
{
    public class BinaryEntityStoreEngine(BinaryEntityStoreFileSystem fileSystem, BinaryStore store, BinaryEntityStoreHeader header, Scheduler scheduler, BinaryEntityStoreToc toc)
    {
        public static async Task<BinaryEntityStoreEngine> CreateAsync(string path, BinaryBufferPool bufferPool)
        {
            var fileSystem = new BinaryEntityStoreFileSystem(path);
            var store = new BinaryStore(fileSystem.Store.Path, bufferPool);
            BinaryEntityStoreHeader header;
            if (store.Size == 0) {
                header = new BinaryEntityStoreHeader();
                var space = await store.WriteAsync(0, header.Size);
                await header.WriteToAsync(space);
            }
            else {
                var space = await store.ReadAsync(0);
                header = await BinaryEntityStoreHeader.ReadFromAsync(space);
            }
            var scheduler = new Scheduler();
            var toc = await BinaryEntityStoreToc.CreateAsync(fileSystem.TableOfContent.Path, bufferPool, store, scheduler);
            return new BinaryEntityStoreEngine(fileSystem, store, header, scheduler, toc);
        }

        //public async Task AddAsync(object entity)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task UpdateAsync(object entity)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task DeleteAsync(object entity)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<object> GetAsync(object id)
        //{
        //    throw new NotImplementedException();
        //}
    }

    public class BinaryEntityStoreToc(BinaryStore store, List<BinaryEntityStoreTocEntry> entries, BinaryBufferPool bufferPool, ScheduledTaskHandle? initHandle = null)
    {
        public static async Task<BinaryEntityStoreToc> CreateAsync(string path, BinaryBufferPool bufferPool, BinaryStore entityStore, Scheduler scheduler)
        {
            var store = new BinaryStore(path, bufferPool);
            if (store.Size == 0) {
                if (entityStore.Size == 0) {
                    return new BinaryEntityStoreToc(store, [], bufferPool);
                }
                else {
                    var entries = new List<BinaryEntityStoreTocEntry>();
                    var initHandle = scheduler.Schedule(async state => {
                        if (state is null) throw new ArgumentNullException(nameof(state));
                        var (entries, entityStore, bufferPool) = ((List<BinaryEntityStoreTocEntry>, BinaryStore, BinaryBufferPool)) state;

                        
                    }, (entries, entityStore, bufferPool));

                    return new BinaryEntityStoreToc(store, entries, bufferPool, initHandle);
                }
            }
            else {
                var entries = new List<BinaryEntityStoreTocEntry>();
                var offset = 0;
                while (offset < store.Size) {
                    
                }
                return new BinaryEntityStoreToc(store, entries, bufferPool);
            }
        }
    }

    public class BinaryEntityStoreHeader(uint version = 1, ARID? serializerType = null)
    {
        private const int V1Size = 14;
        public int Size => V1Size;
        public uint Version { get; } = version;
        public ARID SerializerType { get; } = serializerType ?? Serialization.Bson.BsonSerializer.ARID;

        public async Task WriteToAsync(BinaryWriteBuffer buffer, CancellationToken cancellationToken = default)
        {
            var space = await buffer.WriteAsync(Size, cancellationToken);
            BinaryInformation.UInt32.Converter.Convert(Version, space.Span[..4]);
            SerializerType.WriteTo(space.Span.Slice(4, 10));
        }

        public static async Task<BinaryEntityStoreHeader> ReadFromAsync(BinaryReadBuffer buffer, CancellationToken cancellationToken = default)
        {
            var space = await buffer.ReadAsync(V1Size, cancellationToken);
            var version = BinaryInformation.UInt32.Converter.Convert(space.Span[..4]);
            var serializerType = ARID.ReadFrom(space.Span.Slice(4, 10));
            return new BinaryEntityStoreHeader(version, serializerType);
        }
    }
    

    public class BinaryEntityStoreTocEntry
    {
        public long Offset { get; set; }
        public int Size { get; set; }
    }

    public class BinaryEntityStoreFileSystem
    {
        private readonly List<BinaryEntityStoreFile> indexes;
        public BinaryEntityStoreFileSystem(string path)
        {
            Store = new BinaryEntityStoreFile(path);
            Folder = System.IO.Path.ChangeExtension(path, ".ar.cache");
            if (System.IO.Directory.Exists(Folder)) {
                indexes = [.. System.IO.Directory.GetFiles(Folder, "*.index").Select(f => new BinaryEntityStoreFile(f))];
            }
            else {
                System.IO.Directory.CreateDirectory(Folder);
                indexes = [];
            }
            TableOfContent = new BinaryEntityStoreFile(System.IO.Path.Combine(Folder, "ar.toc"));
        }

        public BinaryEntityStoreFile Store { get; }
        private string Folder { get; }
        public BinaryEntityStoreFile TableOfContent { get; }
        public IReadOnlyList<BinaryEntityStoreFile> Indexes => indexes;
    }
}
