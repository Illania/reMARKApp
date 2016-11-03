//
// Project: Mark5.Mobile.Droid
// File: ComposeDocumentFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
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
        public Document Document { get; set; }
        public DocumentPreview DocumentPreview { get; set; }
        public DocumentCreationModeFlag CreationModeFlag { get; set; }

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"{nameof(ComposeDocumentFragment)} [Document.Id={Document?.Id}, CreationModeFlag={CreationModeFlag}]");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            progress.Visibility = ViewStates.Gone;
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            scrollView.Visibility = ViewStates.Visible;
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            linearLayout.AddView(new ToView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new CcView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new BccView(Context));
            linearLayout.AddView(new Divider(Context));

            return rootView;
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
                //TODO to implement
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ComposeDocumentFragment)} [Document.Id={Document?.Id}, CreationModeFlag={CreationModeFlag}]";
        }

        class ComposeDocumentFragmentState : IRetainableState
        {
            public Document Document { get; set; }
            public DocumentPreview DocumentPreview { get; set; }
            public DocumentCreationModeFlag CreationModeFlag { get; set; }

        }

        #endregion
    }
}
