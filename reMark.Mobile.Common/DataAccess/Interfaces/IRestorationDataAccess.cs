using System.Collections.Generic;
using System.Threading.Tasks;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Common.DataAccess.Interfaces
{
    public interface IRestorationDataAccess
    {     
        Task<List<DeletedObject>> GetDeletedObjectsAsync(List<int> ids, DeletedObjectType type, int maxItems = 500);
       
        Task SaveDeletedObjects<T>(List<T> businessEntities) where T : IBusinessEntity;
        
        Task SaveDeletedObjectLinkedFolders(int documentId, List<int> linkedFoldersIds);
    }
}
