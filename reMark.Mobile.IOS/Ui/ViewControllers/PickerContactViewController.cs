using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class PickerContactViewController : AbstractContactViewController
    {
        readonly TaskCompletionSource<Recipient> tcs = new TaskCompletionSource<Recipient>();
        public Task<Recipient> Result => tcs.Task;

        protected override void SetAlphaForHeaderButtons()
        {
            EmailButton.Enabled = Contact.CommunicationAddresses.Any(ca => ca.Type == CommunicationAddressType.Email && ca.IsPrimary);
            MobileButton.Enabled = false;
            SmsButton.Enabled = false;
            MapButton.Enabled = false;

            EmailButton.Alpha = 1f;
            MobileButton.Alpha = 1f;
            SmsButton.Alpha = 1f;
            MapButton.Alpha = 1f;
        }

        protected override void EmailButton_TouchUpInside(object sender, EventArgs e)
        {
            CommonConfig.UsageAnalytics.LogEvent(new ContactFastActionEvent(ContactActionChoice.Email));

            var primaryEmail = Contact.CommunicationAddresses.FirstOrDefault(ca => ca.Type == CommunicationAddressType.Email && ca.IsPrimary);
            if (primaryEmail == null)
                return;

            tcs.SetResult(new Recipient(ContactPreview.Name, primaryEmail.Address, RecipientType.Contact));

            DismissViewController(true, null);
        }

        protected override void CommunicationAddressClicked(UITableView tv, UITableViewCell cell, CommunicationAddress ca)
        {   
            if (ca.Type == CommunicationAddressType.Email)
            {
                tcs.SetResult(new Recipient(ContactPreview.Name, ca.Address, RecipientType.Contact));
                DismissViewController(true, null);
            }
        }

        protected override async void LinkedContactClicked(ContactPreview contactPreview)
        {
            var vc = new PickerContactViewController();
            vc.SetData(contactPreview, true);
            vc.SetRefreshDataOnAppear();
            NavigationController.PushViewController(vc, true);

            var result = await vc.Result;
            if (result != null)
            {
                if (!tcs.TrySetResult(result))
                    CommonConfig.Logger.Error("Result was already set!");
            }
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            if (coder.DecodeBool("doNotRestore"))
                return null;

            return new PickerContactViewController();
        }

        #region Unused overrides
        protected override void ActionsLinksButton_Clicked(object sender, EventArgs e)
        {
            //Do nothing
        }

        protected override void AssignCategoryButton_Clicked(object sender, EventArgs e)
        {
            //Do nothing
        }

        protected override void MobileButton_TouchUpInside(object sender, EventArgs e)
        {
            //Do nothing
        }

        protected override void SmsButton_TouchUpInside(object sender, EventArgs e)
        {
            //Do nothing
        }

        protected override void MapButton_TouchUpInside(object sender, EventArgs e)
        {
            //Do nothing
        }

        protected override void CommentsButton_Clicked(object sender, EventArgs e)
        {
            //Do nothing
        }

        protected override void DoneButtonItem_Clicked(object sender, EventArgs e)
        {
            //Do nothing
        }

        protected override void EditButtonItem_Clicked(object sender, EventArgs e)
        {
            //Do nothing
        }

        protected override void FileToButton_Clicked(object sender, EventArgs e)
        {
            //Do nothing
        }
        protected override void PhysicalAddressClicked(UITableView tv, UITableViewCell cell, PhysicalAddress pa)
        {
            //Do nothing
        }
        #endregion
    }
}
