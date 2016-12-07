//
// Project: Mark5.Mobile.Droid
// File: ObjectLinksFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views;
using System.Linq;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class ObjectLinksFragment : RetainableStateFragment
    {

        public IBusinessEntity BusinessEntity
        {
            get;
            set;
        }

        public Action CloseRequest { get; set; }

        List<ObjectLink> objectLinks;

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ObjectLinksFragment)} [businessEntity.id={BusinessEntity?.Id}, businessEntity.objectType={BusinessEntity?.ObjectType}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);
            rootView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightergray)));

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            var padding = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 10.0f, Resources.DisplayMetrics) + 0.5f);
            linearLayout.SetPadding(padding, padding, padding, padding);

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.links);

            CommonConfig.Logger.Info($"Created {nameof(ObjectLinksFragment)} [businessEntity.id={BusinessEntity?.Id}, businessEntity.objectType={BusinessEntity?.ObjectType}]...");
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();
        }

        async Task RefreshData()
        {
            try
            {
                if (objectLinks == null)
                {
                    objectLinks = await Managers.CommonActionsManager.GetObjectLinksAsync(BusinessEntity);
                    ProcessObjectLinks(objectLinks);
                }

                RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading object actions failed [businessEntity.id={BusinessEntity.Id}, businessEntity.objectType={BusinessEntity.ObjectType}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                if (CloseRequest != null) CloseRequest();
            }
        }

        void RefreshView()
        {
            progress.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            linearLayout.RemoveViews(0, linearLayout.ChildCount);

            var grouppedObjectLinks = objectLinks.OrderBy(ol => ol.TypeInfo.DescriptionSimple).GroupBy(ol => ol.TypeInfo.DescriptionComplex);

            foreach (var grouppedObjectLink in grouppedObjectLinks)
            {
                linearLayout.AddView(new ObjectLinksView(Context, grouppedObjectLink.Key, grouppedObjectLink.ToArray()));
            }

            linearLayout.Invalidate();
            linearLayout.RequestLayout();
        }

        void ProcessObjectLinks(List<ObjectLink> ols)
        {
            foreach (var ol in ols)
            {
                ol.TypeInfo.DescriptionAction = ProcessString(ol.TypeInfo.DescriptionAction, ol.TypeInfo.FromType, ol.TypeInfo.ToType);
                ol.TypeInfo.DescriptionActionReverse = ProcessString(ol.TypeInfo.DescriptionActionReverse, ol.TypeInfo.FromType, ol.TypeInfo.ToType);
                ol.TypeInfo.DescriptionComplex = ProcessString(ol.TypeInfo.DescriptionComplex, ol.TypeInfo.FromType, ol.TypeInfo.ToType);
                ol.TypeInfo.DescriptionComplexReverse = ProcessString(ol.TypeInfo.DescriptionComplexReverse, ol.TypeInfo.FromType, ol.TypeInfo.ToType);
                ol.TypeInfo.DescriptionSimple = ProcessString(ol.TypeInfo.DescriptionSimple, ol.TypeInfo.FromType, ol.TypeInfo.ToType);
            }
        }

        string ProcessString(string str, ObjectType from, ObjectType to)
        {
            if (str.Contains("%"))
            {
                str = str.Replace("%ObjFromName%", from.ToString());
                str = str.Replace("%ObjToName%", to.ToString());
            }

            return str;
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new ObjectLinksFragmentState
            {
                BusinessEntity = BusinessEntity,
                ObjectLinks = objectLinks
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var oafs = restoredState as ObjectLinksFragmentState;
            if (oafs != null)
            {
                BusinessEntity = oafs.BusinessEntity;
                objectLinks = oafs.ObjectLinks;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ObjectLinksFragment)} [businessEntity.id={BusinessEntity?.Id}, businessEntity?.objectType={BusinessEntity.ObjectType}]";
        }

        class ObjectLinksFragmentState : IRetainableState
        {

            public IBusinessEntity BusinessEntity { get; set; }

            public List<ObjectLink> ObjectLinks { get; set; }
        }
    }
}
