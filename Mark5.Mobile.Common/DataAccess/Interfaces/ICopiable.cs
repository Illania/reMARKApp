using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mark5.Mobile.Common.DataAccess.Interfaces
{
    public interface ICopiable
    {
        Task CopyToFolderAsync(int folderId, List<int> documentIds);
    }
}
