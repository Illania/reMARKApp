using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Common.CallId;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class DownloadViewController : AbstractViewController
    {

        struct ProgressInfo
        {
            public bool Preparing { get; }
            public int TotalItemsCount { get; }
            public int LeftItemsCount { get; }
            public int FailedItemsCount { get; }

            public ProgressInfo(bool preparing, int totalItemsCount, int leftItemsCount, int failedItemsCount)
            {
                Preparing = preparing;
                TotalItemsCount = totalItemsCount;
                LeftItemsCount = leftItemsCount;
                FailedItemsCount = failedItemsCount;
            }
        }

        struct FinishedInfo
        {
            public int DownloadedItemsCount { get; }
            public int FailedItemsCount { get; }

            public FinishedInfo(int downloadedItemsCount, int failedItemsCount)
            {
                DownloadedItemsCount = downloadedItemsCount;
                FailedItemsCount = failedItemsCount;
            }
        }

        public Folder Folder { get; set; }

        readonly TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        public Task Result => tcs.Task;

        UIBarButtonItem doneItem;

        UIView startView;
        UIView progressView;
        UIView finishedView;

        UIButton startButton;
        UILabel lastDownloadedLabel;
        UIActivityIndicatorView progressPreparingIndicator;
        UIProgressView progressIndicator;
        UILabel progressLabel;
        UIButton cancelButton;
        UILabel downloadedLabel;
        UIButton closeButton;

        Stopwatch sw;
        CancellationTokenSource cts;

        NSObject didEnterBackgroundNotification;

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeStartView();
            InitializeProgressView();
            InitializeFinishedView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            didEnterBackgroundNotification = UIApplication.Notifications.ObserveDidEnterBackground(DidEnterBackgroundNotification);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = false;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;

            InitializeNavigationBarTitle();
            InitializeHandlers();
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            var info = await Managers.FoldersManager.GetSavedFolderOfflineInfo(Folder);
            if (info != null)
                lastDownloadedLabel.Text = Localization.GetString("last_downloaded_on") + info.LastDownloaded.ConvertTimestampMillisecondsToDateTime()
        .ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds().FormatUserTimestampAsCompactLongDateTimeString();
            else
                lastDownloadedLabel.Text = null;

            CommonConfig.Logger.Info("Appeared");
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            UIApplication.SharedApplication.IdleTimerDisabled = false;

            DeinitializeHandlers();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            tcs.SetResult(true);
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            doneItem = null;

            startView = null;
            progressView = null;
            finishedView = null;

            startButton = null;
            lastDownloadedLabel = null;
            progressPreparingIndicator = null;
            progressIndicator = null;
            progressLabel = null;
            cancelButton = null;
            downloadedLabel = null;
            closeButton = null;

            didEnterBackgroundNotification?.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        void DidEnterBackgroundNotification(object sender, NSNotificationEventArgs e)
        {
            cts?.Cancel();
        }

        void InitializeStartView()
        {
            startView = new UIView
            {
                Alpha = 1f,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(startView);
            View.AddConstraints(new[] {
                startView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                startView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                startView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                startView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });

            var upView = new UIView
            {
                BackgroundColor = Theme.LightGray,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            startView.AddSubview(upView);
            startView.AddConstraints(new[] {
                upView.WidthAnchor.ConstraintEqualTo(startView.WidthAnchor),
                upView.HeightAnchor.ConstraintEqualTo(startView.HeightAnchor, .5f),
                upView.CenterXAnchor.ConstraintEqualTo(startView.CenterXAnchor),
                upView.TopAnchor.ConstraintEqualTo(startView.TopAnchor)
            });

            var imageView = new UIImageView
            {
                Image = UIImage.FromBundle("Download").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            upView.AddSubview(imageView);
            upView.AddConstraints(new[] {
                imageView.WidthAnchor.ConstraintEqualTo(120f),
                imageView.HeightAnchor.ConstraintEqualTo(120f),
                imageView.CenterXAnchor.ConstraintEqualTo(upView.CenterXAnchor),
                imageView.CenterYAnchor.ConstraintEqualTo(upView.CenterYAnchor)
            });

            var downView = new UIView
            {
                BackgroundColor = Theme.White,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            startView.AddSubview(downView);
            startView.AddConstraints(new[] {
                downView.WidthAnchor.ConstraintEqualTo(startView.WidthAnchor),
                downView.HeightAnchor.ConstraintEqualTo(startView.HeightAnchor, .5f),
                downView.CenterXAnchor.ConstraintEqualTo(startView.CenterXAnchor),
                downView.BottomAnchor.ConstraintEqualTo(startView.BottomAnchor)
            });

            startButton = new UIButton
            {
                TintColor = Theme.LightGray,
                BackgroundColor = Theme.DarkBlue,
                ContentEdgeInsets = new UIEdgeInsets(12.5f, 40f, 12.5f, 40f),
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true
            };
            startButton.TitleLabel.Font = Theme.DefaultLightFont;
            startButton.Layer.CornerRadius = 7.5f;

            switch (Folder.Module)
            {
                case ModuleType.Contacts:
                    startButton.SetTitle(Localization.GetString("download_contacts"), UIControlState.Normal);
                    break;
                case ModuleType.Shortcodes:
                    startButton.SetTitle(Localization.GetString("download_shortcodes"), UIControlState.Normal);
                    break;
                default:
                    startButton.SetTitle(Localization.GetString("download"), UIControlState.Normal);
                    break;
            }

            downView.AddSubview(startButton);
            downView.AddConstraints(new[]
            {
                startButton.CenterXAnchor.ConstraintEqualTo(downView.CenterXAnchor),
                startButton.CenterYAnchor.ConstraintEqualTo(downView.CenterYAnchor)
            });

            lastDownloadedLabel = new UILabel
            {
                TintColor = Theme.LightGray,
                Font = Theme.DefaultLightFont.WithRelativeSize(-2f),
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            downView.Add(lastDownloadedLabel);
            downView.AddConstraints(new[]
            {
                lastDownloadedLabel.TopAnchor.ConstraintEqualTo(startButton.BottomAnchor, 15f),
                lastDownloadedLabel.WidthAnchor.ConstraintEqualTo(downView.WidthAnchor),
                lastDownloadedLabel.CenterXAnchor.ConstraintEqualTo(downView.CenterXAnchor)
            });
        }

        void InitializeProgressView()
        {
            progressView = new UIView
            {
                Alpha = 0f,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(progressView);
            View.AddConstraints(new[] {
                progressView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                progressView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                progressView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                progressView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });

            var upView = new UIView
            {
                BackgroundColor = Theme.White,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            progressView.AddSubview(upView);
            progressView.AddConstraints(new[] {
                upView.WidthAnchor.ConstraintEqualTo(progressView.WidthAnchor),
                upView.HeightAnchor.ConstraintEqualTo(progressView.HeightAnchor, .5f),
                upView.CenterXAnchor.ConstraintEqualTo(progressView.CenterXAnchor),
                upView.TopAnchor.ConstraintEqualTo(progressView.TopAnchor)
            });

            var infoView = new UIView
            {
                BackgroundColor = Theme.DarkBlue,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
            };
            infoView.Layer.CornerRadius = 7.5f;
            upView.AddSubview(infoView);
            upView.AddConstraints(new[] {
                infoView.WidthAnchor.ConstraintEqualTo(upView.WidthAnchor, .8f),
                infoView.HeightAnchor.ConstraintEqualTo(upView.HeightAnchor, .8f),
                infoView.CenterXAnchor.ConstraintEqualTo(upView.CenterXAnchor),
                infoView.BottomAnchor.ConstraintEqualTo(upView.BottomAnchor)
            });

            var warningImage = new UIImageView
            {
                Image = UIImage.FromBundle("Warning").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                TintColor = Theme.LightGray,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            infoView.AddSubview(warningImage);
            infoView.AddConstraints(new[] {
                warningImage.WidthAnchor.ConstraintEqualTo(50f),
                warningImage.HeightAnchor.ConstraintEqualTo(50f),
                warningImage.CenterXAnchor.ConstraintEqualTo(infoView.CenterXAnchor),
                warningImage.CenterYAnchor.ConstraintEqualTo(infoView.CenterYAnchor, -40f)
            });

            var warningInfoLabel = new UILabel
            {
                TextColor = Theme.LightGray,
                Font = Theme.DefaultLightFont.WithRelativeSize(-2f),
                TextAlignment = UITextAlignment.Center,
                Text = Localization.GetString("dont_close_app"),
                Lines = 6,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            infoView.AddSubview(warningInfoLabel);
            infoView.AddConstraints(new[] {
                warningInfoLabel.WidthAnchor.ConstraintEqualTo(infoView.WidthAnchor, 0.8f),
                warningInfoLabel.HeightAnchor.ConstraintEqualTo(100f),
                warningInfoLabel.CenterXAnchor.ConstraintEqualTo(infoView.CenterXAnchor),
                warningInfoLabel.CenterYAnchor.ConstraintEqualTo(infoView.CenterYAnchor, 40f)
            });

            var downView = new UIView
            {
                BackgroundColor = Theme.White,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            progressView.AddSubview(downView);
            progressView.AddConstraints(new[] {
                downView.WidthAnchor.ConstraintEqualTo(progressView.WidthAnchor),
                downView.HeightAnchor.ConstraintEqualTo(progressView.HeightAnchor, .5f),
                downView.CenterXAnchor.ConstraintEqualTo(progressView.CenterXAnchor),
                downView.BottomAnchor.ConstraintEqualTo(progressView.BottomAnchor)
            });

            progressPreparingIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge)
            {
                Color = Theme.DarkBlue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            progressPreparingIndicator.StartAnimating();
            downView.AddSubview(progressPreparingIndicator);
            downView.AddConstraints(new[]
            {
                progressPreparingIndicator.CenterXAnchor.ConstraintEqualTo(downView.CenterXAnchor),
                progressPreparingIndicator.CenterYAnchor.ConstraintEqualTo(downView.CenterYAnchor, -25f)
            });

            progressIndicator = new UIProgressView
            {
                Hidden = true,
                TintColor = Theme.DarkBlue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            downView.AddSubview(progressIndicator);
            downView.AddConstraints(new[]
            {
                progressIndicator.WidthAnchor.ConstraintEqualTo(downView.WidthAnchor, .6f),
                progressIndicator.HeightAnchor.ConstraintEqualTo(5f),
                progressIndicator.CenterXAnchor.ConstraintEqualTo(downView.CenterXAnchor),
                progressIndicator.CenterYAnchor.ConstraintEqualTo(progressPreparingIndicator.CenterYAnchor)
            });

            progressLabel = new UILabel
            {
                TintColor = Theme.DarkBlue,
                Font = Theme.DefaultLightFont.WithRelativeSize(-2f),
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            downView.Add(progressLabel);
            downView.AddConstraints(new[]
            {
                progressLabel.TopAnchor.ConstraintEqualTo(progressPreparingIndicator.BottomAnchor, 15f),
                progressLabel.WidthAnchor.ConstraintEqualTo(downView.WidthAnchor),
                progressLabel.CenterXAnchor.ConstraintEqualTo(downView.CenterXAnchor)
            });

            cancelButton = new UIButton
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            cancelButton.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);
            cancelButton.SetTitle(Localization.GetString("cancel"), UIControlState.Normal);
            downView.Add(cancelButton);
            downView.AddConstraints(new[]
            {
                cancelButton.BottomAnchor.ConstraintEqualTo(downView.BottomAnchor, -25f),
                cancelButton.CenterXAnchor.ConstraintEqualTo(downView.CenterXAnchor)
            });
        }

        void InitializeFinishedView()
        {
            finishedView = new UIView
            {
                Alpha = 0f,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(finishedView);
            View.AddConstraints(new[] {
                finishedView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                finishedView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                finishedView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                finishedView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });

            var upView = new UIView
            {
                BackgroundColor = Theme.LightGray,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            finishedView.AddSubview(upView);
            finishedView.AddConstraints(new[] {
                upView.WidthAnchor.ConstraintEqualTo(finishedView.WidthAnchor),
                upView.HeightAnchor.ConstraintEqualTo(finishedView.HeightAnchor, .5f),
                upView.CenterXAnchor.ConstraintEqualTo(finishedView.CenterXAnchor),
                upView.TopAnchor.ConstraintEqualTo(finishedView.TopAnchor)
            });

            var imageView = new UIImageView
            {
                Image = UIImage.FromBundle("Done").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            upView.AddSubview(imageView);
            upView.AddConstraints(new[] {
                imageView.WidthAnchor.ConstraintEqualTo(120f),
                imageView.HeightAnchor.ConstraintEqualTo(120f),
                imageView.CenterXAnchor.ConstraintEqualTo(upView.CenterXAnchor),
                imageView.CenterYAnchor.ConstraintEqualTo(upView.CenterYAnchor)
            });

            var downView = new UIView
            {
                BackgroundColor = Theme.White,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            finishedView.AddSubview(downView);
            finishedView.AddConstraints(new[] {
                downView.WidthAnchor.ConstraintEqualTo(finishedView.WidthAnchor),
                downView.HeightAnchor.ConstraintEqualTo(finishedView.HeightAnchor, .5f),
                downView.CenterXAnchor.ConstraintEqualTo(finishedView.CenterXAnchor),
                downView.BottomAnchor.ConstraintEqualTo(finishedView.BottomAnchor)
            });

            closeButton = new UIButton
            {
                TintColor = Theme.LightGray,
                BackgroundColor = Theme.DarkBlue,
                ContentEdgeInsets = new UIEdgeInsets(12.5f, 40f, 12.5f, 40f),
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true
            };
            closeButton.TitleLabel.Font = Theme.DefaultLightFont;
            closeButton.Layer.CornerRadius = 7.5f;
            closeButton.SetTitle(Localization.GetString("continue"), UIControlState.Normal);
            downView.AddSubview(closeButton);
            downView.AddConstraints(new[]
            {
                closeButton.CenterXAnchor.ConstraintEqualTo(downView.CenterXAnchor),
                closeButton.CenterYAnchor.ConstraintEqualTo(downView.CenterYAnchor)
            });

            downloadedLabel = new UILabel
            {
                TintColor = Theme.LightGray,
                Lines = 2,
                Font = Theme.DefaultLightFont.WithRelativeSize(-2f),
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            downView.Add(downloadedLabel);
            downView.AddConstraints(new[]
            {
                downloadedLabel.BottomAnchor.ConstraintEqualTo(closeButton.TopAnchor, -25f),
                downloadedLabel.WidthAnchor.ConstraintEqualTo(downView.WidthAnchor),
                downloadedLabel.CenterXAnchor.ConstraintEqualTo(downView.CenterXAnchor)
            });
        }

        void InitializeNavigationBar()
        {
            NavigationItem.SetRightBarButtonItem(doneItem = new UIBarButtonItem(UIBarButtonSystemItem.Done), false);
        }

        void InitializeNavigationBarTitle() => NavigationItem.Title = Localization.GetString("download");

        void InitializeHandlers()
        {
            if (doneItem != null)
                doneItem.Clicked += DoneItem_Clicked;

            if (startButton != null)
                startButton.TouchUpInside += StartButton_TouchUpInside;

            if (cancelButton != null)
                cancelButton.TouchUpInside += CancelButton_TouchUpInside;

            if (closeButton != null)
                closeButton.TouchUpInside += CloseButton_TouchUpInside;
        }

        void DeinitializeHandlers()
        {
            if (doneItem != null)
                doneItem.Clicked -= DoneItem_Clicked;

            if (startButton != null)
                startButton.TouchUpInside -= StartButton_TouchUpInside;

            if (cancelButton != null)
                cancelButton.TouchUpInside -= CancelButton_TouchUpInside;

            if (closeButton != null)
                closeButton.TouchUpInside -= CloseButton_TouchUpInside;
        }

        void DoneItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);

        async void StartButton_TouchUpInside(object sender, EventArgs e)
        {
            if (!CommonConfig.Reachability.IsReachable)
            {
                await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("youre_offline_title"), Localization.GetString("youre_offline_message"));
                return;
            }

            void OnStart()
            {
                CommonConfig.Logger.Info($"Starting download... [folder.Id={Folder.Id}, folder.Module={Folder.Module}, folder.Name={Folder.Name}]");

                InvokeOnMainThread(() =>
                {
                    doneItem.Enabled = false;

                    progressPreparingIndicator.Hidden = false;
                    progressIndicator.Hidden = true;
                    progressIndicator.Progress = 0f;
                    progressLabel.Text = null;
                    cancelButton.Enabled = true;

                    UIView.AnimateNotify(.2d, 0d, UIViewAnimationOptions.CurveLinear, () =>
                    {
                        startView.Alpha = 0f;
                        progressView.Alpha = 1f;
                        finishedView.Alpha = 0f;
                    }, null);

                    sw = new Stopwatch();

                    UIApplication.SharedApplication.IdleTimerDisabled = false;
                    UIApplication.SharedApplication.IdleTimerDisabled = true;
                });
            }

            void OnProgress(ProgressInfo pi)
            {
                if (pi.Preparing || pi.LeftItemsCount % 10 == 0)
                    CommonConfig.Logger.Info($"Downloading... [folder.Id={Folder.Id}, folder.Module={Folder.Module}, folder.Name={Folder.Name}, preparing={pi.Preparing}, totalItemsCount={pi.TotalItemsCount}, leftItemsCount={pi.LeftItemsCount}, failedItemsCount={pi.FailedItemsCount}]");

                if (!pi.Preparing && pi.LeftItemsCount == pi.TotalItemsCount)
                    sw.Start();

                if (!pi.Preparing && pi.LeftItemsCount % 5 != 0)
                    return;

                var timeLeft = -1f;

                if (!pi.Preparing && sw.ElapsedMilliseconds > 3000)
                    timeLeft = sw.ElapsedMilliseconds / (pi.TotalItemsCount - pi.LeftItemsCount) * pi.LeftItemsCount / 1000 / 60;

                InvokeOnMainThread(() =>
                {
                    if (pi.Preparing)
                    {
                        progressPreparingIndicator.Hidden = false;
                        progressIndicator.Hidden = true;

                        progressLabel.Text = Localization.GetString("preparing___");
                    }
                    else
                    {
                        progressPreparingIndicator.Hidden = true;
                        progressIndicator.Hidden = false;

                        progressIndicator.SetProgress(1 - (pi.LeftItemsCount / (float)pi.TotalItemsCount), true);

                        progressLabel.Text = Localization.GetFormattedString("downloading_percentage___", (int)(progressIndicator.Progress * 100));
                        if (timeLeft > 0)
                            progressLabel.Text += " " + Localization.GetFormattedString("time_remaining_minutes", (int)timeLeft);
                        else if (timeLeft > -1)
                            progressLabel.Text += " " + Localization.GetString("time_remaining_less_than_minute");
                    }

                    UIApplication.SharedApplication.IdleTimerDisabled = false;
                    UIApplication.SharedApplication.IdleTimerDisabled = true;
                });
            }

            void OnFinished(FinishedInfo fi)
            {
                CommonConfig.Logger.Info($"Finished [folder.Id={Folder.Id}, folder.Module={Folder.Module}, folder.Name={Folder.Name}, downloadedItemsCount={fi.DownloadedItemsCount}, failedItemsCount={fi.FailedItemsCount}]");

                sw.Stop();
                sw = null;

                InvokeOnMainThread(() =>
                {
                    doneItem.Enabled = true;

                    downloadedLabel.Text = fi.DownloadedItemsCount > 0 ? Localization.GetString("download_complete") : Localization.GetString("download_complete_empty");

                    UIView.AnimateNotify(.2d, 0d, UIViewAnimationOptions.CurveLinear, () =>
                    {
                        startView.Alpha = 0f;
                        progressView.Alpha = 0f;
                        finishedView.Alpha = 1f;
                    }, null);

                    UIApplication.SharedApplication.IdleTimerDisabled = false;

                    var hapticGenerator = new UINotificationFeedbackGenerator();
                    hapticGenerator.NotificationOccurred(UINotificationFeedbackType.Success);
                });
            }

            void OnException(Exception ex)
            {
                CommonConfig.Logger.Error($"Error occured when downloading! [folder.Id={Folder.Id}, folder.Module={Folder.Module}, folder.Name={Folder.Name}]", ex);

                sw.Stop();
                sw = null;

                InvokeOnMainThread(() =>
                {
                    doneItem.Enabled = true;

                    UIView.AnimateNotify(.2d, 0d, UIViewAnimationOptions.CurveLinear, () =>
                    {
                        startView.Alpha = 1f;
                        progressView.Alpha = 0f;
                        finishedView.Alpha = 0f;
                    }, null);

                    UIApplication.SharedApplication.IdleTimerDisabled = false;

                    var hapticGenerator = new UINotificationFeedbackGenerator();
                    hapticGenerator.NotificationOccurred(UINotificationFeedbackType.Error);

                    Dialogs.ShowErrorAlert(this, ex);
                });
            }

            void OnCancelled()
            {
                CommonConfig.Logger.Info($"Cancelled download [folder.Id={Folder.Id}, folder.Module={Folder.Module}, folder.Name={Folder.Name}]");

                sw.Stop();
                sw = null;

                InvokeOnMainThread(() =>
                {
                    doneItem.Enabled = true;

                    UIView.AnimateNotify(.2d, 0d, UIViewAnimationOptions.CurveLinear, () =>
                    {
                        startView.Alpha = 1f;
                        progressView.Alpha = 0f;
                        finishedView.Alpha = 0f;
                    }, null);

                    UIApplication.SharedApplication.IdleTimerDisabled = false;
                });
            }


            cts?.Cancel();
            cts = new CancellationTokenSource();

            await Task.Run(async () => await Download(Folder, OnStart, OnProgress, OnFinished, OnException, OnCancelled, cts.Token));
        }

        async void CancelButton_TouchUpInside(object sender, EventArgs e)
        {
            ((UIButton)sender).Enabled = false;

            var result = await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("warning"), Localization.GetString("download_interrupt_warning"));
            if (result)
                cts?.Cancel();
            else
                ((UIButton)sender).Enabled = true;
        }

        void CloseButton_TouchUpInside(object sender, EventArgs e) => DismissViewController(true, null);

        async Task Download(Folder folder,
                            Action onStartedAction,
                            Action<ProgressInfo> onProgressAction,
                            Action<FinishedInfo> onFinishedAction,
                            Action<Exception> onException,
                            Action onCancelled,
                            CancellationToken ct)
        {
            try
            {
                ActivityIndicator.Show();

                CommonConfig.Logger.Info($"Starting download of folder {folder.Name} [folder.id={folder.Id}, folder.module={folder.Module}]");

                onStartedAction();

                //Different queues for each module, since more info is needed for contacts.
                var shortcodeQueue = new Queue<int>();
                var contactsQueue = new Queue<(int id, string name)>();

                var lastBatchCount = -1;
                var startRowId = -1;

                CommonConfig.Logger.Info($"Starting preparation to download a folder {folder.Name}. [folder.id={folder.Id}, folder.module={folder.Module}]");

                do
                {
                    if (folder.Module == ModuleType.Contacts)
                    {
                        var result = await Managers.ContactsManager.GetContactPreviewsAsync(folder, startRowId, SourceType.Remote);

                        result.ForEach(cp => contactsQueue.Enqueue((cp.Id, cp.Name)));
                        startRowId = result.LastOrDefault()?.RowId ?? -1;
                        lastBatchCount = result.Count;

                        result.Clear();
                        result = null;

                        onProgressAction(new ProgressInfo(true, contactsQueue.Count, -1, -1));
                    }
                    if (folder.Module == ModuleType.Shortcodes)
                    {
                        var result = await Managers.ShortcodesManager.GetShortcodePreviewsAsync(folder, startRowId, SourceType.Remote);

                        result.ForEach(cp => shortcodeQueue.Enqueue(cp.Id));
                        startRowId = result.LastOrDefault()?.RowId ?? -1;
                        lastBatchCount = result.Count;

                        result.Clear();
                        result = null;

                        onProgressAction(new ProgressInfo(true, shortcodeQueue.Count, -1, -1));
                    }


                } while (lastBatchCount >= Managers.ContactsManager.MaxToFetch && !ct.IsCancellationRequested);

                int queueCount = 0;
                if (folder.Module == ModuleType.Contacts)
                    queueCount = contactsQueue.Count;
                else if (folder.Module == ModuleType.Shortcodes)
                    queueCount = shortcodeQueue.Count;

                var totalItemsCount = queueCount;
                var leftItemsCount = queueCount;
                var failedItems = new List<int>();

                if (ct.IsCancellationRequested)
                {
                    onCancelled();
                    return;
                }

                CommonConfig.Logger.Info($"Folder {folder.Name} prepared to download. {totalItemsCount} items to download. [folder.id={folder.Id}, folder.module={folder.Module}]");

                onProgressAction(new ProgressInfo(false, totalItemsCount, leftItemsCount, failedItems.Count));

                if (folder.Module == ModuleType.Contacts)
                {
                    try
                    {
                        await CallIdContainerUtilities.CreateExtensionContactsTable();
                        await CallIdDataAccess.CleanExtensionContactsTable(folder.Id);
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error("Error while creating or cleaning contacts table for Call Id Extension", ex);
                    }
                }

                do
                {
                    int item;
                    string contactName = null;

                    try
                    {
                        if (folder.Module == ModuleType.Contacts)
                        {
                            (var qId, var qName) = contactsQueue.Dequeue();
                            item = qId;
                            contactName = qName;
                        }
                        else //if (folder.Module == ModuleType.Shortcodes)
                            item = shortcodeQueue.Dequeue();

                        leftItemsCount--;
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }

                    try
                    {
                        if (folder.Module == ModuleType.Contacts)
                        {
                            var contact = await Managers.ContactsManager.GetContactAsync(folder, item, SourceType.Remote);
                            try
                            {
                                if (contact.CommunicationAddresses.Count > 0)
                                {
                                    var caList = contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Phone || ca.Type == CommunicationAddressType.Mobile).ToList();

                                    if (caList.Count > 0)
                                    {
                                        try
                                        {
                                            foreach (CommunicationAddress ca in caList)
                                            {
                                                await CallIdDataAccess.AddContactToExtensionContactsTable(folder.Id, contactName, ca.Address);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            CommonConfig.Logger.Error("Error while adding contact to contacts table for Call Id Extension", ex);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                onException(ex);
                                break;
                            }

                            async Task DeepDownload(Contact c)
                            {
                                foreach (var child in c.Children.Where(cp => cp.Type == ContactType.Department))
                                {
                                    var childResult = await Managers.ContactsManager.GetContactWithPreviewAsync(-1, child.Id, SourceType.Remote);

                                    if (childResult.Contact.Children.Any())
                                        await DeepDownload(childResult.Contact);
                                }
                                // Deep download of contacts is disabled
                                //foreach (var child in c.Children.Where(cp => cp.Type == ContactType.Person))
                                //await Managers.ContactsManager.GetContactWithPreviewAsync(-1, child.Id, SourceType.Remote);
                            }

                            await DeepDownload(contact);
                        }
                        else if (folder.Module == ModuleType.Shortcodes)
                            await Managers.ShortcodesManager.GetShortcodeAsync(folder, item, SourceType.Remote);
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error($"Item with ID {item} failed to download. [folder.id={folder.Id}, folder.name={folder.Name}, folder.module={folder.Module}]", ex);
                        failedItems.Add(item);
                    }

                    onProgressAction(new ProgressInfo(false, totalItemsCount, leftItemsCount, failedItems.Count));
                } while (!ct.IsCancellationRequested);

                if (ct.IsCancellationRequested)
                    onCancelled();
                else
                {
                    CommonConfig.Logger.Info($"Folder {folder.Name} downloaded. {totalItemsCount} items downloaded. {failedItems.Count} items failed. [folder.id={folder.Id}, folder.module={folder.Module}]");
                    CommonConfig.Logger.Warning($"Following items failed to download: {string.Join(", ", failedItems)}. [folder.id={folder.Id}]");

                    try
                    {
                        if (folder.Module == ModuleType.Contacts)
                            CallIdExtensionUtilities.ReloadExtension();
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error("Error while reloading Call Id Extension", ex);
                    }

                    if (totalItemsCount > 0)
                        await Managers.FoldersManager.AddSavedFolderInfo(folder);

                    onFinishedAction(new FinishedInfo(totalItemsCount - leftItemsCount, failedItems.Count));
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Unexpected exception when downloading a folder. [folder.id={folder.Id}, folder.name={folder.Name}, folder.module={folder.Module}]", ex);

                onException(ex);
            }
            finally
            {
                ActivityIndicator.Hide();
            }
        }
    }
}
