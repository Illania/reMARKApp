using PCLStorage;
using System.IO;

namespace Mark5.Mobile.Droid.Model
{
    public class FileDescription
    {
        public string Name { get; }
        public long SizeInBytes { get; }
        public string Path { get; }
        public IFile File { get; }

        public FileDescription(IFile file)
        {
            Name = file.Name;
            SizeInBytes = new FileInfo(file.Path).Length;
            Path = file.Path;
            File = file;
        }
    }
}