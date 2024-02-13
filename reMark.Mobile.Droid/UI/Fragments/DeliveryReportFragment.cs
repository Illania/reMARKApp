using System;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Ui.Common;
using reMark.Mobile.Droid.Ui.Views.Common;
using reMark.Mobile.Droid.Utilities;
using Color = Android.Graphics.Color;
using View = Android.Views.View;

namespace reMark.Mobile.Droid.Ui.Fragments
{
    public class DeliveryReportFragment : BaseFragment
    {
        const string TransmitDestinationBundleKey = "TransmitDestinationId_da4826eb-eb7a-4ceb-bd12-9c735bef1555";
        const string ReferenceNumberBundleKey = "ReferenceNumber_40876832-91a3-46d7-a57e-6d850847c295";

        private string referenceNumber;
        private TransmitDestination transmitDestination;

        RelativeLayout relativeLayout;
        LinearLayoutCompat linearLayout;
        View container;

        Action dismissAction;

        public static (DeliveryReportFragment fragment, string tag) NewInstance(
            TransmitDestination destination = null, string referenceNumber = "")
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenContactEvent());

            var args = new Bundle();

            if (!string.IsNullOrEmpty(referenceNumber))
                args.PutString(ReferenceNumberBundleKey, referenceNumber);

            if (destination != null)
                args.PutString(TransmitDestinationBundleKey, Serializer.Serialize(destination));


            var fragment = new DeliveryReportFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(DeliveryReportFragment)} [reference={referenceNumber}]";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(TransmitDestinationBundleKey))
                transmitDestination = Serializer.Deserialize<TransmitDestination>(Arguments.GetString(TransmitDestinationBundleKey));

            if (Arguments.ContainsKey(ReferenceNumberBundleKey))
                referenceNumber = Arguments.GetString(ReferenceNumberBundleKey);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            CommonConfig.Logger.Info($"Creating {nameof(DeliveryReportFragment)} [reference={referenceNumber}]");

            this.container = container;

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);
            rootView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));

            relativeLayout = rootView.FindViewById<RelativeLayout>(Resource.Id.relative_layout);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            var paddingLinearLayout = Conversion.ConvertDpToPixels(10);

            //linearLayout.SetPadding(paddingLinearLayout, paddingLinearLayout * 3, paddingLinearLayout, paddingLinearLayout);
            linearLayout.SetClipToPadding(false);

            var contentView = new DeliveryReportView(Context);
            linearLayout.AddView(contentView);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = null;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(DeliveryReportFragment)} " +
                $"[reference={referenceNumber}]");
        }

        public override void OnDestroyView()
        {
            dismissAction?.Invoke();
            base.OnDestroyView();
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();
        }


        #region Refresh methods

        async Task RefreshData()
        {
            try
            {
                await RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading delivery report failed [reference={referenceNumber}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                Activity?.OnBackPressed();
            }
        }

        async Task RefreshView()
        {
            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                if (linearLayout.GetChildAt(i) is DeliveryReportView dv)
                {
                    dv.SetData(referenceNumber, transmitDestination);
                    await dv.RefreshView();

                    if (linearLayout.GetChildAt(i + 1) is Divider d)
                    {
                        d.Visibility = dv.Visibility;
                        i++;
                    }
                }
            }

            linearLayout.Invalidate();
            linearLayout.RequestLayout();
        }
     

        #endregion

    }
}