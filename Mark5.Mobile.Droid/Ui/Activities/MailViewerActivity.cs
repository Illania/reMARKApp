using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using HtmlAgilityPack;
using MailBee;
using MailBee.Html;
using MailBee.Mime;
using MailBee.Outlook;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model.Exceptions;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.MailViewerViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(Label = "MARK5 Mail Viewer", ScreenOrientation = ScreenOrientation.Portrait, Exported = true)]
    [IntentFilter(new[]
        {
            Intent.ActionView,
            Intent.ActionSend
        },
        Categories = new[]
        {
            Intent.CategoryDefault
        },
        DataMimeTypes = new[]
        {
            "application/octet-stream",
            "message/rfc822"
        })]
    public class MailViewerActivity : BaseAppCompatActivity
    {
        const long MaxSize = 5 * 1024 * 1024; // 5MB

        Toolbar toolbar;
        LinearLayoutCompat linearLayout;

        MailMessage mailMessage;

        public static Intent CreateIntent(Context context)
        {
            return new Intent(context, typeof(MailViewerActivity));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenMailViewerEvent());

            base.OnCreate(savedInstanceState);

            Global.LicenseKey = "MN110-C50DF2550CBE0D750DF4AF2E15D9-0B99";

            SetContentView(Resource.Layout.mailviewer_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            linearLayout = FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            linearLayout.AddView(new SubjectView(this));
            linearLayout.AddView(new Divider(this));
            linearLayout.AddView(new RecipentsView(this));
            linearLayout.AddView(new Divider(this));
            linearLayout.AddView(new PriorityView(this));
            linearLayout.AddView(new Divider(this));
            var av = new AttachmentsView(this);
            av.AttachmentClicked += AttachmentsView_AttachmentClicked;
            av.AttachmentLongClicked += AttachmentsView_AttachmentLongClicked;
            linearLayout.AddView(av);
            linearLayout.AddView(new ContentView(this));

            LoadMailFromUri();
        }

        static byte[] ReadToEnd(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, read);

                return ms.ToArray();
            }
        }

        static void InlineImages(MailMessage mm)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(mm.BodyHtmlText);

            var nodes = htmlDoc.DocumentNode.Descendants("img").Where(n => n.GetAttributeValue("src", null).StartsWith("cid:", StringComparison.CurrentCultureIgnoreCase)).ToArray();
            var atts = mm.Attachments;

            foreach (var node in nodes)
            {
                var srcAttrValue = node.GetAttributeValue("src", null);
                var cid = srcAttrValue.SafeSubstringAfter("cid:", StringComparison.CurrentCultureIgnoreCase);

                if (string.IsNullOrWhiteSpace(cid))
                    continue;

                MailBee.Mime.Attachment matchingAtt = null;
                foreach (var obj in atts)
                {
                    var att = (MailBee.Mime.Attachment)obj;
                    if (att.ContentID == cid)
                    {
                        matchingAtt = att;
                        break;
                    }
                }

                if (matchingAtt == null)
                    continue;

                var matchingAttExt = Path.GetExtension(matchingAtt.FilenameOriginal);

                if (string.IsNullOrWhiteSpace(matchingAttExt))
                    continue;

                node.SetAttributeValue("src", $"data:image/{matchingAttExt};base64,{Convert.ToBase64String(matchingAtt.GetData())}");
            }

            mm.BodyHtmlText = htmlDoc.DocumentNode.OuterHtml;
        }

        static void MakeHtmlSafe(MailMessage mm)
        {
            var p = new Processor();
            p.Dom.OuterHtml = mm.BodyHtmlText;
            mm.BodyHtmlText = p.Dom.ProcessToString(RuleSet.GetSafeHtmlRules(), null);
        }

        void LoadMailFromUri()
        {
            var uri = Intent.Data;

            if (uri == null && Intent.ClipData != null && Intent.ClipData.ItemCount > 0)
                uri = Intent.ClipData.GetItemAt(0).Uri;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(this, Resource.String.loading_mail, Resource.String.please_wait);

            Task.Run(async () =>
                {
                    var auth = AuthenticatorFactory.Create();
                    if (!await auth.IsAuthenticatedAsync())
                        throw new MailViewerException("You need to log in to MARK5 before you can use mail viewer.");

                    if (uri == null)
                        throw new MailViewerException("File could not be loaded.");

                    string name;
                    long size;

                    using (var cursor = ContentResolver.Query(uri, null, null, null, null, null))
                    {
                        if (cursor == null)
                            throw new MailViewerException("File could not be loaded.");

                        cursor.MoveToFirst();

                        name = cursor.GetString(cursor.GetColumnIndex(OpenableColumns.DisplayName));
                        size = cursor.GetLong(cursor.GetColumnIndex(OpenableColumns.Size));
                    }

                    if (size > MaxSize)
                    {
                        CommonConfig.Logger.Error($"Attempted to open file that is too large. Size {size} bytes.");

                        throw new MailViewerException("File too large.");
                    }

                    if (name.EndsWith(".eml", StringComparison.CurrentCultureIgnoreCase))
                    {
                        byte[] bytes;
                        using (var stream = ContentResolver.OpenInputStream(uri))
                        {
                            bytes = ReadToEnd(stream);
                        }

                        try
                        {
                            var mm = new MailMessage
                            {
                                ThrowExceptions = true
                            };
                            mm.LoadMessage(bytes);
                            bytes = null;
                            MakeHtmlSafe(mm);
                            InlineImages(mm);
                            return mm;
                        }
                        catch (MailBeeException ex)
                        {
                            throw new MailViewerException("File could not be loaded.", ex);
                        }
                    }

                    if (name.EndsWith(".msg", StringComparison.CurrentCultureIgnoreCase))
                        using (var inputStream = ContentResolver.OpenInputStream(uri))
                        {
                            using (var msgStream = new MemoryStream())
                            {
                                inputStream.CopyTo(msgStream);
                                inputStream.Dispose();

                                using (var emlStream = new MemoryStream())
                                {
                                    try
                                    {
                                        var msgConverter = new MsgConvert();
                                        msgConverter.MsgToEml(msgStream, emlStream);
                                        msgStream.Dispose();

                                        emlStream.Position = 0;

                                        var mm = new MailMessage
                                        {
                                            ThrowExceptions = true
                                        };
                                        mm.LoadMessage(emlStream.ToArray());
                                        emlStream.Dispose();
                                        MakeHtmlSafe(mm);
                                        InlineImages(mm);
                                        return mm;
                                    }
                                    catch (MailBeeException ex)
                                    {
                                        throw new MailViewerException("File could not be loaded.", ex);
                                    }
                                }
                            }
                        }

                    throw new MailViewerException("Unsupported file.");
                })
                .ContinueWith(async t =>
                    {
                        dismissAction();

                        if (t.IsFaulted)
                        {
                            var ex = t.Exception.InnerException;
                            mailMessage = null;

                            CommonConfig.Logger.Error(ex);

                            await Dialogs.ShowErrorDialogAsync(this, ex);

                            Finish();
                        }
                        else
                        {
                            mailMessage = t.Result;

                            RefreshView();
                        }
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        void RefreshView()
        {
            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                if (linearLayout.GetChildAt(i) is MailViewerView dv)
                {
                    dv.MailMessage = mailMessage;
                    dv.RefreshView();

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

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void AttachmentsView_AttachmentClicked(object sender, MailBee.Mime.Attachment att)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(this, Resource.String.opening_attachment, Resource.String.please_wait);

            try
            {
                var attFile = await CreateTempFile(att.FilenameOriginal, att.GetData());

                var uri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", attFile);
                var mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(att.FilenameOriginal));

                var openFileIntent = new Intent(Intent.ActionView);
                openFileIntent.SetDataAndType(uri, mimeType);
                openFileIntent.AddFlags(ActivityFlags.NewTask);
                openFileIntent.AddFlags(ActivityFlags.GrantReadUriPermission);

                var canOpen = PackageManager.QueryIntentActivities(openFileIntent, 0).Any();
                if (canOpen)
                    StartActivity(openFileIntent);
                else
                    await Dialogs.ShowConfirmDialogAsync(this, Resource.String.attachment_cannot_be_opened_title, Resource.String.attachment_cannot_be_opened_summary);

                dismissAction();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to view attachment [attachment={att}]", ex);

                dismissAction();

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void AttachmentsView_AttachmentLongClicked(object sender, MailBee.Mime.Attachment att)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(this, Resource.String.opening_attachment, Resource.String.please_wait);

            try
            {
                var attFile = await CreateTempFile(att.FilenameOriginal, att.GetData());

                var uri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", attFile);
                var mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(att.FilenameOriginal));

                ShareCompat.IntentBuilder.From(this).SetType(mimeType).SetStream(uri).StartChooser();

                dismissAction();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to view attachment [attachment={att}]", ex);

                dismissAction();

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        async Task<Java.IO.File> CreateTempFile(string filename, byte[] bytes)
        {
            var mailViewerCacheDir = new Java.IO.File(CacheDir, "mailviewer");
            if (!mailViewerCacheDir.Exists())
                mailViewerCacheDir.Mkdir();

            var specificDir = new Java.IO.File(mailViewerCacheDir, Guid.NewGuid().ToString());
            if (!specificDir.Exists())
                specificDir.Mkdir();

            var cacheFile = new Java.IO.File(specificDir, filename);
            cacheFile.CreateNewFile();

            using (var fos = new Java.IO.FileOutputStream(cacheFile))
            {
                await fos.WriteAsync(bytes);
            }

            return cacheFile;
        }
    }
}