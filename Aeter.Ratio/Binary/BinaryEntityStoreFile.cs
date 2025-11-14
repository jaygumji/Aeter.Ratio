namespace Aeter.Ratio.Binary
{
    public class BinaryEntityStoreFile
    {
        public BinaryEntityStoreFile(string path)
        {
            Path = path;
            FileName = System.IO.Path.GetFileName(path);
        }

        public string Path { get; }
        public string FileName { get; }
    }
}
