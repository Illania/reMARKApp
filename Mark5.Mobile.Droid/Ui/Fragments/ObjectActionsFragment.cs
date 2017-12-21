using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ObjectActionsFragment : BaseFragment
    {
        const string BusinessEntityBundleKey = "BusinessEntity_4330331f-58a2-458e-839a-48ace9b11c38";
        const string ObjectActionsKey = "ObjectActions_e5d2d57d-7d56-4f9c-80f1-71ac70c86a01";

        IBusinessEntity businessEntity;

        List<ObjectAction> objectActions;

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        public static (ObjectActionsFragment fragment, string tag) NewInstance(IBusinessEntity businessEntity)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenActionsEvent(businessEntity.ModuleType));

            var args = new Bundle();

            if (businessEntity != null)
                args.PutString(BusinessEntityBundleKey, Serializer.Serialize(businessEntity));

            var fragment = new ObjectActionsFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(ObjectActionsFragment)} [businessEntity.id={businessEntity.Id}, businessEntity.objectType={businessEntity.ObjectType}]";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(BusinessEntityBundleKey))
                businessEntity = Serializer.Deserialize<IBusinessEntity>(Arguments.GetString(BusinessEntityBundleKey));

            if (savedInstanceState?.ContainsKey(ObjectActionsKey) == true)
                objectActions = Serializer.Deserialize<List<ObjectAction>>(savedInstanceState.GetString(ObjectActionsKey));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ObjectActionsFragment)} [businessEntity.id={businessEntity?.Id}, businessEntity.objectType={businessEntity?.ObjectType}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);
            rootView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            var padding = Conversion.ConvertDpToPixels(10f);
            linearLayout.SetPadding(padding, padding, padding, padding);

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.actions);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(ObjectActionsFragment)} [businessEntity.id={businessEntity?.Id}, businessEntity.objectType={businessEntity?.ObjectType}]...");
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (objectActions != null)
                outState.PutString(ObjectActionsKey, Serializer.Serialize(objectActions));
        }

        async Task RefreshData()
        {
            try
            {
                if (objectActions == null)
                    objectActions = await Managers.CommonActionsManager.GetObjectActionsAsync(businessEntity);

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

            var grouppedObjectActions = objectActions.OrderBy(oa => oa.ActionType).ThenBy(oa => oa.ActionTimeTimestamp).GroupBy(oa => oa.ActionType);

            foreach (var grouppedObjectAction in grouppedObjectActions)
                linearLayout.AddView(new ObjectActionsView(Context, grouppedObjectAction.Key, grouppedObjectAction.ToArray()));

            linearLayout.Invalidate();
            linearLayout.RequestLayout();
        }
    }
}