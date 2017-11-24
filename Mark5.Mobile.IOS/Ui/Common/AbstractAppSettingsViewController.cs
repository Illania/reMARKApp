using System;
using InAppSettingsKit;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public abstract class AbstractAppSettingsViewController : AppSettingsViewController
    {
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            CommonConfig.UsageAnalytics.SetScreen(GetType().Name);
        }
    }
}
