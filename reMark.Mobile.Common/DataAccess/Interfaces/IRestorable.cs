using System.Collections.Generic;
using System.Threading.Tasks;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Common.DataAccess.Interfaces
{
    public interface IRestorable
    {
        Task RestoreDeletedObjectsAsync(List<int> ids);
        Task SaveDeletedObjectsAsync<T>(List<T> businessEntities) where T : IBusinessEntity;
    }
}
