using System.Threading.Tasks;

namespace Mark5.Mobile.Common
{
    public interface ICleaningManager
    {
        Task RemoveOrphans();
    }
}

