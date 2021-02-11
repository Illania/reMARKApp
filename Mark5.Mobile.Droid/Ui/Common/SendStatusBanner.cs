using System;
using System.Collections.Concurrent;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model.HubMessages;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public class SendStatusBanner
    {
        BaseAppCompatActivity activity;
        ConcurrentQueue<BannerInfo> queue;
        TinyMessageSubscriptionToken documentUploadStatusChangedToken;
        bool started;

        public SendStatusBanner(BaseAppCompatActivity activity)
        {
            this.activity = activity;
        }

        public void Start()
        {
            if (started)
                return;

            queue = new ConcurrentQueue<BannerInfo>();
            SubscribeToMessages();
            started = true;
        }

        public void Stop()
        {
            if (!started)
                return;

            UnsubscribeFromMessages();
            queue.Clear();
            started = false;
        }

        void SubscribeToMessages()
        {
            documentUploadStatusChangedToken = CommonConfig.MessengerHub.Subscribe<DocumentUploadStatusChangedMessage>(DocumentUploadStatusChanged);
        }

        void UnsubscribeFromMessages()
        {
            documentUploadStatusChangedToken?.Dispose();
        }

        void ShowBanner(BannerInfo info)
        {
            if (!started)
                return;

            var coordinator = activity.FindViewById<CoordinatorLayout>(Resource.Id.coordinator);
            var snackbar = Snackbar.Make(coordinator, info.TextRes, Snackbar.LengthShort);
            snackbar.AddCallback(new BannerCallback(OnBannerHidden));
            snackbar.View.SetBackgroundColor(new Android.Graphics.Color(ContextCompat.GetColor(activity, info.ColorRes)));
            snackbar.Show();
            Vibrate();
        }

        void Vibrate()
        {
            var vibrator = (Vibrator)activity.GetSystemService(Context.VibratorService);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                vibrator.Vibrate(VibrationEffect.CreateOneShot(250, VibrationEffect.DefaultAmplitude));
            else
            {
#pragma warning disable 612, 618
                vibrator.Vibrate(250);
#pragma warning restore 612, 618
            }
        }

        void QueueBanner(BannerInfo info)
        {
            if (queue.IsEmpty)
                ShowBanner(info);

            queue.Enqueue(info);
        }

        void OnBannerHidden()
        {
            queue.TryDequeue(out _);

            if (queue.TryPeek(out var info))
                ShowBanner(info);
        }

        void DocumentUploadStatusChanged(DocumentUploadStatusChangedMessage obj)
        {
            if (obj.Change != DocumentUploadStatusChangedMessage.Status.DocumentSent
                && obj.Change != DocumentUploadStatusChangedMessage.Status.DocumentSentFailed
                && obj.Change != DocumentUploadStatusChangedMessage.Status.DocumentDelayed
                && obj.Change != DocumentUploadStatusChangedMessage.Status.DocumentSendCancelled)
                return;

            QueueBanner(BannerInfo.FromMessage(obj));
        }

        class BannerInfo
        {
            public int TextRes { get; private set; }
            public int ColorRes { get; private set; }

            private BannerInfo() { }

            public static BannerInfo FromMessage(DocumentUploadStatusChangedMessage msg)
            {
                var status = msg.Change;
                var isDraft = msg.IsDraft;

                var bannerInfo = new BannerInfo();
                if (status == DocumentUploadStatusChangedMessage.Status.DocumentSent)
                {
                    bannerInfo.TextRes = isDraft ? Resource.String.draft_saved : Resource.String.email_sent;
                    bannerInfo.ColorRes = Resource.Color.darkerblue;
                }
                else if(status == DocumentUploadStatusChangedMessage.Status.DocumentDelayed)
                {
                    bannerInfo.TextRes = Resource.String.delayed_send_queued;
                    bannerInfo.ColorRes = Resource.Color.darkgray;
                }
                else if (status == DocumentUploadStatusChangedMessage.Status.DocumentSentFailed)
                {
                    bannerInfo.TextRes = isDraft ? Resource.String.draft_error : Resource.String.email_error;
                    bannerInfo.ColorRes = Resource.Color.red;
                }
                else if (status == DocumentUploadStatusChangedMessage.Status.DocumentSendCancelled)
                {
                    bannerInfo.TextRes = Resource.String.send_cancelled;
                    bannerInfo.ColorRes = Resource.Color.darkgray;
                }

                return bannerInfo;
            }
        }


        class BannerCallback : BaseTransientBottomBar.BaseCallback
        {
            readonly Action onDismissed;

            public BannerCallback(Action onDismissed)
            {
                this.onDismissed = onDismissed;
            }

            public override void OnDismissed(Java.Lang.Object transientBottomBar, int e)
            {
                base.OnDismissed(transientBottomBar, e);
                onDismissed?.Invoke();
            }
        }
    }


}
