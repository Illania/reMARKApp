using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class DownloadFragment : RetainableStateFragment
    {
        public const string FolderBundleKey = "Folder_3a0c4202-ba8e-457d-9db7-025692ad0b35";

        Folder folder;

        LinearLayout startLayout;
        LinearLayout progressLayout;
        LinearLayout finishedLayout;

        AppCompatButton startButton;
        AppCompatTextView lastDownloadedOnTextView;
        ProgressBar progressBar;
        AppCompatTextView progressStatus;
        AppCompatButton cancelButton;
        AppCompatButton closeButton;

        Stopwatch sw;
        CancellationTokenSource cts;
        PowerManager.WakeLock wakelock;
        bool downloadRunning;

        public static (DownloadFragment fragment, string var) NewInstance(Folder folder)
        {
            var args = new Bundle();

            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            var fragment = new DownloadFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(DownloadFragment)} [folder.Id={folder.Id}]";

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (Arguments.ContainsKey(FolderBundleKey))
                folder = Serializer.Deserialize<Folder>(Arguments.GetString(FolderBundleKey));
            
            CommonConfig.Logger.Info($"Creating {nameof(DownloadFragment)} [folder.Id={folder?.Id}]...");

            var rootView = inflater.Inflate(Resource.Layout.download, container, false);

            startLayout = rootView.FindViewById<LinearLayout>(Resource.Id.start_layout);
            progressLayout = rootView.FindViewById<LinearLayout>(Resource.Id.progress_layout);
            finishedLayout = rootView.FindViewById<LinearLayout>(Resource.Id.finished_layout);

            startButton = rootView.FindViewById<AppCompatButton>(Resource.Id.start_button);
            lastDownloadedOnTextView = rootView.FindViewById<AppCompatTextView>(Resource.Id.last_downloaded_on_text_view);
            progressBar = rootView.FindViewById<ProgressBar>(Resource.Id.progress_bar);
            progressStatus = rootView.FindViewById<AppCompatTextView>(Resource.Id.progress_status);
            cancelButton = rootView.FindViewById<AppCompatButton>(Resource.Id.cancel_button);
            closeButton = rootView.FindViewById<AppCompatButton>(Resource.Id.close_button);

            startButton.Click += StartButton_Click;
            cancelButton.Click += CancelButton_Click;
            closeButton.Click += CloseButton_Click;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.download);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = folder.Name;

            CommonConfig.Logger.Info($"Created {nameof(DownloadFragment)}");
        }

        public override async void OnResume()
        {
            base.OnResume();

            var info = await Managers.FoldersManager.GetSavedFolderOfflineInfo(folder);
            if (info != null)
                lastDownloadedOnTextView.Text = GetString(Resource.String.last_downloaded_on) + " " + info.LastDownloaded.FormatUserTimestampAsCompactLongDateTimeString(Context);
            else
                lastDownloadedOnTextView.Text = null;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            wakelock?.Release();
            wakelock = null;
        }

        public bool OnBackPressed()
        {
            if (downloadRunning)
            {
                Toast.MakeText(Context, Resource.String.download_in_progress_cancel_to_go_back, ToastLength.Long).Show();
                return false;
            }

            return true;
        }

        async void StartButton_Click(object sender, EventArgs e)
        {
            if (!CommonConfig.Reachability.IsReachable)
            {
                Dialogs.ShowConfirmDialog(Context, Resource.String.youre_offline_title, Resource.String.youre_offline_message);
                return;
            }

            void OnStart()
            {
                CommonConfig.Logger.Info($"Starting download... [folder.Id={folder.Id}, folder.Module={folder.Module}, folder.Name={folder.Name}]");

                wakelock?.Release();
                wakelock = null;

                var pm = (PowerManager)Context.GetSystemService(Context.PowerService);
                wakelock = pm.NewWakeLock(WakeLockFlags.ScreenDim, $"{nameof(DownloadFragment)} [folder.Id={folder.Id}]");
                wakelock.Acquire();

                downloadRunning = true;

                progressStatus.Text = null;
                cancelButton.Enabled = true;

                progressLayout.Visibility = ViewStates.Visible;
                startLayout.Visibility = ViewStates.Gone;
                finishedLayout.Visibility = ViewStates.Gone;

                sw = new Stopwatch();           
            }

            void OnProgress(ProgressInfo pi)
            {
                CommonConfig.Logger.Info($"Downloading... [folder.Id={folder.Id}, folder.Module={folder.Module}, folder.Name={folder.Name}, preparing={pi.Preparing}, totalItemsCount={pi.TotalItemsCount}, leftItemsCount={pi.LeftItemsCount}, failedItemsCount={pi.FailedItemsCount}]");

                if (!pi.Preparing && pi.LeftItemsCount == pi.TotalItemsCount)
                    sw.Start();

                if (!pi.Preparing && pi.LeftItemsCount % 5 != 0)
                    return;

                var timeLeft = -1f;

                if (!pi.Preparing && sw.ElapsedMilliseconds > 3000)
                    timeLeft = sw.ElapsedMilliseconds / (pi.TotalItemsCount - pi.LeftItemsCount) * pi.LeftItemsCount / 1000 / 60;

                if (pi.Preparing)
                {
                    progressBar.Indeterminate = true;
                    progressStatus.Text = GetString(Resource.String.preparing);
                }
                else
                {
                    progressBar.Indeterminate = false;
                    progressBar.Max = pi.TotalItemsCount;
                    progressBar.Progress = pi.TotalItemsCount - pi.LeftItemsCount;

                    progressStatus.Text = GetString(Resource.String.downloading_percentage, (int)((1 - (pi.LeftItemsCount / (float)pi.TotalItemsCount)) * 100));

                    if (timeLeft > 0)
                        progressStatus.Text += "\n" + Resources.GetQuantityString(Resource.Plurals.time_remaining_minutes, (int)timeLeft, (int)timeLeft);
                    else if (timeLeft > -1)
                        progressStatus.Text += "\n" + GetString(Resource.String.time_remaining_minutes_zero);
                }
            }

            void OnFinished(FinishedInfo fi)
            {
                CommonConfig.Logger.Info($"Finished [folder.Id={folder.Id}, folder.Module={folder.Module}, folder.Name={folder.Name}, downloadedItemsCount={fi.DownloadedItemsCount}, failedItemsCount={fi.FailedItemsCount}]");

                wakelock?.Release();
                wakelock = null;

                sw.Stop();
                sw = null;

	            finishedLayout.Visibility = ViewStates.Visible;
	            startLayout.Visibility = ViewStates.Gone;
	            progressLayout.Visibility = ViewStates.Gone;

	            downloadRunning = false;
            }

            void OnException(Exception ex)
            {
                CommonConfig.Logger.Error($"Error occured when downloading! [folder.Id={folder.Id}, folder.Module={folder.Module}, folder.Name={folder.Name}]", ex);

                wakelock?.Release();
                wakelock = null;

                sw.Stop();
                sw = null;

	            startLayout.Visibility = ViewStates.Visible;
	            progressLayout.Visibility = ViewStates.Gone;
	            finishedLayout.Visibility = ViewStates.Gone;

	            Dialogs.ShowErrorDialog(Activity, ex);

	            downloadRunning = false;
            }

            void OnCancelled()
            {
                CommonConfig.Logger.Info($"Cancelled download [folder.Id={folder.Id}, folder.Module={folder.Module}, folder.Name={folder.Name}]");

                wakelock?.Release();
                wakelock = null;

                sw.Stop();
                sw = null;

	            startLayout.Visibility = ViewStates.Visible;
	            progressLayout.Visibility = ViewStates.Gone;
	            finishedLayout.Visibility = ViewStates.Gone;

	            downloadRunning = false;
            }

            cts?.Cancel();
            cts = new CancellationTokenSource();

            await Download(folder, OnStart, OnProgress, OnFinished, OnException, OnCancelled, cts.Token);
        }

        async void CancelButton_Click(object sender, EventArgs e)
        {
            cancelButton.Enabled = false;

            var result = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.warning, Resource.String.download_in_progress_warning);
            if (result)
                cts?.Cancel();
            else
                cancelButton.Enabled = true;
        }

        void CloseButton_Click(object sender, EventArgs e) => Activity?.Finish();

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
                    onCancelled();
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

                                // Deep download of contacts is disabled
                                //foreach (var child in c.Children.Where(cp => cp.Type == ContactType.Person))
                                //await Managers.ContactsManager.GetContactWithPreviewAsync(-1, child.Id, SourceType.Remote);
                            }

                            await DeepDownload(contact);
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

                if (ct.IsCancellationRequested)
                    onCancelled();
                else
                {
                    CommonConfig.Logger.Info($"Folder {folder.Name} downloaded. {totalItemsCount} items downloaded. {failedItemsCount} items failed. [folder.id={folder.Id}, folder.module={folder.Module}]");

                    await Managers.FoldersManager.AddSavedFolderInfo(folder);

                    onFinishedAction(new FinishedInfo(totalItemsCount - leftItemsCount, failedItemsCount));
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Unexpected exception when downloading a folder. [folder.id={folder.Id}, folder.name={folder.Name}, folder.module={folder.Module}]", ex);

                onException(ex);
            }
        }

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
    }
}