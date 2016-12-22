//
// Project: Mark5.Mobile.IOS
// File: ComposeDocumentViewController.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.Common.StackView;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews;
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
        SubjectsView subjectView;
        ContentView contentView;
        readonly List<ComposeDocumentView> subViews = new List<ComposeDocumentView>();

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

            subjectView = new SubjectsView();
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
                        //await Dialogs.ShowErrorDialogAsync(Activity, new Exception(Resources.GetString(Resource.String.error_while_sending_document)));
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

                //await Dialogs.ShowErrorDialogAsync(Activity, ex);

                //if (CloseRequest != null) CloseRequest();
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

    }
}
