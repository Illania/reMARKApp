using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;
using System.Threading;
using Mark5.Mobile.Common.Managers;
using System.Collections.Generic;
using System.Linq;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class DownloadViewController : AbstractViewController
    {

        struct ProgressInfo
        {
            public bool Preparing { get; }
            public int TotalItems { get; }
            public int LeftItems { get; }
            public int ErrorsCount { get; }

            public ProgressInfo(bool preparing, int totalItems, int leftItems, int errorsCount)
            {
                Preparing = preparing;
                TotalItems = totalItems;
                LeftItems = leftItems;
                ErrorsCount = errorsCount;
            }
        }

        public Folder Folder { get; set; }

        UIBarButtonItem doneItem;

        UILabel progress;
        UIButton startButton;
        UIButton stopButton;

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
                Lines = 4
            };
            View.AddSubview(progress);
            View.AddConstraints(new[] {
                progress.WidthAnchor.ConstraintEqualTo(View.WidthAnchor),
                progress.HeightAnchor.ConstraintEqualTo(88f),
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
                    progress.Text = "Starting...";
                    startButton.Enabled = false;
                    stopButton.Enabled = true;

                    UIApplication.SharedApplication.IdleTimerDisabled = false;
                    UIApplication.SharedApplication.IdleTimerDisabled = true;
                });
            }

            void OnProgress(ProgressInfo pi)
            {
                InvokeOnMainThread(() =>
                {
                    progress.Text = $"Preparing={pi.Preparing}\nTotalItems={pi.TotalItems}\nLeftItems={pi.LeftItems}\nErrorsCount={pi.ErrorsCount}";

                    UIApplication.SharedApplication.IdleTimerDisabled = false;
                    UIApplication.SharedApplication.IdleTimerDisabled = true;
                });
            }

            void OnFinish()
            {
                InvokeOnMainThread(() =>
                {
                    progress.Text = "Stopped";
                    startButton.Enabled = true;
                    stopButton.Enabled = false;

                    UIApplication.SharedApplication.IdleTimerDisabled = false;
                });
            }

            Task.Run(async () => await Download(OnStart, OnProgress, OnFinish, CancellationToken.None));
        }

        void StopButton_TouchUpInside(object sender, EventArgs e)
        {

        }

        async Task Download(Action startedAction, Action<ProgressInfo> progressAction, Action finishedAction, CancellationToken ct)
        {
            try
            {
                startedAction();

                var lastBatchCount = -1;
                var startRowId = -1;
                var queue = new Queue<int>();

                do
                {
                    var result = await Managers.ContactsManager.GetContactPreviewsAsync(Folder, startRowId, SourceType.Remote);

                    result.ForEach(cp => queue.Enqueue(cp.Id));
                    startRowId = result.Last().RowId;
                    lastBatchCount = result.Count;

                    result.Clear();
                    result = null;

                    progressAction(new ProgressInfo(true, queue.Count, -1, 0));
                } while (lastBatchCount >= Managers.ContactsManager.MaxToFetch && !ct.IsCancellationRequested);

                var totalItems = queue.Count;
                var errors = 0;

                if (ct.IsCancellationRequested)
                {
                    finishedAction();
                    return;
                }

                progressAction(new ProgressInfo(false, totalItems, queue.Count, errors));

                do
                {
                    int item = -1;
                    try
                    {
                        item = queue.Dequeue();
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }

                    try
                    {
                        await Managers.ContactsManager.GetContactAsync(Folder, item, SourceType.Remote);
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error(ex);
                        errors++;
                    }

                    progressAction(new ProgressInfo(false, totalItems, queue.Count, errors));
                } while (!ct.IsCancellationRequested);

                finishedAction();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);

                finishedAction();
            }
        }
    }
}
