using System.Threading.Tasks;
using PCLStorage;

namespace Mark5.Mobile.Common.Extensions
{
    public static class IFolderExtensions
    {
        public static async Task MoveRecursivelyAsync(this IFolder folderToMove, IFolder destinationFolder, CreationCollisionOption collisionOptions)
        {
            var target = await destinationFolder.CreateFolderAsync(folderToMove.Name, collisionOptions);

            foreach (var file in await folderToMove.GetFilesAsync())
                await file.MoveAsync(target.Path);

            foreach (var folder in await folderToMove.GetFoldersAsync())
            {
                var subFolder = await target.CreateFolderAsync(folder.Name, collisionOptions);
                await MoveRecursivelyAsync(folder, subFolder, collisionOptions);
            }
        }
    }
}