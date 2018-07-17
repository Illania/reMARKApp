using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ObjectLinksFragment : BaseFragment
    {
        const string BusinessEntityBundleKey = "BusinessEntity_0dd3cb9b-f178-4b02-b7d3-e1bb3428c913";
        const string ObjectLinksKey = "ObjectLinks_0baf33c2-242b-44e6-ba0f-5f82fb1dc0e1";

        IBusinessEntity businessEntity;

        List<ObjectLink> objectLinks;

        ProgressBar progress;
        NestedScrollView scrollView;
        LinearLayoutCompat linearLayout;

        public static (ObjectLinksFragment fragment, string tag) NewInstance(IBusinessEntity businessEntity)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenLinksEvent(businessEntity.ModuleType));

            var args = new Bundle();

            if (businessEntity != null)
                args.PutString(BusinessEntityBundleKey, Serializer.Serialize(businessEntity));

            var fragment = new ObjectLinksFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(ObjectLinksFragment)} [businessEntity.id={businessEntity.Id}, businessEntity?.objectType={businessEntity.ObjectType}]";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(BusinessEntityBundleKey))
                businessEntity = Serializer.Deserialize<IBusinessEntity>(Arguments.GetString(BusinessEntityBundleKey));

            if (savedInstanceState?.ContainsKey(ObjectLinksKey) == true)
                objectLinks = Serializer.Deserialize<List<ObjectLink>>(savedInstanceState.GetString(ObjectLinksKey));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ObjectLinksFragment)} [businessEntity.id={businessEntity?.Id}, businessEntity.objectType={businessEntity?.ObjectType}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);
            rootView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            var padding = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 10f, Resources.DisplayMetrics) + 0.5f);
            linearLayout.SetPadding(padding, padding, padding, padding);

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.links);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(ObjectLinksFragment)} [businessEntity.id={businessEntity?.Id}, businessEntity.objectType={businessEntity?.ObjectType}]...");
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (objectLinks != null)
                outState.PutString(ObjectLinksKey, Serializer.Serialize(objectLinks));
        }

        async Task RefreshData()
        {
            try
            {
                if (objectLinks == null)
                {
                    objectLinks = await Managers.CommonActionsManager.GetObjectLinksAsync(businessEntity);
                    ProcessObjectLinks(objectLinks);
                }

                RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading object actions failed [businessEntity.id={businessEntity.Id}, businessEntity.objectType={businessEntity.ObjectType}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        void RefreshView()
        {
            progress.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            linearLayout.RemoveViews(0, linearLayout.ChildCount);

            var grouppedObjectLinks = objectLinks.OrderBy(ol => ol.TypeInfo.DescriptionSimple).GroupBy(ol => ol.IsReverse ? ol.TypeInfo.DescriptionComplexReverse : ol.TypeInfo.DescriptionComplex);

            foreach (var grouppedObjectLink in grouppedObjectLinks)
            {
                var olv = new ObjectLinksView(Context, grouppedObjectLink.Key, grouppedObjectLink.ToArray());
                olv.ObjectLinkClicked += ObjectLinksView;
                linearLayout.AddView(olv);
            }

            linearLayout.Invalidate();
            linearLayout.RequestLayout();
        }

        void ProcessObjectLinks(List<ObjectLink> ols)
        {
            foreach (var ol in ols)
            {
                ol.TypeInfo.DescriptionAction = ProcessString(ol.TypeInfo.DescriptionAction, ol.FromObjectType, ol.ToObjectType);
                ol.TypeInfo.DescriptionActionReverse = ProcessString(ol.TypeInfo.DescriptionActionReverse, ol.FromObjectType, ol.ToObjectType);
                ol.TypeInfo.DescriptionComplex = ProcessString(ol.TypeInfo.DescriptionComplex, ol.FromObjectType, ol.ToObjectType);
                ol.TypeInfo.DescriptionComplexReverse = ProcessString(ol.TypeInfo.DescriptionComplexReverse, ol.FromObjectType, ol.ToObjectType);
                ol.TypeInfo.DescriptionSimple = ProcessString(ol.TypeInfo.DescriptionSimple, ol.FromObjectType, ol.ToObjectType);
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

        void ObjectLinksView(object sender, ObjectLink ol)
        {
            if (ol.IsReverse)
            {
                switch (ol.FromObjectType)
                {
                    case ObjectType.Document:
                        StartActivity(DocumentActivity.CreateIntent(Context, documentId: ol.FromObjectId));
                        break;
                    case ObjectType.Contact:
                        StartActivity(ContactActivity.CreateIntent(Context, contactId: ol.FromObjectId));
                        break;
                    case ObjectType.Shortcode:
                        StartActivity(ShortcodeActivity.CreateIntent(Context, shortcodeId: ol.FromObjectId));
                        break;
                }
            }
            else
            {
                switch (ol.ToObjectType)
                {
                    case ObjectType.Document:
                        StartActivity(DocumentActivity.CreateIntent(Context, documentId: ol.ToObjectId));
                        break;
                    case ObjectType.Contact:
                        StartActivity(ContactActivity.CreateIntent(Context, contactId: ol.ToObjectId));
                        break;
                    case ObjectType.Shortcode:
                        StartActivity(ShortcodeActivity.CreateIntent(Context, shortcodeId: ol.ToObjectId));
                        break;
                }
            }
        }
    }
}