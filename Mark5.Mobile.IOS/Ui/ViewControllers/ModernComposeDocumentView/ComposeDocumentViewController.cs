using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ModernComposeDocumentView
{
    public class ComposeDocumentViewController : AbstractViewController
    {

        public bool RestoreWorkingCopy { get; set; }

        public DocumentCreationModeFlag DocumentCreationModeFlag { get; set; } = DocumentCreationModeFlag.New;
        public CopyToNewOption CopyToNewOption { get; set; }

        public DocumentDirection PreviousDocumentDirection { get; set; }
        public int? PreviousDocumentFolderId { get; set; }
        public int? PreviousDocumentId { get; set; }
        public Dictionary<DocumentAddressType, string[]> PreconfiguredEmailAddresses { get; set; }

        DocumentPreview documentPreview = new DocumentPreview();
        Document document = new Document();

        DocumentPreview previousDocumentPreview;
        Document previousDocument;

        bool documentLoaded;
        bool templateLoaded;

        ContentView contentView;

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            await LoadDocument();
            await ShowDocument();
        }

        void InitializeNavigationBar()
        {
            Title = Localization.GetString("new_document");


            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem
            {
                Title = "Close"
            };
            NavigationItem.LeftBarButtonItem.Clicked += (sender, e) => DismissViewController(true, null);
        }

        void InitializeView()
        {
            contentView = new ContentView();
            contentView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(contentView);
            View.AddConstraints(new[]
            {
                contentView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                contentView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                contentView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                contentView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });

            NavigationController.SetToolbarHidden(false, false);
        }

        async Task LoadDocument()
        {
            if (documentLoaded)
                return;

            documentLoaded = true;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_document___"));

            try
            {
                if (RestoreWorkingCopy)
                {
                    var wc = await Managers.DocumentsManager.GetDocumentWorkingCopyAsync();

                    DocumentCreationModeFlag = wc.DocumentCreationModeFlag;
                    CopyToNewOption = wc.CopyToNewOption;
                    PreviousDocumentFolderId = wc.PreviousDocumentFolderId;
                    PreviousDocumentId = wc.PreviousDocumentId;
                    PreviousDocumentDirection = wc.PreviousDocumentDirection;
                    documentPreview = wc.DocumentPreview;
                    document = wc.Document;
                }

                if (DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption == CopyToNewOption.KeepOnlyAddresses ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption == CopyToNewOption.KeepTextAndAttachments ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption == CopyToNewOption.KeepOnlyAttachments ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.Reply && CopyToNewOption == CopyToNewOption.None ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll && CopyToNewOption == CopyToNewOption.None ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.Forward && CopyToNewOption == CopyToNewOption.None)
                {
                    var result = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(PreviousDocumentFolderId ?? -1, PreviousDocumentId.Value);
                    previousDocumentPreview = result.DocumentPreview;
                    previousDocument = result.Document;
                }
                else if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit &&
                         PreviousDocumentDirection == DocumentDirection.Draft &&
                         CopyToNewOption == CopyToNewOption.None)
                {
                    var result = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(PreviousDocumentFolderId ?? -1, PreviousDocumentId.Value);
                    previousDocumentPreview = result.DocumentPreview;
                    previousDocument = result.Document;

                    document.Id = PreviousDocumentId.Value;
                    documentPreview.Id = PreviousDocumentId.Value;
                }

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        async Task ShowDocument()
        {
            var views = new AbstractComposeDocumentView[] { contentView };
            foreach (var view in views)
            {
                view.RestoreWorkingCopy = RestoreWorkingCopy;
                view.DocumentCreationModeFlag = DocumentCreationModeFlag;
                view.CopyToNewOption = CopyToNewOption;
                view.Document = document;
                view.DocumentPreview = documentPreview;
                view.PreviousDocumentDirection = PreviousDocumentDirection;
                view.PreviousDocument = previousDocument;
                view.PreviousDocumentPreview = previousDocumentPreview;
                view.PreconfiguredEmailAddresses = PreconfiguredEmailAddresses;

                await view.InitializeView();
            }
        }
    }
}
