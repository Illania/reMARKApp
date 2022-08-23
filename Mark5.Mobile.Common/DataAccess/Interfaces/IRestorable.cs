using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.DataAccess.Interfaces
{
    public interface IRestorable
    {
        Task RestoreDeletedObjectsAsync(List<int> ids);
        Task SaveDeletedObjectsAsync<T>(List<T> businessEntities) where T : IBusinessEntity;
    }
}
