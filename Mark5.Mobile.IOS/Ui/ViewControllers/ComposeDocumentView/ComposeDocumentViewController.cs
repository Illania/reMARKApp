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
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews;
using Mark5.Mobile.IOS.Utilities;
using MobileCoreServices;
using Photos;
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
        UIBarButtonItem insertButtonItem;
        UIBarButtonItem sendButtonItem;

        UIStackView headerStackView;
        ToView toView;
        CcView ccView;
        BccView bccView;
        LineView lineView;
        PriorityView priorityView;
        SubjectView subjectView;
        AttachmentsView attachmentsView;

        Worker autoSaveWorkingCopyWorker;

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
            insertButtonItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle(Path.Combine("icons", "create.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                Enabled = false
            };

            NavigationItem.SetLeftBarButtonItem(cancelButtonItem, false);
            NavigationItem.SetRightBarButtonItems(new[] { sendButtonItem, insertButtonItem }, false);
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
            insertButtonItem.Clicked += InsertButtonItem_Clicked;
            sendButtonItem.Clicked += SendButtonItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            cancelButtonItem.Clicked -= CancelButtonItem_Clicked;
            insertButtonItem.Clicked -= InsertButtonItem_Clicked;
            sendButtonItem.Clicked -= SendButtonItem_Clicked;
        }

        void CancelButtonItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        async void InsertButtonItem_Clicked(object sender, EventArgs e)
        {
            var d = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);
            var source = await Dialogs.ShowListActionSheetAsync(this, new[] { Localization.GetString("insert_template"), Localization.GetString("take_photo"), Localization.GetString("existing_photo"), Localization.GetString("browse_files") }, d);
            if (source < 0)
                return;

            if (source == 0)
                await InsertTemplate();

            if (source == 1)
                InsertNewPhoto(d);

            if (source == 2)
                InsertExistingPhoto(d);

            if (source == 3)
                InsertFile(d);
        }

        void SendButtonItem_Clicked(object sender, EventArgs e)
        {

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

            switch (PlatformConfig.Preferences.UseTemplate)
            {
                case Preferences.TemplateUsageMode.Default:
                    await InsertDefaultTemplate();
                    break;
                case Preferences.TemplateUsageMode.Local:
                    await InsertLocalTemplate();
                    break;
                case Preferences.TemplateUsageMode.AlwaysAsk:
                    var templateListStrings = new []
                    {
                        Localization.GetString("template_selection_default"),
                        Localization.GetString("template_selection_local"),
                        Localization.GetString("template_selection_another")
                    };

                    var result = await Dialogs.ShowListActionSheetAsync(this, templateListStrings, View);
                    switch (result)
                    {
                        case 0:
                            await InsertDefaultTemplate();
                            break;
                        case 1:
                            await InsertLocalTemplate();
                            break;
                        case 2:
                            await InsertTemplate();
                            break;
                    }
                    break;
            }

            templateLoaded = true;
        }

        #region Templates loading

        async Task InsertDefaultTemplate()
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_template___"));

            try
            {
                var template = await Managers.DocumentsManager.GetDefaultTemplateAsync(DocumentCreationModeFlag);
                if (template == null)
                    return;
                
                ProcessTemplate(template, previousDocumentPreview);

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

        async Task InsertTemplate()
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

                ProcessTemplate(template, previousDocumentPreview);

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

        async Task InsertLocalTemplate()
        {
            var insertTemplateJs = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/insertTemplate", "js"));
            var localTemplateText = Regex.Replace(PlatformConfig.Preferences.LocalTemplate, @"\r\n?|\n", "\\n", RegexOptions.Multiline);
            insertTemplateJs = ProcessWebTemplate(insertTemplateJs, "text", "local", localTemplateText);

            var result = await EvaluateJavaScriptAsync(insertTemplateJs);
        }

        #endregion

        #region Attachments

        void InsertNewPhoto(PopoverPresentationControllerDelegate d)
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeAddAttachmentEvent(AddAttachmentType.TakePhoto));

            var picker = new UIImagePickerController
            {
                AllowsEditing = false,
                SourceType = UIImagePickerControllerSourceType.Camera,
                CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Photo,
                CameraDevice = UIImagePickerControllerCameraDevice.Rear,
                Delegate = new ImagePickerControllerDelegate(this, HandleAttachmentImage),
                ModalPresentationStyle = UIModalPresentationStyle.PageSheet
            };
            if (picker.PopoverPresentationController != null)
                picker.PopoverPresentationController.Delegate = d;
            PresentViewController(picker, true, null);
        }

        void InsertExistingPhoto(PopoverPresentationControllerDelegate d)
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeAddAttachmentEvent(AddAttachmentType.PickPhoto));

            var picker = new UIImagePickerController
            {
                AllowsEditing = false,
                SourceType = UIImagePickerControllerSourceType.SavedPhotosAlbum,
                MediaTypes = new[] { UTType.Image.ToString() },
                Delegate = new ImagePickerControllerDelegate(this, HandleAttachmentImage),
                ModalPresentationStyle = UIModalPresentationStyle.PageSheet
            };
            if (picker.PopoverPresentationController != null)
                picker.PopoverPresentationController.Delegate = d;
            PresentViewController(picker, true, null);
        }

        void InsertFile(PopoverPresentationControllerDelegate d)
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeAddAttachmentEvent(AddAttachmentType.Local));

            var picker = new UIDocumentPickerViewController(new[]
            {
                    "public.content",
                    "public.data",
                    "public.msg",
                    "public.eml"
                }, UIDocumentPickerMode.Import)
            {
                Delegate = new DocumentMenuDelegate(this, HandleAttachmentUrl)
            };
            if (picker.PopoverPresentationController != null)
                picker.PopoverPresentationController.Delegate = d;
            PresentViewController(picker, true, null);
        }

        async void HandleAttachmentUrl(NSUrl url)
        {
            Stream stream = null;

            try
            {
                var filename = url.LastPathComponent;
                stream = new FileStream(url.Path, FileMode.Open, FileAccess.Read);
                var result = url.TryGetResource(NSUrl.FileSizeKey, out NSObject sizeObject, out NSError _error);

                if (!result)
                    throw new Exception(_error.ToString());

                var sizeInBytes = int.Parse(sizeObject.ToString());

                if (sizeInBytes > ServerConfig.SystemSettings.DocumentsModuleInfo.MaximumAttachmentSizeBytes)
                {
                    await Dialogs.ShowErrorAlertAsync(this, new Exception(Localization.GetString("attachment_too_big")));
                    return;
                }

                var file = await Managers.DocumentsManager.SaveDocumentWorkingCopyAttachmentAsync(filename, stream);
                attachmentsView.AddFileDescription(new FileDescription(file));
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to save attachment [Url={url}, PreviousDocumentId={PreviousDocumentId}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={DocumentCreationModeFlag}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, new Exception(Localization.GetString("error_saving_local_attachment")));
            }
            finally
            {
                stream?.Dispose();
            }
        }

        async void HandleAttachmentImage(string filename, NSData jpegData)
        {
            Stream stream = null;

            try
            {
                var sizeInBytes = (long)jpegData.Length;
                stream = jpegData.AsStream();

                if (sizeInBytes > ServerConfig.SystemSettings.DocumentsModuleInfo.MaximumAttachmentSizeBytes)
                {
                    await Dialogs.ShowErrorAlertAsync(this, new Exception(Localization.GetString("attachment_too_big")));
                    return;
                }

                var file = await Managers.DocumentsManager.SaveDocumentWorkingCopyAttachmentAsync(filename, stream);
                attachmentsView.AddFileDescription(new FileDescription(file));
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to save image [FileName={filename}, PreviousDocumentId={PreviousDocumentId}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={DocumentCreationModeFlag}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, new Exception(Localization.GetString("error_saving_local_attachment")));
            }
            finally
            {
                stream?.Dispose();
            }
        }

        #endregion

        #region Utilities

        static void ProcessTemplate(Template template, DocumentPreview documentPreview)
        {
            var templateContent = template.Content;

            var currentTime = DateTime.Now;
            var dateString = currentTime.ToString("dd-MM-yyyy");
            var timeString = currentTime.ToString("HH:mm");

            var fromNameString = string.Empty;
            if (documentPreview?.Addresses != null)
                fromNameString = documentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.From).Select(da => da.Name).FirstOrDefault() ?? string.Empty;

            if (template.ContentType == ContentType.Html)
            {
                templateContent = templateContent.Replace("&lt;FROMNAME&gt;", fromNameString);
                templateContent = templateContent.Replace("&lt;DATE&gt;", dateString);
                templateContent = templateContent.Replace("&lt;TIME&gt;", timeString);

                templateContent = templateContent.Replace("&lt;CUR&gt;", string.Empty);
                templateContent = templateContent.Replace("&lt;SOURCETEXT&gt;", string.Empty);
                templateContent = templateContent.Replace("&lt;COMPANYNAME&gt;", string.Empty);
                templateContent = templateContent.Replace("&lt;FROMNAMEWITHCOMPANY&gt;", string.Empty);
            }
            else
            {
                templateContent = templateContent.Replace("<FROMNAME>", fromNameString);
                templateContent = templateContent.Replace("<DATE>", dateString);
                templateContent = templateContent.Replace("<TIME>", timeString);

                templateContent = templateContent.Replace("<CUR>", string.Empty);
                templateContent = templateContent.Replace("<SOURCETEXT>", string.Empty);
                templateContent = templateContent.Replace("<COMPANYNAME>", string.Empty);
                templateContent = templateContent.Replace("<FROMNAMEWITHCOMPANY>", string.Empty);
            }

            template.Content = templateContent;
        }

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

        class ImagePickerControllerDelegate : UIImagePickerControllerDelegate
        {
            readonly WeakReference<ComposeDocumentViewController> viewControllerWeakReference;
            readonly Action<string, NSData> handler;

            public ImagePickerControllerDelegate(ComposeDocumentViewController vc, Action<string, NSData> handler)
            {
                viewControllerWeakReference = vc.Wrap();
                this.handler = handler;
            }

            public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info)
            {
                try
                {
                    NSData jpegImage;
                    using (var image = (UIImage)info[UIImagePickerController.OriginalImage])
                    {
                        jpegImage = image.AsJPEG();
                    }

                    string filename = null;
                    PHAsset asset = null;

                    if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                        asset = (PHAsset)info[UIImagePickerController.PHAsset];
                    else
                    {
                        var referenceUrl = (NSUrl)info[UIImagePickerController.ReferenceUrl];
                        if (referenceUrl != null)
                        {
                            var results = PHAsset.FetchAssets(new[] { referenceUrl }, null);
                            asset = (PHAsset)results.firstObject;
                        }
                    }

                    if (asset != null)
                        filename = PHAssetResource.GetAssetResources(asset)[0].OriginalFilename;
                    else
                        filename = "photo_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";

                    picker.DismissViewController(true, null);

                    handler(filename, jpegImage);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not pick media", ex);

                    var vc = viewControllerWeakReference.Unwrap();
                    if (vc != null)
                        Dialogs.ShowErrorAlert(vc, ex);

                    picker.DismissViewController(true, null);
                }
            }
        }

        class DocumentMenuDelegate : UIDocumentPickerDelegate, IUIDocumentPickerDelegate
        {
            readonly WeakReference<ComposeDocumentViewController> vcWeak;
            readonly Action<NSUrl> handler;

            public DocumentMenuDelegate(ComposeDocumentViewController vc, Action<NSUrl> handler)
            {
                vcWeak = vc.Wrap();
                this.handler = handler;
            }

            public override void DidPickDocument(UIDocumentPickerViewController controller, NSUrl url)
            {
                handler(url);
            }
        }

        #endregion

    }
}