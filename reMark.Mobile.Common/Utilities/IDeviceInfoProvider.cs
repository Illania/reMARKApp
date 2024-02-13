using reMark.Mobile.Common.Model;
using DeviceType = reMark.Mobile.Common.Model.DeviceType;

namespace reMark.Mobile.Common.Utilities
{
    public interface IDeviceInfoProvider
    {
        DeviceType GetDeviceType();

        string GetDeviceId();

        string GetDeviceName();

        string GetAppVersionString();
    }
}