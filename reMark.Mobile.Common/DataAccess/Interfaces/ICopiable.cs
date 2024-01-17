using System.Collections.Generic;
using System.Threading.Tasks;

namespace reMark.Mobile.Common.DataAccess.Interfaces
{
    public interface ICopiable
    {
        Task CopyToFolderAsync(int folderId, List<int> documentIds);
    }
}
