using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView
{
    public class ComposeDocumentViewController : AbstractWebViewController
    {
        const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB
        const int AutoSaveWorkingCopyInterval = 5000; // 5 seconds

        string DefaultTitle = Localization.GetString("new_document");

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
        string previousDocumentContent;

        UIBarButtonItem cancelButtonItem;
        UIBarButtonItem sendButtonItem;
        UIBarButtonItem attachmentButtonItem;

        UIStackView headerStackView;
        ToView toView;
        CcView ccView;
        BccView bccView;
        LineView lineView;
        PriorityView priorityView;
        SubjectView subjectView;
        AttachmentsView attachmentsView;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitNavigationBar();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

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

        void InitNavigationBar()
        {
            Title = DefaultTitle;

            cancelButtonItem = new UIBarButtonItem
            {
                Title = Localization.GetString("cancel")
            };
            sendButtonItem = new UIBarButtonItem
            {
                Title = Localization.GetString("send"),
                Enabled = false
            };
            attachmentButtonItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle(Path.Combine("icons", "attachment.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
            };

            NavigationItem.SetLeftBarButtonItem(cancelButtonItem, false);
            NavigationItem.SetRightBarButtonItems(new[] { sendButtonItem, attachmentButtonItem }, false);
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
            };

            headerStackView.AddArrangedSubview(toView = new ToView());
            headerStackView.AddArrangedSubview(ccView = new CcView());
            headerStackView.AddArrangedSubview(bccView = new BccView());
            headerStackView.AddArrangedSubview(lineView = new LineView(this));
            if (PlatformConfig.Preferences.ComposePriorityEnabled)
                headerStackView.AddArrangedSubview(priorityView = new PriorityView(this));
            headerStackView.AddArrangedSubview(subjectView = new SubjectView());
            headerStackView.AddArrangedSubview(attachmentsView = new AttachmentsView());

            SetHeaderView(headerStackView);
        }

        void InitializeHandlers()
        {
            cancelButtonItem.Clicked += CancelButtonItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            cancelButtonItem.Clicked -= CancelButtonItem_Clicked;
        }

        private void CancelButtonItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        async void RefreshData()
        {
            await LoadDocument();
            await LoadTemplate();
        }

        async Task LoadDocument()
        {
            if (documentLoaded)
                return;

            try
            {
                await StartRefreshing();

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

                var subViews = headerStackView.Subviews.OfType<ComposeDocumentSubView>().ToArray();
                foreach (var subView in subViews)
                {
                    subView.RestoreWorkingCopy = RestoreWorkingCopy;
                    subView.DocumentCreationModeFlag = DocumentCreationModeFlag;
                    subView.CopyToNewOption = CopyToNewOption;
                    subView.Document = document;
                    subView.DocumentPreview = documentPreview;
                    subView.PreviousDocumentDirection = PreviousDocumentDirection;
                    subView.PreviousDocument = previousDocument;
                    subView.PreviousDocumentPreview = previousDocumentPreview;
                    subView.PreconfiguredEmailAddresses = PreconfiguredEmailAddresses;

                    await subView.InitializeView();
                }

                if (RestoreWorkingCopy)
                {
                    var files = await Managers.DocumentsManager.GetDocumentWorkingCopyAttachmentsAsync();
                    attachmentsView.InitializeFileDescriptions(files.Select(f => new FileDescription(f)).ToArray());
                }

                LoadEditor();

                if (DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption == CopyToNewOption.KeepTextAndAttachments ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.Reply && CopyToNewOption == CopyToNewOption.None ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll && CopyToNewOption == CopyToNewOption.None ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.Forward && CopyToNewOption == CopyToNewOption.None)
                {
                    if (previousDocumentPreview != null && !string.IsNullOrWhiteSpace(previousDocument?.HtmlBody))
                    {
                        var config = HtmlProcessingConfiguration.DefaultForEditing;
                        config.ReplyHeaderParameters = GetReplyHeaderParameters(previousDocumentPreview);
                        previousDocumentContent = await ProcessHtml(previousDocument.HtmlBody, config);
                    }
                    else if (previousDocumentPreview != null && !string.IsNullOrWhiteSpace(previousDocument?.PlainTextBody))
                    {
                        var config = PlainTextProcessingConfiguration.DefaultForEditing;
                        config.ReplyHeaderParameters = GetReplyHeaderParameters(previousDocumentPreview);
                        previousDocumentContent = await ProcessPlainText(previousDocument.PlainTextBody, config);
                    }
                    else
                        previousDocumentContent = null;
                }

                if (previousDocumentContent != null)
                {
                    ToolbarItems = new[]
                    {
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                        new UIBarButtonItem(Localization.GetString("edit_original_email"), UIBarButtonItemStyle.Plain, async (sender, e) => {
                            var vc = new EditOriginalDocumentViewController { Content = previousDocumentContent };
                            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                            var editedContent = await vc.Result;
                            if (editedContent != null)
                                previousDocumentContent = editedContent;
                        }),
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
                    };
                    NavigationController.SetToolbarHidden(false, false);
                }

                await EndRefreshing();

                documentLoaded = true;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to load email into editor", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        async Task LoadTemplate()
        {
            if (templateLoaded)
                return;

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit)
                return;

            if (CopyToNewOption == CopyToNewOption.KeepTextAndAttachments)
                return;

            var useTemplate = PlatformConfig.Preferences.UseTemplate;

            switch (useTemplate)
            {
                case Preferences.TemplateUsageMode.Default:
                    await GetDefaultTemplate();
                    break;
                case Preferences.TemplateUsageMode.Local:
                    await GetLocalTemplate();
                    break;
                case Preferences.TemplateUsageMode.AlwaysAsk:
                    var templateListStrings = new string[]
                    {
                        Localization.GetString("template_selection_default"),
                        Localization.GetString("template_selection_local"),
                        Localization.GetString("template_selection_another")
                    };

                    var result = await Dialogs.ShowListActionSheetAsync(this, templateListStrings, View);
                    switch (result)
                    {
                        case 0:
                            await GetDefaultTemplate();
                            break;
                        case 1:
                            await GetLocalTemplate();
                            break;
                        case 2:
                            await GetAllTemplates();
                            break;
                    }
                    break;
            }

            templateLoaded = true;
        }

        #region Templates loading

        async Task GetDefaultTemplate()
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_template___"));

            try
            {
                var template = await Managers.DocumentsManager.GetDefaultTemplateAsync(DocumentCreationModeFlag);
                if (template == null)
                    return;

                var insertTemplateJs = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/insertTemplate", "js"));
                if (template.ContentType == ContentType.PlainText)
                {
                    var templateText = Regex.Replace(template.Content, @"\r\n?|\n", "\\n", RegexOptions.Multiline);
                    insertTemplateJs = ProcessWebTemplate(insertTemplateJs, "text", template.Id, templateText);
                }
                if (template.ContentType == ContentType.Html)
                {
                    var templateText = Regex.Replace(template.Content, @"\r\n?|\n", " ", RegexOptions.Multiline);
                    insertTemplateJs = ProcessWebTemplate(insertTemplateJs, "html", template.Id, templateText);
                }

                var result = await EvaluateJavaScriptAsync(insertTemplateJs);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting default template [PreviousDocumentId={PreviousDocumentId}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={DocumentCreationModeFlag}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        async Task GetLocalTemplate()
        {
            var insertTemplateJs = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/insertTemplate", "js"));
            var localTemplateText = Regex.Replace(PlatformConfig.Preferences.LocalTemplate, @"\r\n?|\n", "\\n", RegexOptions.Multiline);
            insertTemplateJs = ProcessWebTemplate(insertTemplateJs, "text", "local", localTemplateText);

            var result = await EvaluateJavaScriptAsync(insertTemplateJs);
        }

        async Task GetAllTemplates()
        {
            var tp = new TemplatesListViewController();
            PresentViewController(new NavigationController(tp, UIModalPresentationStyle.PageSheet), true, null);

            var templatePreview = await tp.Result;
            if (templatePreview == null)
                return;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_template___"));

            try
            {
                var template = await Managers.DocumentsManager.GetTemplateAsync(templatePreview.Id);
                if (template == null)
                    return;

                var insertTemplateJs = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/insertTemplate", "js"));
                if (template.ContentType == ContentType.PlainText)
                {
                    var templateText = Regex.Replace(template.Content, @"\r\n?|\n", "\\n", RegexOptions.Multiline);
                    insertTemplateJs = ProcessWebTemplate(insertTemplateJs, "text", template.Id, templateText);
                }
                if (template.ContentType == ContentType.Html)
                {
                    var templateText = Regex.Replace(template.Content, @"\r\n?|\n", " ", RegexOptions.Multiline);
                    insertTemplateJs = ProcessWebTemplate(insertTemplateJs, "html", template.Id, templateText);
                }

                var result = await EvaluateJavaScriptAsync(insertTemplateJs);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting template [templatePreview.Id={templatePreview?.Id}, PreviousDocumentId={PreviousDocumentId}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={DocumentCreationModeFlag}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        #endregion

        #region Utilities

        static string[] GetReplyHeaderParameters(DocumentPreview documentPreview)
        {
            var from = GetAddressTextFromPreviousDocument(documentPreview, DocumentAddressType.From);
            var date = documentPreview.DateReceivedTimestamp
                                      .ConvertTimestampMillisecondsToDateTime()
                                      .ConvertUtcToUserTime()
                                      .ConvertDateTimeToTimestampMilliseconds()
                                      .FormatUserTimestampAsTimeAndDateString();
            var to = GetAddressTextFromPreviousDocument(documentPreview, DocumentAddressType.To, DocumentAddressType.Cc);
            var subject = documentPreview.Subject;

            return new[] { from, date, to, subject };
        }

        static string GetAddressTextFromPreviousDocument(DocumentPreview documentPreview, params DocumentAddressType[] addressTypes)
        {
            var sb = new StringBuilder();
            var addresses = documentPreview.Addresses.Where(da => addressTypes.Contains(da.AddressType)).ToArray();
            for (var i = 0; i < addresses.Length; i++)
            {
                var hasName = !string.IsNullOrWhiteSpace(addresses[i].Name);
                if (hasName)
                    sb.Append(addresses[i].Name).Append(" &lt;");
                sb.Append(addresses[i].Address);
                if (hasName)
                    sb.Append("&gt;");
                if (i < addresses.Length - 1)
                    sb.Append(", ");
            }

            return sb.ToString();
        }

        #endregion

    }
}