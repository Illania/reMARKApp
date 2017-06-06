using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Utilities
{
    public interface IDeviceInfoProvider
    {
        DeviceType GetDeviceType();

        string GetDeviceId();

        string GetDeviceName();

        string GetAppVersionString();
    }
}