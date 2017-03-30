//
// Project: Mark5.Mobile.Droid
// File: SearchFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Animation;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.InputMethods;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.SearchViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class DocumentSearchCriteriaFragment : RetainableStateFragment, ISearchCriteriaFragment
    {
        SearchDocumentsCriteria searchCriteria;

        LinearLayoutCompat containerLinearLayout;
        FloatingActionButton fab;

        List<AbstractSearchView<SearchDocumentsCriteria>> subviews = new List<AbstractSearchView<SearchDocumentsCriteria>>();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(DocumentSearchCriteriaFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);

            var scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            scrollView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));

            containerLinearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            containerLinearLayout.SetBackgroundColor(Color.Transparent);
            containerLinearLayout.DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_horizontal);
            containerLinearLayout.ShowDividers = LinearLayoutCompat.ShowDividerMiddle;
            containerLinearLayout.Focusable = true;
            containerLinearLayout.FocusableInTouchMode = true;
            containerLinearLayout.RequestFocus(); //TODO
            containerLinearLayout.FocusChange += ContainerLinearLayout_FocusChange; //TODO

            var paddingLinearLayout = ConversionUtils.ConvertDpToPixels(12);
            var bottomPadding = ConversionUtils.ConvertDpToPixels(56) + (Resources.GetDimension(Resource.Dimension.fab_margin) + 2) * 2;
            containerLinearLayout.SetPadding(paddingLinearLayout, paddingLinearLayout, paddingLinearLayout, (int)bottomPadding);

            fab = ((View)container.Parent.Parent).FindViewById<FloatingActionButton>(Resource.Id.fab);

            var fabIcon = Resources.GetDrawable(Resource.Drawable.action_search_server, null).GetConstantState().NewDrawable().Mutate();
            fabIcon.SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)), PorterDuff.Mode.Multiply);

            fab.SetImageDrawable(fabIcon);
            fab.SetOnClickListener(new ActionOnClickListener(HandleSearchButtonClicked));
            fab.BackgroundTintList = ColorStateList.ValueOf(new Color(ContextCompat.GetColor(Context, Resource.Color.lightblue)));
            fab.RippleColor = new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)).ToArgb();
            fab.Visibility = ViewStates.Visible;

            var p = (CoordinatorLayout.LayoutParams)fab.LayoutParameters;
            p.Gravity = (int)(GravityFlags.Bottom | GravityFlags.CenterHorizontal);
            fab.LayoutParameters = p;

            var directionCriteria = new DocumentDirectionsSearchView(Context);
            subviews.Add(directionCriteria);

            var daterangeCriteria = new DocumentDateRangeSearchView(Context, this);
            subviews.Add(daterangeCriteria);

            var subjectMessageCriteria = new DocumentSubjectMessageSearchView(Context);
            subjectMessageCriteria.BackPressed += EditorText_BackPressed;
            subviews.Add(subjectMessageCriteria);

            var fromToCriteria = new DocumentFromToSearchView(Context);
            subviews.Add(fromToCriteria);

            var extraFieldsCriteria = new DocumentExtraFieldsSearchView(Context);
            subviews.Add(extraFieldsCriteria);

            var attUnreadCriteria = new DocumentAttachmentUnreadSearchView(Context);
            subviews.Add(attUnreadCriteria);

            var handledCriteria = new DocumentHandledSearchView(Context);
            subviews.Add(handledCriteria);

            containerLinearLayout.AddView(directionCriteria);
            containerLinearLayout.AddView(subjectMessageCriteria);
            containerLinearLayout.AddView(fromToCriteria);
            containerLinearLayout.AddView(daterangeCriteria);
            PrepareEditableTextRow();
            PrepareDropdownTextRow();
            if (ServerConfig.SystemSettings.DocumentsModuleInfo.ExtraFieldInfos.Any())
                containerLinearLayout.AddView(extraFieldsCriteria);
            containerLinearLayout.AddView(attUnreadCriteria);
            if (ServerConfig.SystemSettings.DocumentsModuleInfo.HandledFieldEnabled)
                containerLinearLayout.AddView(handledCriteria);

            HasOptionsMenu = true;

            return rootView;
        }

        void EditorText_BackPressed(object sender, EventArgs e)
        {
            //TODO to be completed
        }

        void ContainerLinearLayout_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            CommonConfig.Logger.Error($" --- STATUS ---- : {e.HasFocus} ");

            UpdateFabVisibility();
        }

        public void OnBackButtonPressed()
        {
            containerLinearLayout.RequestFocus();
            UpdateFabVisibility();
        }

        void UpdateFabVisibility()
        {
            if (containerLinearLayout.HasFocus)
            {
                fab.Visibility = ViewStates.Visible;
            }
            else
            {
                fab.Visibility = ViewStates.Gone;
            }
        }

        void HandleSearchButtonClicked()
        {
            CollectCriterias();
        }

        void CollectCriterias()
        {
            subviews.ForEach(v => v.UpdateCriteria());
        }

        public void PrepareEditableTextRow()
        {
            var ll = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };

            ll.DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_vertical);
            ll.ShowDividers = LinearLayoutCompat.ShowDividerMiddle;

            var lp = new LinearLayoutCompat.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1);

            var commentCriteria = new DocumentCommentsSearchView(Context, ll);
            subviews.Add(commentCriteria);

            var attachmentCriteria = new DocumentAttachmentSearchView(Context, ll);
            subviews.Add(attachmentCriteria);

            var referenceNumberCriteria = new DocumentReferenceNumberSearchView(Context, ll);
            subviews.Add(referenceNumberCriteria);

            ll.AddView(referenceNumberCriteria, lp);
            ll.AddView(commentCriteria, lp);
            ll.AddView(attachmentCriteria, lp);
            ll.LayoutTransition = new LayoutTransition();

            containerLinearLayout.AddView(ll);
        }

        public void PrepareDropdownTextRow()
        {
            var ll = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };

            ll.DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_vertical);
            ll.ShowDividers = LinearLayoutCompat.ShowDividerMiddle;

            var lp = new LinearLayoutCompat.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1);

            var categoriesCriteria = new DocumentCategoriesSearchView(Context, this);
            subviews.Add(categoriesCriteria);

            var linesCriteria = new DocumentLinesSearchView(Context, this);
            subviews.Add(linesCriteria);

            var priorityCriteria = new DocumentPrioritySearchView(Context, this);
            subviews.Add(priorityCriteria);

            ll.AddView(categoriesCriteria, lp);
            ll.AddView(linesCriteria, lp);
            ll.AddView(priorityCriteria, lp);

            containerLinearLayout.AddView(ll);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.search);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.documents);

            CommonConfig.Logger.Info($"Created {nameof(DocumentSearchCriteriaFragment)}");
        }

        public override void OnResume()
        {
            base.OnResume();

            searchCriteria = searchCriteria ?? new SearchDocumentsCriteria();
            RefreshViews();
        }

        void RefreshViews()
        {
            subviews.ForEach(c =>
            {
                c.Criteria = searchCriteria;
                c.Refresh();
            });
        }

        void Reset()
        {
            searchCriteria = new SearchDocumentsCriteria();
            containerLinearLayout.RequestFocus();
            ((InputMethodManager)Context.GetSystemService(Android.Content.Context.InputMethodService)).HideSoftInputFromWindow(containerLinearLayout.WindowToken, HideSoftInputFlags.None);
            RefreshViews();
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();
            var item = menu.Add(Menu.None, 10, 10, Resource.String.reset);
            item.SetShowAsAction(ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 10)
            {
                Reset();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }


        public void ReplaceFragment(Fragment f, string tag)
        {
            var fragmentManager = ((AppCompatActivity)Activity).SupportFragmentManager;

            fragmentManager.BeginTransaction()
                                       .SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right)
                                       .Replace(Resource.Id.fragment_container, f, tag)
                                       .AddToBackStack(tag)
                                       .Commit();
        }


        #region Retained State

        public override IRetainableState OnRetainInstanceState()
        {
            CollectCriterias();

            return new DocumentSearchCriteriaFragmentState
            {
                Criteria = searchCriteria,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var df = restoredState as DocumentSearchCriteriaFragmentState;
            if (df != null)
            {
                searchCriteria = df.Criteria;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(DocumentSearchCriteriaFragment)}";
        }

        class DocumentSearchCriteriaFragmentState : IRetainableState
        {
            public SearchDocumentsCriteria Criteria { get; set; }
        }

        #endregion
    }

    public interface ISearchCriteriaFragment
    {
        void ReplaceFragment(Fragment f, string tag);
    }

}