//
// Project: 
// File: CategoriesListFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid
{
    public class CategoriesListFragment : RetainableStateFragment
    {
        public BusinessEntityPreview BusinessEntityPreview
        {
            get;
            set;
        }

        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(CategoriesListFragment)} [businessEntity.id={BusinessEntityPreview.Id}, businessEntity.objectType={BusinessEntityPreview.ObjectType}]");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout, container, false);

            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.categories);

            CommonConfig.Logger.Info($"Created {nameof(CategoriesListFragment)} [businessEntity.id={BusinessEntityPreview.Id}, businessEntity.objectType={BusinessEntityPreview.ObjectType}]");
        }

        #region Retained State

        public override IRetainableState OnRetainInstanceState()
        {
            return new CategoriesListFragmentState
            {
                BusinessEntityPreview = BusinessEntityPreview,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var clfs = restoredState as CategoriesListFragmentState;
            if (clfs != null)
            {
                BusinessEntityPreview = clfs.BusinessEntityPreview;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(CategoriesListFragment)} [businessEntity.id={BusinessEntityPreview.Id}, businessEntity.objectType={BusinessEntityPreview.ObjectType}]";
        }

        class CategoriesListFragmentState : IRetainableState
        {
            public BusinessEntityPreview BusinessEntityPreview { get; set; }
        }

        #endregion

    }
}
