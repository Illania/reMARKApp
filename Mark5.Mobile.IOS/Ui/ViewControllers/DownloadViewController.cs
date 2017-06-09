using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
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

        UIBarButtonItem doneItem;

        UILabel progress;
        UIButton startButton;
        UIButton stopButton;

        Stopwatch sw;
        CancellationTokenSource cts;

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExtendedLayoutIncludesOpaqueBars = true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeNavigationBarTitle();
            InitializeHandlers();

            ReachabilityBar.Attach(View, null, (float) NavigationController.BottomLayoutGuide.Length);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(DownloadViewController)} appeared");
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(DownloadViewController)} received memory warning!");

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        void InitializeView()
        {
            progress = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "Ready",
                Lines = 5
            };
            View.AddSubview(progress);
            View.AddConstraints(new[] {
                progress.WidthAnchor.ConstraintEqualTo(View.WidthAnchor),
                progress.HeightAnchor.ConstraintEqualTo(120f),
                progress.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                progress.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
            });

            startButton = UIButton.FromType(UIButtonType.RoundedRect);
            startButton.TranslatesAutoresizingMaskIntoConstraints = false;
            startButton.TintColor = UIColor.Green;
            startButton.SetTitle("Start", UIControlState.Normal);
            View.AddSubview(startButton);
            View.AddConstraints(new[] {
                startButton.WidthAnchor.ConstraintEqualTo(View.WidthAnchor),
                startButton.HeightAnchor.ConstraintEqualTo(22f),
                startButton.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                startButton.TopAnchor.ConstraintEqualTo(progress.BottomAnchor),
            });

            stopButton = UIButton.FromType(UIButtonType.RoundedRect);
            stopButton.TranslatesAutoresizingMaskIntoConstraints = false;
            stopButton.Enabled = false;
            stopButton.TintColor = UIColor.Red;
            stopButton.SetTitle("Stop", UIControlState.Normal);
            View.AddSubview(stopButton);
            View.AddConstraints(new[] {
                stopButton.WidthAnchor.ConstraintEqualTo(View.WidthAnchor),
                stopButton.HeightAnchor.ConstraintEqualTo(22f),
                stopButton.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                stopButton.TopAnchor.ConstraintEqualTo(startButton.BottomAnchor),
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

            if (stopButton != null)
                stopButton.TouchUpInside += StopButton_TouchUpInside;
        }

        void DeinitializeHandlers()
        {
            if (doneItem != null)
                doneItem.Clicked -= DoneItem_Clicked;

            if (startButton != null)
                startButton.TouchUpInside -= StartButton_TouchUpInside;

            if (stopButton != null)
                stopButton.TouchUpInside -= StopButton_TouchUpInside;
        }

        void DoneItem_Clicked(object sender, EventArgs e) => NavigationController.DismissViewController(true, null);

        void StartButton_TouchUpInside(object sender, EventArgs e)
        {
            void OnStart()
            {
                InvokeOnMainThread(() =>
                {
                    doneItem.Enabled = false;
                    progress.Text = "Starting...";
                    startButton.Enabled = false;
                    stopButton.Enabled = true;

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
                    progress.Text = $"Preparing={pi.Preparing}\nTotalItems={pi.TotalItemsCount}\nLeftItems={pi.LeftItemsCount}\nErrorsCount={pi.FailedItemsCount}\nETA=around {timeLeft} minutes";

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
                    progress.Text = "Stopped";
                    startButton.Enabled = true;
                    stopButton.Enabled = false;

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
                    progress.Text = "Exception";
                    startButton.Enabled = true;
                    stopButton.Enabled = false;

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
            stopButton.Enabled = false;
            cts?.Cancel();
        }

        async Task Download(Folder folder, Action onStartedAction, Action<ProgressInfo> onProgressAction, Action<FinishedInfo> onFinishedAction, Action<Exception> onException, CancellationToken ct)
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
                    var result = await Managers.ContactsManager.GetContactPreviewsAsync(folder, startRowId, SourceType.Remote);

                    result.ForEach(cp => queue.Enqueue(cp.Id));
                    startRowId = result.Last().RowId;
                    lastBatchCount = result.Count;

                    result.Clear();
                    result = null;

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
                        await Managers.ContactsManager.GetContactAsync(folder, item, SourceType.Remote);
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error($"Item with ID {item} failed to download. [folder.id={folder.Id}, folder.name={folder.Name}, folder.module={folder.Module}]", ex);
                        failedItemsCount++;
                    }

                    onProgressAction(new ProgressInfo(false, totalItemsCount, leftItemsCount, failedItemsCount));
                } while (!ct.IsCancellationRequested);

                CommonConfig.Logger.Info($"Folder {folder.Name} downloaded. {totalItemsCount} items downloaded. {failedItemsCount} items failed. [folder.id={folder.Id}, folder.module={folder.Module}]");

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
