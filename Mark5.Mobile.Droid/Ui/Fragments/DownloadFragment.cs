using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Android.Support.V7.Widget;
using System.Diagnostics;
using Android.Support.V7.App;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class DownloadFragment : RetainableStateFragment
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

        AppCompatTextView textView;
        AppCompatButton startButton, stopButton;

        Stopwatch sw;
        CancellationTokenSource cts;
        PowerManager.WakeLock wakelock;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(DownloadFragment)} [folder.Id={Folder?.Id}]...");

            var rootView = inflater.Inflate(Resource.Layout.download, container, false);

            textView = rootView.FindViewById<AppCompatTextView>(Resource.Id.textView);
            startButton = rootView.FindViewById<AppCompatButton>(Resource.Id.startButton);
            stopButton = rootView.FindViewById<AppCompatButton>(Resource.Id.stopButton);

            startButton.Click += StartButton_Click;
            stopButton.Click += StopButton_Click;

            textView.Text = "Ready";
            stopButton.Enabled = false;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = "Download";
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = Folder.Name;

            CommonConfig.Logger.Info($"Created {nameof(DownloadFragment)}");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            wakelock?.Release();
            wakelock = null;

            textView?.Dispose();
            startButton?.Dispose();
            stopButton?.Dispose();
        }

        void StartButton_Click(object sender, EventArgs e)
        {
            void OnStart()
            {
                wakelock?.Release();
                wakelock = null;

                var pm = (PowerManager)Context.GetSystemService(Context.PowerService);
                wakelock = pm.NewWakeLock(WakeLockFlags.ScreenDim, GenerateTag());
                wakelock.Acquire();

                Activity.RunOnUiThread(() =>
                {
                    textView.Text = "Starting...";
                    startButton.Enabled = false;
                    stopButton.Enabled = true;

                    sw = new Stopwatch();
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

                Activity.RunOnUiThread(() =>
                {
                    textView.Text = $"Preparing={pi.Preparing}\nTotalItems={pi.TotalItemsCount}\nLeftItems={pi.LeftItemsCount}\nErrorsCount={pi.FailedItemsCount}\nETA=around {timeLeft} minutes";
                });
            }

            void OnFinished(FinishedInfo fi)
            {
                wakelock?.Release();
                wakelock = null;

                sw.Stop();
                sw = null;

                Activity.RunOnUiThread(() =>
                {
                    textView.Text = "Stopped";
                    startButton.Enabled = true;
                    stopButton.Enabled = false;
                });
            }

            void OnException(Exception ex)
            {
                wakelock?.Release();
                wakelock = null;

                sw.Stop();
                sw = null;

                Activity.RunOnUiThread(() =>
                {
                    textView.Text = "Exception";
                    startButton.Enabled = true;
                    stopButton.Enabled = false;

                    Dialogs.ShowErrorDialog(Activity, ex);
                });
            }

            cts?.Cancel();
            cts = new CancellationTokenSource();

            Task.Run(async () => await Download(Folder, OnStart, OnProgress, OnFinished, OnException, cts.Token));
        }

        void StopButton_Click(object sender, EventArgs e)
        {
            stopButton.Enabled = false;
            cts?.Cancel();
        }

        public override string GenerateTag()
        {
            return $"{nameof(DownloadFragment)} [folder.Id={Folder.Id}]";
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