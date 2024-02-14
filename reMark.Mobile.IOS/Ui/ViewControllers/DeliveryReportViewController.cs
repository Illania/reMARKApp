using System;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.Common.Reports;
using reMark.Mobile.IOS.Utilities;
using UIKit;
using reMark.Mobile.Common;
using System.Threading;
using reMark.Mobile.IOS.Model;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class DeliveryReportViewController: AbstractWebViewController
    {
        private string referenceNumber;
        private TransmitDestination transmitDestination;

        UIBarButtonItem closeItem;
        CancellationTokenSource loadCts;

        public void SetData(string referenceNumber, TransmitDestination transmitDestination)
        {
            this.referenceNumber = referenceNumber;
            this.transmitDestination = transmitDestination;
        }


        #region UIViewController overrides

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (Integration.IsRunningAtLeast(11))
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;

        }

        public override void LoadView()
        {
            base.LoadView();
            closeItem = new UIBarButtonItem
            {
                Title = Localization.GetString("close")
            };

            NavigationItem.SetLeftBarButtonItem(closeItem, false);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            closeItem.Clicked += CloseItem_Clicked;

        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (IsRefreshing)
                Clear();


            RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            loadCts?.Cancel();
            loadCts = null;

            closeItem.Clicked -= CloseItem_Clicked;

        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();
            closeItem = null; 
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }


        #endregion

        void CloseItem_Clicked(object sender, EventArgs e)
        {

            if (NavigationController != null)
                NavigationController.ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;

            DismissViewController(true, null);
        }

        public async void RefreshData()
        {
            loadCts?.Cancel();
            loadCts = new CancellationTokenSource();
            var token = loadCts.Token;

            try
            {
                await StartRefreshing();

                var html = DeliveryStatusReport.GetReportHtml(new DeliveryReportData(referenceNumber,
                    transmitDestination.Address,
                    transmitDestination.Status));

                if (!string.IsNullOrWhiteSpace(html))
                    await LoadHtmlString(html, HtmlProcessingConfiguration.DefaultForViewing);

                if (token.IsCancellationRequested)
                    return;

                await EndRefreshing();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Delivery report load failed", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);

                if (SplitViewController == null || SplitViewController.Collapsed)
                {
                    if (PresentingViewController == null)
                        NavigationController?.PopViewController(true);
                    else
                        DismissViewController(true, null);
                }
                else
                {
                    ClearData();
                }
            }
        }


        public void ClearData()
        {
            loadCts?.Cancel();
            Clear();
        }
      
    }
}
