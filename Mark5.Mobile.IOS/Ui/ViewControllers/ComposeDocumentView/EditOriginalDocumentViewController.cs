using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView
{
    public class EditOriginalDocumentViewController : AbstractWebViewController
    {
        public string Content { get; set; }

        readonly TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
        public Task<string> Result => tcs.Task;

        UIBarButtonItem doneItem;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            doneItem = new UIBarButtonItem(UIBarButtonSystemItem.Done)
            {
                Enabled = false
            };
            NavigationItem.SetRightBarButtonItem(doneItem, false);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            doneItem.Clicked += DoneItem_Clicked;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            doneItem.Clicked -= DoneItem_Clicked;
        }

        protected override void Recycle()
        {
            base.Recycle();

            doneItem = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        async void DoneItem_Clicked(object sender, EventArgs e)
        {
            doneItem.Enabled = false;
            var content = await GetContent();
            tcs.SetResult(content);
            DismissViewController(true, null);
        }

        async void RefreshData()
        {
            await StartRefreshing();
            await LoadHtmlString(Content, HtmlProcessingConfiguration.Disabled);
            await EndRefreshing();
            doneItem.Enabled = true;
        }
    }
}