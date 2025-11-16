using System.Collections.Generic;
using System.Linq;

namespace Aeter.Ratio.Binary.EntityStore
{
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
