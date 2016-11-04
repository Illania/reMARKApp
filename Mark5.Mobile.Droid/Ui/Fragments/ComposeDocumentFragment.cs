//
// Project: Mark5.Mobile.Droid
// File: ComposeDocumentFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ComposeDocumentFragment : RetainableStateFragment
    {
        public Document PreviousDocument { get; set; }
        public DocumentPreview PreviousDocumentPreview { get; set; }
        public DocumentCreationModeFlag CreationModeFlag { get; set; }
        public int? PreviousDocumentFolderId { get; set; }

        public Document Document { get; set; } = new Document();
        public DocumentPreview DocumentPreview { get; set; } = new DocumentPreview();

        ToView toView;
        CcView ccView;
        BccView bccView;
        PriorityView priorityView;
        LineView lineView;
        SubjectView subjectView;
        ContentView contentView;

        List<ComposeDocumentView> subViews = new List<ComposeDocumentView>();

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"{nameof(ComposeDocumentFragment)} [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}]");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            progress.Visibility = ViewStates.Gone;
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            scrollView.Visibility = ViewStates.Visible;
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            toView = new ToView(Context);
            subViews.Add(toView);

            ccView = new CcView(Context);
            subViews.Add(ccView);

            bccView = new BccView(Context);
            subViews.Add(bccView);

            priorityView = new PriorityView(Context);
            subViews.Add(priorityView);

            lineView = new LineView(Context);
            subViews.Add(lineView);

            subjectView = new SubjectView(Context);
            subViews.Add(subjectView);

            contentView = new ContentView(Context);
            subViews.Add(contentView);

            foreach (var subview in subViews)
            {
                linearLayout.AddView(subview);
                linearLayout.AddView(new Divider(Context));
            }

            return rootView;
        }

        public override async void OnResume()
        {
            base.OnResume();

            await ShowDocument();
        }

        async Task ShowDocument()
        {
            if (CreationModeFlag == DocumentCreationModeFlag.New)
            {
                await AskIfShouldUseTemplates();
            }
        }

        async Task AskIfShouldUseTemplates()
        {
            if (CreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                CommonConfig.Logger.Info("Document opened in edit mode, no need to add template");
                return;
            }

            var useTemplate = PlatformConfig.Preferences.UseTemplate;
            if (useTemplate == Utilities.Preferences.TemplateUsageMode.DontUse)
            {
                return;
            }

            if (useTemplate == Utilities.Preferences.TemplateUsageMode.Local)
            {
                var localTemplate = PlatformConfig.Preferences.LocalTemplate;
                contentView.InsertTemplate(localTemplate, ContentType.PlainText);
            }
            else if (useTemplate == Utilities.Preferences.TemplateUsageMode.Default)
            {

            }
            else if (useTemplate == Utilities.Preferences.TemplateUsageMode.AlwaysAsk)
            {

            }
        }

        #region Retained State methods

        public override IRetainableState OnRetainInstanceState()
        {
            //CommonConfig.Logger.Info($"Retaining state [entity.Id={Entity?.Id}, addCommentText={addCommentEditText?.Text}");

            //return new CommentsFragmentState
            //{
            //    Entity = Entity,
            //    AddCommentText = addCommentEditText.Text
            //};

            //TODO to implement
            return new ComposeDocumentFragmentState();
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var cfs = restoredState as ComposeDocumentFragmentState;
            if (cfs != null)
            {
                Document = cfs.Document;
                DocumentPreview = cfs.DocumentPreview;
                PreviousDocument = cfs.PreviousDocument;
                PreviousDocumentPreview = cfs.PreviousDocumentPreview;
                CreationModeFlag = cfs.CreationModeFlag;
                PreviousDocumentFolderId = cfs.PreviousDocumentFolderId;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ComposeDocumentFragment)} [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}]";
        }

        class ComposeDocumentFragmentState : IRetainableState
        {
            public Document Document { get; set; }
            public DocumentPreview DocumentPreview { get; set; }
            public Document PreviousDocument { get; set; }
            public DocumentPreview PreviousDocumentPreview { get; set; }
            public int? PreviousDocumentFolderId { get; set; }
            public DocumentCreationModeFlag CreationModeFlag { get; set; }

        }

        #endregion
    }
}
