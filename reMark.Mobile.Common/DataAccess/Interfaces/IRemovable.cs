using System.Collections.Generic;
using System.Threading.Tasks;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Common.DataAccess.Interfaces
{
    public interface IRemovable
    {
        Task RemoveFromFolderAsync(List<IBusinessEntity> businessEntities, int folderId, bool saveBeforeDeletion = false);
        Task RemoveFromFolderAsync(List<int> ids, int folderId);
    }
}
