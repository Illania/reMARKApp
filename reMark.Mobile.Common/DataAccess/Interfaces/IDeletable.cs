using System.Collections.Generic;
using System.Threading.Tasks;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Common.DataAccess.Interfaces
{
    public interface IDeletable
    {
        Task DeleteAsync(List<IBusinessEntity> businessEntities, bool saveBeforeDeletion = false);
    }
}
