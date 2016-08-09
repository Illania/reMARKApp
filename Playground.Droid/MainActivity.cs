using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.ServiceReference.Exceptions;
using PCLStorage;

namespace Playground.Droid
{
    [Activity(Label = "Playground.Droid", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ThreadPool.SetMinThreads(50, 50);
            ThreadPool.SetMaxThreads(100, 100);

            Task.Run(async () =>
            {
                var mainDataFolder = FileSystem.Current.LocalStorage;
                var dataFolder = await mainDataFolder.CreateFolderAsync("data", CreationCollisionOption.OpenIfExists, default(CancellationToken));
                var cacheFolder = await mainDataFolder.CreateFolderAsync(PortablePath.Combine("..", "cache"), CreationCollisionOption.OpenIfExists, default(CancellationToken));
                var dbFolder = await mainDataFolder.CreateFolderAsync("db", CreationCollisionOption.OpenIfExists, default(CancellationToken));
                var attachmentsFolder = await mainDataFolder.CreateFolderAsync("attachments", CreationCollisionOption.OpenIfExists, default(CancellationToken));

                CommonConfig.DataFolder = dataFolder;
                CommonConfig.CacheFolder = cacheFolder;
                CommonConfig.DatabaseFolder = dbFolder;
                CommonConfig.AttachmentsFolder = attachmentsFolder;

                try
                {
                    await DatabaseUtils.InitializeDatabases();
                }
                catch
                {
                    throw;
                }
            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine("Error in initialization" + t.Exception);
                }
            });

            SetContentView(Resource.Layout.AltMain);

            var button = FindViewById<Button>(Resource.Id.myButton);
            var textView = FindViewById<TextView>(Resource.Id.myTextView);

            button.Click += async (sender, e) =>
            {
                try
                {
                    var auth = AuthenticatorFactory.Create();

                    var result = await auth.AuthenticateAsync("mark5", "mark5", false, "192.168.75.51", 8093, DeviceType.Android, "test", "test");

                    textView.Text = result.Authenticated ? "Is Authenticated" : "Not Authneticated";

                    Managers.Initialize(result);

                    var result2 = await Managers.SystemManager.GetSystemSettingsAsync();

                    Console.WriteLine(result2);

                    var result3 = await Managers.FoldersManager.GetFoldersAsync(ModuleType.Documents, depth: 1);

                    Console.WriteLine(result3);

                    Log.Info("M5", "results ready");

                    var folder = new Folder
                    {
                        Id = -10,
                    };
                    var document = await Managers.DocumentsManager.GetDocumentAsync(folder, 2537, DocumentBodyTypeRequest.None);

                    var attachmentDescription = document.Attachments[0];

                    var result5 = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, document, folder, false);

                    textView.Text = result5;
                }
                catch (AppServiceException ex)
                {
                    var text = ex.Message + "   " + ex.Detail?.Code + "   " + ex.Detail?.DiagnosticInformation;
                    Log.Info("M5", text);
                }
                catch (Exception ex)
                {
                    Log.Info("M5", ex.ToString());
                    textView.Text = ex.ToString();
                    throw;
                }

                button.Enabled = true;
            };
        }
    }
}


