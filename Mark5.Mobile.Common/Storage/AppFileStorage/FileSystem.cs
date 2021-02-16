using System;
using Mark5.Mobile.Common.Storage.AppFileStorage.Interface;

namespace Mark5.Mobile.Common.Storage.AppFileStorage
{
    /// <summary>
    /// Provides access to an implementation of <see cref="IFileSystem"/> for the current platform
    /// </summary>
    public static class FileSystem
    {
        static Lazy<IFileSystem> _fileSystem = new Lazy<IFileSystem>(() => CreateFileSystem(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// The implementation of <see cref="IFileSystem"/> for the current platform
        /// </summary>
        public static IFileSystem Current
        {
            get
            {
                IFileSystem ret = _fileSystem.Value;
                if (ret == null)
                {
                    throw NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }

        static IFileSystem CreateFileSystem()
        {
            return new AppFileSystem();
        }

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException("This functionality is not implemented.");
        }
    }
}