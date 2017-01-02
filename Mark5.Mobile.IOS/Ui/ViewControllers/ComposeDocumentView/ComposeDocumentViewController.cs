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

        Document PreviousDocument { get; set; }
        DocumentPreview PreviousDocumentPreview { get; set; }

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

            //TODO subscription to keyboard notifications
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{typeof(ComposeDocumentViewController)} appeared");

            if (OutgoingDocumentGuid == Guid.Empty)
            {
                OutgoingDocumentGuid = Guid.NewGuid();
            }

            PreviousDocumentDirection = DocumentDirection.None;
            CreationModeFlag = DocumentCreationModeFlag.New;

            LoadDocument();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
        }

        #endregion

        #region Init methods

        void InitNavigationBar()
        {
            cancelButtonItem = new UIBarButtonItem();
            cancelButtonItem.Title = "Cancel";
            //cancelButtonItem.Clicked += DoCancel; //TODO
            NavigationItem.SetLeftBarButtonItem(cancelButtonItem, false);

            sendButtonItem = new UIBarButtonItem();
            sendButtonItem.Title = "Send";
            //sendButtonItem.Clicked += DoSend; //TODO
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

        #endregion

        async Task LoadDocument()
        {
            if (PreviousDocument != null || CreationModeFlag == DocumentCreationModeFlag.New)
            {
                await ShowDocument();
                return;
            }

            //TODO
            //var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.loading_document, Resource.String.please_wait);

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
                        //TODO
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
                    //TODO eventually this could be improved by first checking the cache
                    var container = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(PreviousDocumentFolderId.Value, PreviousDocumentId.Value, sourceType); // TODO
                    PreviousDocument = container.Document;
                    PreviousDocumentPreview = container.DocumentPreview;
                    if (CreationModeFlag == DocumentCreationModeFlag.Edit && PreviousDocumentPreview.Direction == DocumentDirection.Draft)
                    {
                        Document.Id = DocumentPreview.Id = PreviousDocument.Id;
                    }
                }

                await ShowDocument();
            }
            catch (Exception ex)
            {
                //TODO
                //dismissAction();

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

            //await AskIfShouldUseTemplates(); //TODO
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
            else if (useTemplate == Preferences.TemplateUsageMode.AlwaysAsk)
            {
                //var result = await Dialogs.ShowListDialog(Context, Resource.String.template_question, Resource.Array.template_question_options, true); //TODO
                int result = 0;
                switch (result)
                {
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
            //var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_templates, Resource.String.please_wait); //TODO
            List<TemplatePreview> templatesPreviews = null;

            try
            {
                templatesPreviews = await Managers.DocumentsManager.GetTemplatePreviewsAsync();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting default template [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", ex);

                //dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                //dismissAction();
            }

            if (templatesPreviews != null)
            {
                var templatesForCreationMode = templatesPreviews.Where(t => t.CreationMode.HasFlag(CreationModeFlag));
                if (templatesForCreationMode.Any())
                {
                    var templateNames = templatesForCreationMode.Select(t => (t.Private ? "[Private] " : "[Public] ") + t.Name).ToArray();
                    //var result = await Dialogs.ShowListDialog(Context, Resource.String.template_question, templateNames, true); //TODO 
                    int result = 0;
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
            //var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_template, Resource.String.please_wait);

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

                //dismissAction(); //TODO
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                //dismissAction(); //TODO
            }
        }

        async Task GetTemplate(TemplatePreview templatePreview)
        {
            //var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_template, Resource.String.please_wait); TODO

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

                //dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                //dismissAction();
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
