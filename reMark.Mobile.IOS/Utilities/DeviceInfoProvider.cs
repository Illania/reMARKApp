using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Utilities;
using Security;
using UIKit;
using DeviceType = reMark.Mobile.Common.Model.DeviceType;

namespace reMark.Mobile.IOS.Utilities
{
    public class DeviceInfoProvider : IDeviceInfoProvider
    {
        public DeviceType GetDeviceType()
        {
            return DeviceType.IOS;
        }

        public string GetDeviceId()
        {
            var rec = new SecRecord(SecKind.GenericPassword)
            {
                Generic = NSData.FromString("MARK5_Device_ID")
            };

            var match = SecKeyChain.QueryAsRecord(rec, out SecStatusCode res);

            if (res == SecStatusCode.Success)
                return match.ValueData.ToString();

            var newId = Guid.NewGuid().ToString();

            rec = new SecRecord(SecKind.GenericPassword)
            {
                Label = "MARK5 Device ID",
                Description = "MARK5 Device Identifier",
                Generic = NSData.FromString("MARK5_Device_ID"),
                ValueData = NSData.FromString(newId)
            };

            var err = SecKeyChain.Add(rec);

            if (err != SecStatusCode.Success && err != SecStatusCode.DuplicateItem)
                CommonConfig.Logger.Error($"Could not save Device ID to keychain. Error: {err}");

            return newId;
        }

        public string GetDeviceName()
        {
            return $"{Integration.GetModelNumber()} - {UIDevice.CurrentDevice.Name}";
        }

        public string GetAppVersionString()
        {
            return NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleShortVersionString")) + " (" + NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleVersion")) + ")";
        }
    }
}