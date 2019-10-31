using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using MessageUI;
using UIKit;
using Xamarin.Essentials;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class SystemReportCollector
    {
        public static bool CanMailReport => MFMailComposeViewController.CanSendMail;

        public static UIActivityViewController CreateShareReportController(string report)
        {
            var vc = new UIActivityViewController(new[] { new NSString(report) }, null)
            {
                ExcludedActivityTypes = new[]
                {
                    UIActivityType.AddToReadingList,
                    UIActivityType.AssignToContact,
                    UIActivityType.OpenInIBooks,
                    UIActivityType.PostToFacebook,
                    UIActivityType.PostToTencentWeibo,
                    UIActivityType.PostToTwitter,
                    UIActivityType.PostToVimeo,
                    UIActivityType.PostToWeibo,
                    UIActivityType.SaveToCameraRoll
                }
            };
            vc.SetValueForKey(NSObject.FromObject("reMARK iOS System Report"), new NSString("subject"));

            return vc;
        }

        public static ComposeDocumentViewController CreateShareReportComposeDocumentViewController(string preconfiguredContent)
        {
            return new ComposeDocumentViewController
            {
                PreconfiguredEmailAddresses = new Dictionary<DocumentAddressType, string[]>() { { DocumentAddressType.To, new string[] { "appfeedback@nordic-it.com" } } },
                PreconfiguredSubject = "reMARK iOS System Report",
                PreconfiguredContent = preconfiguredContent
            };
        }

        public static ComposeDocumentViewController CreateShareFeedbackComposeDocumentViewController(string preconfiguredContent)
        {
            return new ComposeDocumentViewController
            {
                PreconfiguredEmailAddresses = new Dictionary<DocumentAddressType, string[]>() { { DocumentAddressType.To, new string[] { "appfeedback@nordic-it.com" } } },
                PreconfiguredSubject = "reMARK iOS Feedback",
                PreconfiguredContent = preconfiguredContent
            };
        }

        public static MFMailComposeViewController CreateMailFeedbackController(string report)
        {
            var mcvc = new MFMailComposeViewController();
            mcvc.SetToRecipients(new[] { "appfeedback@nordic-it.com" });
            mcvc.SetSubject("reMARK iOS Feedback");
            mcvc.AddAttachmentData(NSData.FromString(report), "text/plain", "reMARK_iOS_System_Report.txt");
            mcvc.Finished += Mcvc_Finished;
            mcvc.NavigationBar.TintColor = Theme.DarkBlue;

            return mcvc;
        }

        static void Mcvc_Finished(object sender, MFComposeResultEventArgs e)
        {
            e.Controller.Finished -= Mcvc_Finished;
            e.Controller.DismissViewController(true, null);
        }

        public static string CreateFullReport()
        {
            var sb = new StringBuilder();

            sb.Append(CreateSystemInfoReport());
            sb.AppendLine();
            sb.Append(CreateLogReport());

            return sb.ToString();
        }

        public static Task<string> CreateFullReportAsync()
        {
            return Task.Run(() => CreateFullReport());
        }

        public static string CreateSystemInfoReport()
        {
            var sb = new StringBuilder();

            sb.AppendLine("===== General =====");
            sb.AppendLine("Platform: iOS");
            sb.AppendLine("Version: " + NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"] + " (" + NSBundle.MainBundle.InfoDictionary["CFBundleVersion"] + ")");
            sb.AppendLine("UTC Date: " + DateTime.UtcNow);
            sb.AppendLine("Local Date: " + DateTime.Now);
            sb.AppendLine();

            sb.AppendLine("===== Device information =====");
            sb.AppendLine("Name: " + UIDevice.CurrentDevice.Name);
            sb.AppendLine("UserInterfaceIdiom: " + UIDevice.CurrentDevice.UserInterfaceIdiom.ToString());
            sb.AppendLine("Model: " + UIDevice.CurrentDevice.Model);
            sb.AppendLine("Model number: " + Integration.GetModelNumber());
            sb.AppendLine("Installation ID: " + CommonConfig.DeviceInfoProvider.GetDeviceId());
            sb.AppendLine("LocalizedModel: " + UIDevice.CurrentDevice.LocalizedModel);
            sb.AppendLine("SystemName: " + UIDevice.CurrentDevice.SystemName);
            sb.AppendLine("SystemVersion: " + UIDevice.CurrentDevice.SystemVersion);
            sb.AppendLine("IsMultitaskingSupported: " + UIDevice.CurrentDevice.IsMultitaskingSupported);
            sb.AppendLine("BatteryMonitoringEnabled: " + UIDevice.CurrentDevice.BatteryMonitoringEnabled);
            sb.AppendLine("BatteryLevel: " + UIDevice.CurrentDevice.BatteryLevel);
            sb.AppendLine("BatteryState: " + UIDevice.CurrentDevice.BatteryState.ToString());
            sb.AppendLine();

            sb.AppendLine("===== Connection information =====");
            sb.AppendLine("Username: " + Managers.ActiveConnectionInfo?.Username);
            sb.AppendLine("Hostname: " + Managers.ActiveConnectionInfo?.Hostname);
            sb.AppendLine("Port: " + Managers.ActiveConnectionInfo?.Port);
            sb.AppendLine("SSL: " + Managers.ActiveConnectionInfo?.SslMode);
            sb.AppendLine("Friendly device name: " + Managers.ActiveConnectionInfo?.FriendlyDeviceName);
            sb.AppendLine("Installation ID: " + Managers.ActiveConnectionInfo?.InstallationId);
            sb.AppendLine("APNS Token: " + PlatformConfig.Preferences.PushNotificationToken);
            sb.AppendLine();

            sb.AppendLine("===== Server information =====");
            sb.AppendLine(Serializer.SerializeSelectively(ServerConfig.SystemSettings, new[] { (typeof(ContactsModuleInfo), nameof(ContactsModuleInfo.Countries)) }));
            sb.AppendLine();

            sb.AppendLine("===== Memory information =====");
            sb.AppendLine("Free disk space: " + Integration.GetFreeDiskSpace() / 1024 / 1024);
            sb.AppendLine("Total disk space: " + Integration.GetTotalDiskSpace() / 1024 / 1024);
            sb.AppendLine("[MONO] Total memory: " + GC.GetTotalMemory(false) / 1024);
            sb.AppendLine("[MONO] Total memory after GC: " + GC.GetTotalMemory(true) / 1024);
            sb.AppendLine();

            sb.AppendLine("===== Preferences =====");
            foreach (var kv in PlatformConfig.Preferences.All)
                sb.AppendLine(kv.Key + ": " + kv.Value);

            sb.AppendLine();

            return sb.ToString();
        }

        public static string CreateLogReport()
        {
            var sb = new StringBuilder();
            sb.Append("===== log =====");
            sb.AppendLine(((ConsoleAndFileLogger)CommonConfig.Logger).ReadLogFile());
            return sb.ToString();
        }

        public static string CreateExceptionReport(Exception ex)
        {
            var sb = new StringBuilder();

            sb.AppendLine("===== Exception =====");
            sb.AppendLine("Exception Type: " + ex.GetType());
            sb.AppendLine();
            sb.AppendLine("Stack trace: " + ex.StackTrace);

            return sb.ToString();
        }

        public static string CreateFailedDocumentReport(Exception ex)
        {
            var sb = new StringBuilder();

            sb.AppendLine("===== Document Failed Report =====");
            sb.Append(CreateSystemInfoReport());
            sb.AppendLine();
            sb.Append(CreateExceptionReport(ex));

            return sb.ToString();
        }
    }
}