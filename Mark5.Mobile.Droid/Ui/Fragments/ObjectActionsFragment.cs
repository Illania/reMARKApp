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
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ObjectActionsFragment : RetainableStateFragment
    {
        public IBusinessEntity BusinessEntity { get; set; }

        public Action CloseRequest { get; set; }

        List<ObjectAction> objectActions;

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ObjectActionsFragment)} [businessEntity.id={BusinessEntity?.Id}, businessEntity.objectType={BusinessEntity?.ObjectType}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);
            rootView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            var padding = ConversionUtils.ConvertDpToPixels(10f);
            linearLayout.SetPadding(padding, padding, padding, padding);

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity) Activity).SupportActionBar.Title = GetString(Resource.String.actions);
            ((AppCompatActivity) Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(ObjectActionsFragment)} [businessEntity.id={BusinessEntity?.Id}, businessEntity.objectType={BusinessEntity?.ObjectType}]...");
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
                if (objectActions == null)
                    objectActions = await Managers.CommonActionsManager.GetObjectActionsAsync(BusinessEntity);

                RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading object actions failed [businessEntity.id={BusinessEntity.Id}, businessEntity.objectType={BusinessEntity.ObjectType}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                if (CloseRequest != null)
                    CloseRequest();
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

        public override IRetainableState OnRetainInstanceState()
        {
            return new ObjectActionsFragmentState
            {
                BusinessEntity = BusinessEntity,
                ObjectActions = objectActions
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var oafs = restoredState as ObjectActionsFragmentState;
            if (oafs != null)
            {
                BusinessEntity = oafs.BusinessEntity;
                objectActions = oafs.ObjectActions;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ObjectActionsFragment)} [businessEntity.id={BusinessEntity.Id}, businessEntity.objectType={BusinessEntity.ObjectType}]";
        }

        class ObjectActionsFragmentState : IRetainableState
        {
            public IBusinessEntity BusinessEntity { get; set; }

            public List<ObjectAction> ObjectActions { get; set; }
        }
    }
}