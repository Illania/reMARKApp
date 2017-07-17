using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using Mark5.Mobile.IOS.Utilities.Extensions;

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

        public Task DidDisappear { get => tcs.Task; }


        UIBarButtonItem doneItem;
        UIView startView;
        UIButton startButton;
        UILabel lastDownloadedLabel;

        Stopwatch sw;
        CancellationTokenSource cts;

        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeStartView();
            //InitializeProgressView();
            //InitializeFinishedView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExtendedLayoutIncludesOpaqueBars = false;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeNavigationBarTitle();
            InitializeHandlers();

            ReachabilityBar.Attach(View, null, (float)NavigationController.BottomLayoutGuide.Length);
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            var info = await Managers.FoldersManager.GetSavedFolderOfflineInfo(Folder);
            if (info != null)
                lastDownloadedLabel.Text = "Last downloaded on: " + info.LastDownloaded.FormatUserTimestampAsCompactLongDateTimeString();
            else
                lastDownloadedLabel.Text = null;

            CommonConfig.Logger.Info($"{nameof(DownloadViewController)} appeared");
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            tcs.SetResult(true);
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(DownloadViewController)} received memory warning!");

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        void InitializeStartView()
        {
            startView = new UIView
            {
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

            var imageView = new UIView
            {
                BackgroundColor = Theme.DarkBlue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            upView.AddSubview(imageView);
            upView.AddConstraints(new[] {
                imageView.WidthAnchor.ConstraintEqualTo(125f),
                imageView.HeightAnchor.ConstraintEqualTo(125f),
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
            startButton.SetTitle("Download contacts", UIControlState.Normal);
            downView.AddSubview(startButton);
            downView.AddConstraints(new[]
            {
                startButton.CenterXAnchor.ConstraintEqualTo(downView.CenterXAnchor),
                startButton.CenterYAnchor.ConstraintEqualTo(downView.CenterYAnchor)
            });

            lastDownloadedLabel = new UILabel
            {
                TintColor = Theme.LightGray,
                Font = Theme.DefaultLightFont.WithRelativeSize(-4f),
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
            var progressView = new UIView
            {
                Alpha = 0f
            };
            View.AddSubview(progressView);
            View.AddConstraints(new[] {
                progressView.WidthAnchor.ConstraintEqualTo(View.WidthAnchor),
                progressView.HeightAnchor.ConstraintEqualTo(View.HeightAnchor),
                progressView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                progressView.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor)
            });

        }

        void InitializeFinishedView()
        {
            var finishedView = new UIView
            {
                Alpha = 0f
            };
            View.AddSubview(finishedView);
            View.AddConstraints(new[] {
                finishedView.WidthAnchor.ConstraintEqualTo(View.WidthAnchor),
                finishedView.HeightAnchor.ConstraintEqualTo(View.HeightAnchor),
                finishedView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                finishedView.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor)
            });
        }

        void InitializeNavigationBar()
        {
            NavigationItem.SetRightBarButtonItem(doneItem = new UIBarButtonItem(UIBarButtonSystemItem.Done), false);
        }

        void InitializeNavigationBarTitle()
        {
            NavigationItem.Title = "Download";
        }

        void InitializeHandlers()
        {
            if (doneItem != null)
                doneItem.Clicked += DoneItem_Clicked;

            if (startButton != null)
                startButton.TouchUpInside += StartButton_TouchUpInside;

            //if (stopButton != null)
            //    stopButton.TouchUpInside += StopButton_TouchUpInside;
        }

        void DeinitializeHandlers()
        {
            if (doneItem != null)
                doneItem.Clicked -= DoneItem_Clicked;

            if (startButton != null)
                startButton.TouchUpInside -= StartButton_TouchUpInside;

            //if (stopButton != null)
            //    stopButton.TouchUpInside -= StopButton_TouchUpInside;
        }

        void DoneItem_Clicked(object sender, EventArgs e) => NavigationController.DismissViewController(true, null);

        void StartButton_TouchUpInside(object sender, EventArgs e)
        {
            void OnStart()
            {
                InvokeOnMainThread(() =>
                {
                    doneItem.Enabled = false;
                    //progress.Text = "Starting...";
                    //startButton.Enabled = false;
                    //stopButton.Enabled = true;

                    sw = new Stopwatch();

                    UIApplication.SharedApplication.IdleTimerDisabled = false;
                    UIApplication.SharedApplication.IdleTimerDisabled = true;
                });
            }

            void OnProgress(ProgressInfo pi)
            {
                if (!pi.Preparing && pi.LeftItemsCount == pi.TotalItemsCount)
                    sw.Start();

                if (!pi.Preparing && pi.LeftItemsCount % 5 != 0)
                    return;

                var timeLeft = -1f;

                if (!pi.Preparing && sw.ElapsedMilliseconds > 3000)
                    timeLeft = sw.ElapsedMilliseconds / (pi.TotalItemsCount - pi.LeftItemsCount) * pi.LeftItemsCount / 1000 / 60;

                InvokeOnMainThread(() =>
                {
                    //progress.Text = $"Preparing={pi.Preparing}\nTotalItems={pi.TotalItemsCount}\nLeftItems={pi.LeftItemsCount}\nErrorsCount={pi.FailedItemsCount}\nETA=around {timeLeft} minutes";

                    UIApplication.SharedApplication.IdleTimerDisabled = false;
                    UIApplication.SharedApplication.IdleTimerDisabled = true;
                });
            }

            void OnFinished(FinishedInfo fi)
            {
                sw.Stop();
                sw = null;

                InvokeOnMainThread(() =>
                {
                    doneItem.Enabled = true;
                    //progress.Text = "Stopped";
                    //startButton.Enabled = true;
                    //stopButton.Enabled = false;

                    UIApplication.SharedApplication.IdleTimerDisabled = false;
                });
            }

            void OnException(Exception ex)
            {
                sw.Stop();
                sw = null;

                InvokeOnMainThread(() =>
                {
                    doneItem.Enabled = true;
                    //progress.Text = "Exception";
                    //startButton.Enabled = true;
                    //stopButton.Enabled = false;

                    UIApplication.SharedApplication.IdleTimerDisabled = false;

                    Dialogs.ShowErrorDialog(this, ex);
                });
            }

            cts?.Cancel();
            cts = new CancellationTokenSource();

            Task.Run(async () => await Download(Folder, OnStart, OnProgress, OnFinished, OnException, cts.Token));
        }

        void StopButton_TouchUpInside(object sender, EventArgs e)
        {
            //stopButton.Enabled = false;
            cts?.Cancel();
        }

        async Task Download(Folder folder,
                            Action onStartedAction,
                            Action<ProgressInfo> onProgressAction,
                            Action<FinishedInfo> onFinishedAction,
                            Action<Exception> onException,
                            CancellationToken ct)
        {
            try
            {
                CommonConfig.Logger.Info($"Starting download of folder {folder.Name} [folder.id={folder.Id}, folder.module={folder.Module}]");

                onStartedAction();

                var queue = new Queue<int>();

                var lastBatchCount = -1;
                var startRowId = -1;

                CommonConfig.Logger.Info($"Starting preparation to download a folder {folder.Name}. [folder.id={folder.Id}, folder.module={folder.Module}]");

                do
                {
                    if (folder.Module == ModuleType.Contacts)
                    {
                        var result = await Managers.ContactsManager.GetContactPreviewsAsync(folder, startRowId, SourceType.Remote);

                        result.ForEach(cp => queue.Enqueue(cp.Id));
                        startRowId = result.Last().RowId;
                        lastBatchCount = result.Count;

                        result.Clear();
                        result = null;
                    }
                    if (folder.Module == ModuleType.Shortcodes)
                    {
                        var result = await Managers.ShortcodesManager.GetShortcodePreviewsAsync(folder, startRowId, SourceType.Remote);

                        result.ForEach(cp => queue.Enqueue(cp.Id));
                        startRowId = result.Last().RowId;
                        lastBatchCount = result.Count;

                        result.Clear();
                        result = null;
                    }

                    onProgressAction(new ProgressInfo(true, queue.Count, -1, -1));
                } while (lastBatchCount >= Managers.ContactsManager.MaxToFetch && !ct.IsCancellationRequested);

                var totalItemsCount = queue.Count;
                var leftItemsCount = queue.Count;
                var failedItemsCount = 0;

                if (ct.IsCancellationRequested)
                {
                    onFinishedAction(new FinishedInfo(0, 0));
                    return;
                }

                CommonConfig.Logger.Info($"Folder {folder.Name} prepared to download. {totalItemsCount} items to download. [folder.id={folder.Id}, folder.module={folder.Module}]");

                onProgressAction(new ProgressInfo(false, totalItemsCount, leftItemsCount, failedItemsCount));

                do
                {
                    int item;
                    try
                    {
                        item = queue.Dequeue();
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

                            async Task DeepDownload(Contact c)
                            {
                                foreach (var child in c.Children.Where(cp => cp.Type == ContactType.Department))
                                {
                                    var childResult = await Managers.ContactsManager.GetContactWithPreviewAsync(-1, child.Id, SourceType.Remote);

                                    if (childResult.Contact.Children.Any())
                                        await DeepDownload(childResult.Contact);
                                }

                                //foreach (var child in c.Children.Where(cp => cp.Type == ContactType.Person))
                                //await Managers.ContactsManager.GetContactWithPreviewAsync(-1, child.Id, SourceType.Remote);
                            }

                            await DeepDownload(contact); // TODO consider making this optional
                        }
                        if (folder.Module == ModuleType.Shortcodes)
                            await Managers.ShortcodesManager.GetShortcodeAsync(folder, item, SourceType.Remote);
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error($"Item with ID {item} failed to download. [folder.id={folder.Id}, folder.name={folder.Name}, folder.module={folder.Module}]", ex);
                        failedItemsCount++;
                    }

                    onProgressAction(new ProgressInfo(false, totalItemsCount, leftItemsCount, failedItemsCount));
                } while (!ct.IsCancellationRequested);

                CommonConfig.Logger.Info($"Folder {folder.Name} downloaded. {totalItemsCount} items downloaded. {failedItemsCount} items failed. [folder.id={folder.Id}, folder.module={folder.Module}]");

                await Managers.FoldersManager.AddSavedFolderInfo(folder);

                onFinishedAction(new FinishedInfo(totalItemsCount - leftItemsCount, failedItemsCount));
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Unexpected exception when downloading a folder. [folder.id={folder.Id}, folder.name={folder.Name}, folder.module={folder.Module}]", ex);

                onException(ex);
            }
        }
    }
}
