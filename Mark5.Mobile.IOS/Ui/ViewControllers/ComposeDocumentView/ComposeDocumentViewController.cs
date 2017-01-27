//
// Project: Mark5.Mobile.IOS
// File: ComposeDocumentViewController.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.Common.StackView;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class ComposeDocumentViewController : StackViewController
    {
        public DocumentDirection PreviousDocumentDirection { get; set; }
        public DocumentCreationModeFlag CreationModeFlag { get; set; }
        public DocumentCreationModeFlag OutgoingDocumentOriginalCreationModeFlag { get; set; }
        public Guid OutgoingDocumentGuid { get; set; }
        public OutgoingDocumentState OutgoingDocumentState { get; set; }
        public List<OutgoingDocumentAttachmentDescription> OutgoingDocumentInitialAttachments { get; set; } = new List<OutgoingDocumentAttachmentDescription>();
        public bool LocalDocument { get; set; }
        public int? PreviousDocumentFolderId { get; set; }
        public int? PreviousDocumentId { get; set; }
        public string[] PreconfiguredEmailAddresses { get; set; }

        public Document PreviousDocument { get; set; }
        public DocumentPreview PreviousDocumentPreview { get; set; }

        Document Document { get; set; } = new Document();
        DocumentPreview DocumentPreview { get; set; } = new DocumentPreview();

        ToView toView;
        CcView ccView;
        BccView bccView;
        LineView lineView;
        PriorityView priorityView;
        SubjectView subjectView;
        ContentView contentView;
        readonly List<ComposeDocumentView> subViews = new List<ComposeDocumentView>();

        bool templateLoaded;

        UIBarButtonItem cancelButtonItem;
        UIBarButtonItem sendButtonItem;

        // This value will be later updated from notification.
        float keyboardHeight = 216.0f;

        public ComposeDocumentViewController()
        {
            Title = Localization.GetString("new_document");
        }

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitNavigationBar();
            InitSubViews();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();

            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, OnKeyboardDidShowNotification);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillChangeFrameNotification, OnKeyboardDidShowNotification);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardWillHideNotification);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public override async void ViewDidAppear(bool animated)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{typeof(ComposeDocumentViewController)} appeared");

            if (OutgoingDocumentGuid == Guid.Empty)
            {
                OutgoingDocumentGuid = Guid.NewGuid();
            }

            PreviousDocumentDirection = DocumentDirection.None;
            CreationModeFlag = DocumentCreationModeFlag.New;

            await LoadDocument();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            DeInitializeHandlers();

            NSNotificationCenter.DefaultCenter.RemoveObservers(new[]
            {
                    UIKeyboard.DidShowNotification,
                    UIKeyboard.WillChangeFrameNotification,
                    UIKeyboard.WillHideNotification
                });
        }

        #endregion

        #region Init methods

        void InitNavigationBar()
        {
            cancelButtonItem = new UIBarButtonItem();
            cancelButtonItem.Title = Localization.GetString("cancel");
            NavigationItem.SetLeftBarButtonItem(cancelButtonItem, false);

            sendButtonItem = new UIBarButtonItem();
            sendButtonItem.Title = Localization.GetString("send");
            sendButtonItem.Enabled = false;
            NavigationItem.SetRightBarButtonItem(sendButtonItem, false);
        }

        void InitSubViews()
        {
            toView = new ToView();
            subViews.Add(toView);

            ccView = new CcView();
            subViews.Add(ccView);

            bccView = new BccView();
            subViews.Add(bccView);

            lineView = new LineView(this);
            subViews.Add(lineView);

            priorityView = new PriorityView(this);
            if (PlatformConfig.Preferences.ComposePriorityEnabled)
                subViews.Add(priorityView);

            subjectView = new SubjectView();
            subViews.Add(subjectView);

            contentView = new ContentView();
            subViews.Add(contentView);

            AddArrangedViewsWithSeparators(subViews);
        }

        void InitializeHandlers()
        {
            cancelButtonItem.Clicked += CancelButtonItem_Clicked;
            sendButtonItem.Clicked += SendButtonItem_Clicked;
        }

        void DeInitializeHandlers()
        {
            cancelButtonItem.Clicked -= CancelButtonItem_Clicked;
            sendButtonItem.Clicked -= SendButtonItem_Clicked;
        }

        #endregion

        async Task LoadDocument()
        {
            if (PreviousDocument != null || CreationModeFlag == DocumentCreationModeFlag.New)
            {
                await ShowDocument();
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_document___"));

            try
            {
                if (LocalDocument)
                {
                    var outgoingContainer = await Managers.DocumentsManager.GetOutgoingDocumentContainerAsync(OutgoingDocumentGuid, true);
                    PreviousDocument = outgoingContainer.Document;
                    PreviousDocumentPreview = outgoingContainer.DocumentPreview;
                    PreviousDocumentId = outgoingContainer.Info.PreviousDocumentId;
                    PreviousDocumentFolderId = outgoingContainer.Info.PreviousDocumentdFolderId;
                    OutgoingDocumentState = outgoingContainer.Info.State;
                    OutgoingDocumentOriginalCreationModeFlag = outgoingContainer.Info.Flag;
                    if (outgoingContainer.Info.State == OutgoingDocumentState.Failed)
                    {
                        await Dialogs.ShowErrorDialogAsync(this, new Exception(Localization.GetString("error_while_sending_document")));
                    }
                    if (outgoingContainer.LocalAttachments != null)
                    {
                        OutgoingDocumentInitialAttachments.AddRange(outgoingContainer.LocalAttachments);
                    }
                }
                else
                {
                    var sourceType = SourceType.Auto;
                    PreviousDocument = await Managers.DocumentsManager.GetDocumentAsync(PreviousDocumentFolderId.Value, PreviousDocumentId.Value, sourceType);
                    if (CreationModeFlag == DocumentCreationModeFlag.Edit && PreviousDocumentPreview.Direction == DocumentDirection.Draft)
                    {
                        Document.Id = DocumentPreview.Id = PreviousDocument.Id;
                    }
                }

                dismissAction();
                await ShowDocument();
            }
            catch (Exception ex)
            {
                dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        async Task ShowDocument()
        {
            foreach (var subView in subViews)
            {
                subView.Document = Document;
                subView.DocumentPreview = DocumentPreview;
                subView.PreviousDocument = PreviousDocument;
                subView.PreviousDocumentPreview = PreviousDocumentPreview;
                subView.CreationModeFlag = CreationModeFlag;
                await subView.RefreshView();
            }

            if (CreationModeFlag == DocumentCreationModeFlag.New && PreconfiguredEmailAddresses != null)
            {
                toView.SetEmails(PreconfiguredEmailAddresses);
            }

            await AskIfShouldUseTemplates();
        }

        #region Navigation Bar items related

        bool IsFormValid()
        {
            if (subjectView.Empty)
            {
                return false;
            }

            var recipientAdded = false;
            foreach (var recipientView in new List<RecipientsView> { toView, ccView, bccView })
            {
                recipientAdded |= !recipientView.Empty;
            }

            if (!recipientAdded)
            {
                return false;
            }

            return !lineView.LineSelectedIsAmbiguous;
        }

        #endregion

        #region Keyboard Notifications

        void OnKeyboardDidShowNotification(NSNotification notification)
        {
            keyboardHeight = UI.KeyboardHeightFromNotification(notification);

            var insets = ScrollView.ContentInset;
            insets.Bottom = keyboardHeight;
            ScrollView.ContentInset = insets;
        }

        void OnKeyboardWillHideNotification(NSNotification notification)
        {
            var insets = ScrollView.ContentInset;
            insets.Bottom = 0.0f;
            ScrollView.ContentInset = insets;
        }

        #endregion

        #region Actions

        void SendButtonItem_Clicked(object sender, EventArgs e)
        {
            //TODO
        }

        void CancelButtonItem_Clicked(object sender, EventArgs e)
        {
            //TODO complete
            PopOrDismissViewController();
        }

        void PopOrDismissViewController()
        {
            if (PresentingViewController != null)
            {
                DismissViewController(true, null);
            }
            else
            {
                NavigationController.PopViewController(true);
            }
        }

        #endregion

        #region Template methods

        async Task AskIfShouldUseTemplates()
        {
            if (templateLoaded)
            {
                return;
            }

            if (CreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                CommonConfig.Logger.Info("Document opened in edit mode, no need to add template");
                return;
            }

            var useTemplate = PlatformConfig.Preferences.UseTemplate;

            if (useTemplate == Preferences.TemplateUsageMode.DontUse)
            {
                return;
            }

            if (useTemplate == Preferences.TemplateUsageMode.Local)
            {
                await GetLocalTemplate();
            }
            else if (useTemplate == Preferences.TemplateUsageMode.Default)
            {
                await GetDefaultTemplate();
            }
            else if (useTemplate == Preferences.TemplateUsageMode.AlwaysAsk) //TODO popover delegate not set
            {
                var templateListStrings = new string[] { Localization.GetString("template_selection_default"),
                    Localization.GetString("template_selection_local"),
                    Localization.GetString("template_selection_another") };
                var result = await Dialogs.ShowListDialogAsync(this, Localization.GetString("template_selection_title"), templateListStrings, contentView);
                switch (result)
                {
                    case -1:
                        break;
                    case 0:
                        await GetDefaultTemplate(true);
                        break;
                    case 1:
                        await GetLocalTemplate();
                        break;
                    case 2:
                        await GetAllTemplates();
                        break;
                }
            }

            templateLoaded = true;
        }

        async Task GetAllTemplates()
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_templates___"));
            List<TemplatePreview> templatesPreviews = null;

            try
            {
                templatesPreviews = await Managers.DocumentsManager.GetTemplatePreviewsAsync();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting default template [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }

            if (templatesPreviews != null)
            {
                var templatesForCreationMode = templatesPreviews.Where(t => t.CreationMode.HasFlag(CreationModeFlag));
                if (templatesForCreationMode.Any())
                {
                    var templateNames = templatesForCreationMode.Select(t => (t.Private ? "[Private] " : "[Public] ") + t.Name).ToArray();

                    var result = await Dialogs.ShowListDialogAsync(this, Localization.GetString("template_question"), templateNames, contentView);
                    var selectedPreview = templatesPreviews[result];
                    await GetTemplate(selectedPreview);
                }
                else
                {
                    await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("no_templates_title"), Localization.GetString("no_templates_content"));
                }
            }
        }

        async Task GetLocalTemplate()
        {
            var localTemplate = PlatformConfig.Preferences.LocalTemplate;
            await contentView.InsertLocalTemplate(localTemplate);
        }

        async Task GetDefaultTemplate(bool errorMessageIfNull = false)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_template___"));

            try
            {
                var template = await Managers.DocumentsManager.GetDefaultTemplateAsync(CreationModeFlag);
                if (template != null)
                {
                    await ApplyTemplate(template);
                }
                else if (errorMessageIfNull)
                {
                    throw new Exception(Localization.GetString("template_null"));
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting default template [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        async Task GetTemplate(TemplatePreview templatePreview)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_template___"));

            try
            {
                var template = await Managers.DocumentsManager.GetTemplateAsync(templatePreview.Id);
                if (template != null)
                {
                    await ApplyTemplate(template);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting template [template.Id={templatePreview?.Id}, PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        async Task ApplyTemplate(Template template)
        {
            ProcessTemplate(template);

            await contentView.InsertTemplate(template);

            if (!string.IsNullOrEmpty(template.Subject))
            {
                subjectView.SetSubject(template.Subject);
            }

            lineView.SetLineFromGuid(template.LineGuid);
        }

        void ProcessTemplate(Template template)
        {
            var templateContent = template.Content;

            var currentTime = DateTime.Now;
            var dateString = currentTime.ToString("dd-MM-yyyy");
            var timeString = currentTime.ToString("HH:mm");

            string fromNameString = string.Empty;
            if (PreviousDocumentPreview != null && PreviousDocumentPreview.Addresses != null)
            {
                fromNameString = PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.From).Select(da => da.Name).FirstOrDefault() ?? string.Empty;
            }

            if (template.ContentType == ContentType.Html)
            {
                templateContent = templateContent.Replace("&lt;FROMNAME&gt;", fromNameString);
                templateContent = templateContent.Replace("&lt;DATE&gt;", dateString);
                templateContent = templateContent.Replace("&lt;TIME&gt;", timeString);
            }
            else
            {
                templateContent = templateContent.Replace("<FROMNAME>", fromNameString);
                templateContent = templateContent.Replace("<DATE>", dateString);
                templateContent = templateContent.Replace("<TIME>", timeString);
            }

            if (template.ContentType == ContentType.Html)
            {
                templateContent = templateContent.Replace("&lt;CUR&gt;", string.Empty);
                templateContent = templateContent.Replace("&lt;SOURCETEXT&gt;", string.Empty);
                templateContent = templateContent.Replace("&lt;COMPANYNAME&gt;", string.Empty);
                templateContent = templateContent.Replace("&lt;FROMNAMEWITHCOMPANY&gt;", string.Empty);
            }
            else
            {
                templateContent = templateContent.Replace("<CUR>", string.Empty);
                templateContent = templateContent.Replace("<SOURCETEXT>", string.Empty);
                templateContent = templateContent.Replace("<COMPANYNAME>", string.Empty);
                templateContent = templateContent.Replace("<FROMNAMEWITHCOMPANY>", string.Empty);
            }

            template.Content = templateContent;
        }

        #endregion

    }
}
