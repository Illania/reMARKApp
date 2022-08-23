using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.DataAccess.Interfaces
{
    public interface IDeletable
    {
        Task DeleteAsync(List<IBusinessEntity> businessEntities, bool saveBeforeDeletion = false);
    }
}
