//
// Project: Mark5.Mobile.IOS
// File: ComposeDocumentViewController.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using Mark5.Mobile.Common;
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
        
        bool showedDocumentOnAppear;

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

            if (!showedDocumentOnAppear)
            {
                showedDocumentOnAppear = true;
                ShowDocument();
            }
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
            StackView.Alpha = 0.0f;

        }

        #endregion


    }
}
