using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using InAppSettingsKit;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class LoginSettingsViewController : AbstractAppSettingsViewController, ISettingsDelegate
    {
        const string SslEnabledKey = "sslEnabled";
        const string CreateReportKey = "createReport";
        const string AppVersionKey = "appVersion";
        const string OpenSettingsAppKey = "openSettingsApp";

        public class SettingsValues
        {
            public SslMode SslMode { get; set; }
        }

        public event EventHandler<SettingsValues> RestrictedSettingsValuesUpdated;

        public LoginSettingsViewController(SettingsValues values)
        {
            Delegate = this;
            File = "Root.Login.inApp";
            ShowDoneButton = true;
            NeverShowPrivacySettings = true;
            ShowCreditsFooter = false;
            SettingsStore = new InMemorySettingsStore();

            SettingsStore.SetBool(values.SslMode != SslMode.Off, SslEnabledKey);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        public void SettingsViewControllerDidEnd(AppSettingsViewController sender)
        {
            if (RestrictedSettingsValuesUpdated != null)
            {
                var sslEnabled = SettingsStore.GetBool(SslEnabledKey);

                var rsv = new SettingsValues();

                if (sslEnabled)
                    rsv.SslMode = SslMode.On;
                else
                    rsv.SslMode = SslMode.Off;

                RestrictedSettingsValuesUpdated(this, rsv);
            }

            DismissViewController(true, null);
        }

        [Export("tableView:cellForSpecifier:")]
        public virtual UITableViewCell GetCellForSpecifier(UITableView tableView, SettingsSpecifier specifier)
        {
            if (specifier.Key == AppVersionKey)
            {
                var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateWithSideText("cell");
                cell.TextLabel.Text = specifier.Title;
                cell.DetailTextLabel.Text = $"{NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"]} ({NSBundle.MainBundle.InfoDictionary["CFBundleVersion"]})";
                return cell;
            }

            return null;
        }

        [Export("tableView:heightForSpecifier:")]
        public virtual nfloat GetHeightForSpecifier(UITableView tableView, SettingsSpecifier specifier)
        {
            switch (specifier.Key)
            {
                case AppVersionKey:
                    return 44f;
                default:
                    return 0f;
            }
        }

        [Export("settingsViewController:buttonTappedForSpecifier:")]
        public virtual void ButtonTappedForSpecifier(AppSettingsViewController sender, SettingsSpecifier specifier)
        {
            if (specifier.Key == CreateReportKey)
            {
                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("creating_system_report___"));

                Task.Run(() => { return SystemReportCollector.CreateFullReport(); })
                    .ContinueWith(t =>
                        {
                            dismissAction?.Invoke();

                            if (!t.IsFaulted)
                            {
                                var src = SystemReportCollector.CreateShareReportController(t.Result);
                                if (src.PopoverPresentationController != null)
                                    src.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(sender.TableView, sender.TableView.CellAt(sender.SettingsReader.GetIndexPath(specifier.Key)));
                                PresentViewController(src, true, null);
                            }
                        },
                        TaskScheduler.FromCurrentSynchronizationContext());
            }

            if (specifier.Key == OpenSettingsAppKey)
                UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString), new NSDictionary(), null);
        }

        public override nfloat GetHeightForFooter(UITableView tableView, nint section)
        {
            var footerText = SettingsReader.GetFooterText(section);

            if (string.IsNullOrWhiteSpace(footerText))
                return 0f;

            var width = tableView.Frame.Width - tableView.LayoutMargins.Left - tableView.LayoutMargins.Right;

            var size = new NSString(footerText).GetBoundingRect(new CGSize(width, nfloat.MaxValue),
                                                                NSStringDrawingOptions.UsesLineFragmentOrigin,
                                                                new UIStringAttributes { Font = Theme.DefaultFont },
                                                                null);

            return size.Height + 10f;
        }

        class InMemorySettingsStore : AbstractSettingsStore
        {
            readonly Dictionary<string, NSObject> values = new Dictionary<string, NSObject>();

            public override NSObject GetObject(string key)
            {
                return values.ContainsKey(key) ? values[key] : null;
            }

            public override void SetObject(NSObject value, string key)
            {
                values[key] = value;
            }
        }
    }
}