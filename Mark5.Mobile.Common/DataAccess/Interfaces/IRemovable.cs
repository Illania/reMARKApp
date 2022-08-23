using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.DataAccess.Interfaces
{
    public interface IRemovable
    {
        Task RemoveFromFolderAsync(List<IBusinessEntity> businessEntities, int folderId, bool saveBeforeDeletion = false);
        Task RemoveFromFolderAsync(List<int> ids, int folderId);
    }
}
