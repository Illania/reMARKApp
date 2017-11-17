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
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

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

        LinearLayout startLayout;
        LinearLayout progressLayout;
        LinearLayout finishedLayout;

        AppCompatButton startButton;
        AppCompatTextView lastDownloadedOn;
        ProgressBar progressBar;
        AppCompatTextView progressStatus;
        AppCompatButton cancelButton;
        AppCompatTextView downloaded;
        AppCompatButton closeButton;

        Stopwatch sw;
        CancellationTokenSource cts;
        PowerManager.WakeLock wakelock;
        bool downloadRunning;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(DownloadFragment)} [folder.Id={Folder?.Id}]...");

            var rootView = inflater.Inflate(Resource.Layout.download, container, false);

            startLayout = rootView.FindViewById<LinearLayout>(Resource.Id.start_layout);
            progressLayout = rootView.FindViewById<LinearLayout>(Resource.Id.progress_layout);
            finishedLayout = rootView.FindViewById<LinearLayout>(Resource.Id.finished_layout);

            startButton = rootView.FindViewById<AppCompatButton>(Resource.Id.start_button);
            lastDownloadedOn = rootView.FindViewById<AppCompatTextView>(Resource.Id.last_downloaded_on_text_view);
            progressBar = rootView.FindViewById<ProgressBar>(Resource.Id.progress_bar);
            progressStatus = rootView.FindViewById<AppCompatTextView>(Resource.Id.progress_status);
            cancelButton = rootView.FindViewById<AppCompatButton>(Resource.Id.cancel_button);
            downloaded = rootView.FindViewById<AppCompatTextView>(Resource.Id.downloaded);
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
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = Folder.Name;

            CommonConfig.Logger.Info($"Created {nameof(DownloadFragment)}");
        }

        public override async void OnResume()
        {
            base.OnResume();

            var info = await Managers.FoldersManager.GetSavedFolderOfflineInfo(Folder);
            if (info != null)
                lastDownloadedOn.Text = GetString(Resource.String.last_downloaded_on) + " " + info.LastDownloaded.FormatUserTimestampAsCompactLongDateTimeString(Context);
            else
                lastDownloadedOn.Text = null;
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

        void StartButton_Click(object sender, EventArgs e)
        {
            if (!CommonConfig.Reachability.IsReachable)
            {
                Dialogs.ShowConfirmDialog(Context, Resource.String.youre_offline_title, Resource.String.youre_offline_message);
                return;
            }

            void OnStart()
            {
                CommonConfig.Logger.Info($"Starting download... [folder.Id={Folder.Id}, folder.Module={Folder.Module}, folder.Name={Folder.Name}]");

                wakelock?.Release();
                wakelock = null;

                var pm = (PowerManager)Context.GetSystemService(Context.PowerService);
                wakelock = pm.NewWakeLock(WakeLockFlags.ScreenDim, GenerateTag());
                wakelock.Acquire();

                Activity.RunOnUiThread(() =>
                {
                    downloadRunning = true;

                    progressStatus.Text = null;
                    cancelButton.Enabled = true;

                    progressLayout.Visibility = ViewStates.Visible;
                    startLayout.Visibility = ViewStates.Gone;
                    finishedLayout.Visibility = ViewStates.Gone;

                    sw = new Stopwatch();
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

                Activity.RunOnUiThread(() =>
                {
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
                });
            }

            void OnFinished(FinishedInfo fi)
            {
                CommonConfig.Logger.Info($"Finished [folder.Id={Folder.Id}, folder.Module={Folder.Module}, folder.Name={Folder.Name}, downloadedItemsCount={fi.DownloadedItemsCount}, failedItemsCount={fi.FailedItemsCount}]");

                wakelock?.Release();
                wakelock = null;

                sw.Stop();
                sw = null;

                Activity.RunOnUiThread(() =>
                {
                    downloaded.Text = fi.DownloadedItemsCount > 0
                        ? GetString(Resource.String.download_finished)
                        : GetString(Resource.String.download_finished_empty);
                    
                    finishedLayout.Visibility = ViewStates.Visible;
                    startLayout.Visibility = ViewStates.Gone;
                    progressLayout.Visibility = ViewStates.Gone;

                    downloadRunning = false;
                });
            }

            void OnException(Exception ex)
            {
                CommonConfig.Logger.Error($"Error occured when downloading! [folder.Id={Folder.Id}, folder.Module={Folder.Module}, folder.Name={Folder.Name}]", ex);

                wakelock?.Release();
                wakelock = null;

                sw.Stop();
                sw = null;

                Activity.RunOnUiThread(() =>
                {
                    startLayout.Visibility = ViewStates.Visible;
                    progressLayout.Visibility = ViewStates.Gone;
                    finishedLayout.Visibility = ViewStates.Gone;

                    Dialogs.ShowErrorDialog(Activity, ex);

                    downloadRunning = false;
                });
            }

            void OnCancelled()
            {
                CommonConfig.Logger.Info($"Cancelled download [folder.Id={Folder.Id}, folder.Module={Folder.Module}, folder.Name={Folder.Name}]");

                wakelock?.Release();
                wakelock = null;

                sw.Stop();
                sw = null;

                Activity.RunOnUiThread(() =>
                {
                    startLayout.Visibility = ViewStates.Visible;
                    progressLayout.Visibility = ViewStates.Gone;
                    finishedLayout.Visibility = ViewStates.Gone;

                    downloadRunning = false;
                });
            }

            cts?.Cancel();
            cts = new CancellationTokenSource();

            Task.Run(async () => await Download(Folder, OnStart, OnProgress, OnFinished, OnException, OnCancelled, cts.Token));
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

        public override string GenerateTag()
        {
            return $"{nameof(DownloadFragment)} [folder.Id={Folder.Id}]";
        }

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

                        result.ForEach(cp => contactsQueue.Enqueue((cp.Id,cp.Name)));
                        startRowId = result.LastOrDefault()?.RowId ?? -1;
                        lastBatchCount = result.Count;

                        result.Clear();
                        result = null;
                    }
                    if (folder.Module == ModuleType.Shortcodes)
                    {
                        var result = await Managers.ShortcodesManager.GetShortcodePreviewsAsync(folder, startRowId, SourceType.Remote);

                        result.ForEach(cp => shortcodeQueue.Enqueue(cp.Id));
                        startRowId = result.LastOrDefault()?.RowId ?? -1;
                        lastBatchCount = result.Count;

                        result.Clear();
                        result = null;
                    }

                    var count = folder.Module == ModuleType.Contacts ? contactsQueue.Count : shortcodeQueue.Count;

                    onProgressAction(new ProgressInfo(true, count, -1, -1));
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

                try
                {
                    await CallerIdDatabaseProvider.CallerIdDatabase.CreateTable();
                    await CallerIdDatabaseProvider.CallerIdDatabase.CleanTable(folder.Id);
                } 
                catch (Exception ex)
                {
                    onException(ex);
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
                        {
                            item = shortcodeQueue.Dequeue();
                        }
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
                                        foreach (CommunicationAddress ca in caList)
                                        {
                                            await CallerIdDatabaseProvider.CallerIdDatabase.AddContact(folder.Id, contactName, ca.Address);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                onException(ex);
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
                        if (folder.Module == ModuleType.Shortcodes)
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
                    CommonConfig.Logger.Warning($"Following items failed to download: [folder.id={folder.Id}]");
                    foreach (var failedItem in failedItems)
                        CommonConfig.Logger.Warning("   - " + failedItem);

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
        }
    }
}