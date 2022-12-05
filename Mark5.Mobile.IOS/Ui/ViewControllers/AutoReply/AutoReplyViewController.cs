using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Foundation;
using GMImagePicker;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Common.ShareExtension;
using Mark5.Mobile.IOS.Model;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells.AddEditAppointmentTableViewCell;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using Photos;
using UIKit;
using static Mark5.Mobile.IOS.Model.DateTimeChangeEvent;
using static SQLite.SQLite3;
using LineView = Mark5.Mobile.IOS.Ui.ViewControllers.AutoReply.LineView;
using SubjectView = Mark5.Mobile.IOS.Ui.ViewControllers.AutoReply.SubjectView;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.AutoReply
{
    public class AutoReplyViewController : AbstractWebViewController, IUIAdaptivePresentationControllerDelegate
    {

        readonly string DefaultTitle = Localization.GetString("autoreply_settings");
        public bool RestoreWorkingCopy { get; set; }
        readonly AutoReplyRule autoReplyRule = new();

        bool replyTextLoaded;
        bool refreshing;

        UIBarButtonItem cancelButtonItem;
        UIBarButtonItem saveButtonItem;

        UIStackView headerStackView;
        IsActiveView activeView;
        DateView startDateView;
        DateView endDateView;
        LineView lineView;
        SubjectView subjectView;

        public AutoReplyViewController(AutoReplyRule autoReplyRule)
        {
            allowsPasteAsText = true;
            this.autoReplyRule = autoReplyRule;
        }
        #region ViewController lifecycle methods

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitNavigationBar();
            InitializeView();

            NavigationController.PresentationController.Delegate = this;

        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                ModalInPresentation = true;

            InitializeHandlers();
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

            DeinitializeHandlers();
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

            cancelButtonItem = null;
            saveButtonItem = null;

            activeView?.RemoveFromSuperview();
            startDateView?.RemoveFromSuperview();
            endDateView?.RemoveFromSuperview();
            lineView?.RemoveFromSuperview();
            subjectView?.RemoveFromSuperview();

            activeView = null;
            startDateView = null;
            endDateView = null;
            lineView = null;
            subjectView = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        #endregion

        #region Initialization

        void InitNavigationBar()
        {
            Title = DefaultTitle;

            cancelButtonItem = new UIBarButtonItem
            {
                Title = Localization.GetString("cancel")
            };

            saveButtonItem = new UIBarButtonItem
            {
                Title = Localization.GetString("save")
            };

            NavigationItem.SetRightBarButtonItem(saveButtonItem, false);
            NavigationItem.SetLeftBarButtonItem(cancelButtonItem, false);
        }

        void InitializeView()
        {
            View.BackgroundColor = Theme.White;

            headerStackView = new UIStackView
            {
                BackgroundColor = Theme.White,
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = 0f,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            headerStackView.AddArrangedSubview(activeView = new IsActiveView());
            headerStackView.AddArrangedSubview(startDateView = new DateView(DateRowType.Starts, DateChanged));
            headerStackView.AddArrangedSubview(endDateView = new DateView(DateRowType.Ends, DateChanged));
            headerStackView.AddArrangedSubview(lineView = new LineView(this));
            headerStackView.AddArrangedSubview(subjectView = new SubjectView());

            var containerView = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.LightGray,
            };

            containerView.AddSubview(headerStackView);
            containerView.AddConstraints(new[]
            {
                headerStackView.TopAnchor.ConstraintEqualTo(containerView.TopAnchor),
                headerStackView.LeftAnchor.ConstraintEqualTo(containerView.LeftAnchor),
                headerStackView.RightAnchor.ConstraintEqualTo(containerView.RightAnchor),
                headerStackView.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor),
            });

            SetHeaderView(containerView);
        }

        void InitializeHandlers()
        {
            cancelButtonItem.Clicked += CancelButtonItem_Clicked;
            saveButtonItem.Clicked += SaveButtonItem_Clicked;

            activeView.Edited += Subview_Edited;
            startDateView.Edited += Subview_Edited;
            endDateView.Edited += Subview_Edited;
            lineView.Edited += Subview_Edited;
            subjectView.Edited += Subview_Edited;
        }

        void DeinitializeHandlers()
        {
            cancelButtonItem.Clicked -= CancelButtonItem_Clicked;
            saveButtonItem.Clicked -= SaveButtonItem_Clicked;

            activeView.Edited -= Subview_Edited;
            startDateView.Edited -= Subview_Edited;
            endDateView.Edited -= Subview_Edited;
            lineView.Edited -= Subview_Edited;
            subjectView.Edited -= Subview_Edited;
        }

        async void RefreshData()
        {
            if (refreshing)
                return;

            refreshing = true;
            await LoadReplyText();
            saveButtonItem.Enabled = true;
            refreshing = false;
        }

        async Task LoadReplyText()
        {
            if (replyTextLoaded)
                return;

            try
            {
                await StartRefreshing();

                var subViews = headerStackView.Subviews.OfType<AutoReplySubView>().ToArray();
                foreach (var subView in subViews)
                {
                    subView.AutoReplyRule = autoReplyRule;
                    await subView.InitializeView();
                  
                }
                CheckDates();

                if (!string.IsNullOrEmpty(autoReplyRule.ReplyText))
                    await LoadHtmlString(autoReplyRule.ReplyText, HtmlProcessingConfiguration.DefaultForEditing);
                else
                    LoadEditor();

                await EndRefreshing();

                replyTextLoaded = true;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to load reply text into editor", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        bool CheckDates()
       {
            if (autoReplyRule.ActiveTo < autoReplyRule.ActiveFrom)
            {
                endDateView.SetInvalidDateAndTime(autoReplyRule.ActiveTo);
                return false;        
            }
            return true;
        }

        #endregion

        #region Handlers

        [Export("presentationControllerDidDismiss:")]
        public async void DidDismiss(UIPresentationController presentationController)
        {

            await Managers.DocumentsManager.DeleteDocumentWorkingCopyAsync();
        }

        void CancelButtonItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        async void SaveButtonItem_Clicked(object sender, EventArgs e)
        {

            var subViews = headerStackView.Subviews.OfType<AutoReplySubView>().ToArray();
            foreach (var subView in subViews)
                await subView.UpdateAutoReplyRule();

            autoReplyRule.ReplyText = await GetContent();

            var result = await CheckDataIsValid();
            if (!result)
                return;

            await Managers.DocumentsManager.SetAutoReplyRule(autoReplyRule);
            DismissViewController(true, null); 
        }

        void Subview_Edited(object sender, EventArgs e)
        {
        }

        public void DateChanged(DateTimeChangeEvent args)
        {
            switch (args.rowType)
            {
                case DateRowType.Ends:
                    autoReplyRule.ActiveTo = args.selectedDate;
                    break;
                case DateRowType.Starts:
                    var previousStartDate = autoReplyRule.ActiveFrom.Date;
                    autoReplyRule.ActiveFrom = args.selectedDate;
                    if (autoReplyRule.ActiveTo < autoReplyRule.ActiveFrom)
                    {
                        var difference = (autoReplyRule.ActiveTo - previousStartDate).TotalMinutes;
                        autoReplyRule.ActiveTo = autoReplyRule.ActiveFrom.AddMinutes(difference);
                    }
                    break;
            }
        }


        #endregion


        #region Utilities

        async Task<bool> CheckDataIsValid()
        {
            if (subjectView.Empty)
            {
                var result = await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("warning"), Localization.GetString("no_subject_added"));
                if (!result)
                    return false;
            }
            if (!CheckDates())
            {
                var result = await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("warning"), Localization.GetString("active_to_less_than_active_from"));
                if (!result)
                    return false;
            }

            return true;
        }

        protected override async Task<string> GetContent()
        {
            var newContent = await base.GetContent();
            newContent = await HtmlUtilities.CleanContent(newContent);

            return newContent;
        }
        #endregion

    }
}