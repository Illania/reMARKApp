//
// Project: Mark5.Mobile.IOS
// File: MailViewerActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Support.V7.Widget;
using MailBee;
using MailBee.Mime;
using MailBee.Outlook;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Model.Exceptions;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.MailViewerViews;

namespace Mark5.Mobile.Droid.Ui.Activities
{

    [Activity(Label = "MARK5 Mail Viewer", ScreenOrientation = ScreenOrientation.Portrait, Exported = true)]
    [IntentFilter(new[] { Intent.ActionView, Intent.ActionSend },
                  Categories = new[] { Intent.CategoryDefault },
                  DataMimeTypes = new[] { "application/octet-stream", "message/rfc822" })]
    public class MailViewerActivity : BaseAppCompatActivity
    {

        const long MaxSize = 25 * 1024 * 1024; // 25MB

        Toolbar toolbar;
        LinearLayoutCompat linearLayout;

        MailMessage mailMessage;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Global.LicenseKey = "MN110-0FC7C778C79DC717C73F6688DFAB-8C0F";

            SetContentView(Resource.Layout.mailviewer_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            linearLayout = FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            linearLayout.AddView(new SubjectView(this));
            linearLayout.AddView(new Divider(this));
//            linearLayout.AddView(new RecipentsView(Context));
//            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new PriorityView(this));
            linearLayout.AddView(new Divider(this));
//            var av = new AttachmentsView(Context);
//            av.AttachmentClicked += AttachmentsView_AttachmentClicked;
//            av.AttachmentLongClicked += AttachmentsView_AttachmentLongClicked;
//            linearLayout.AddView(av);
            linearLayout.AddView(new ContentView(this));

            LoadMailFromUri();
        }

        void LoadMailFromUri()
        {
            var uri = Intent.Data;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(this, Resource.String.loading_mail, Resource.String.please_wait);

            Task.Run(() =>
            {
                if (uri == null)
                    throw new MailViewerException("File could not be loaded.");
                
                var cursor = ContentResolver.Query(uri, null, null, null, null, null);

                if (cursor == null)
                    throw new MailViewerException("File could not be loaded.");

                cursor.MoveToFirst();

                var name = cursor.GetString(cursor.GetColumnIndex(OpenableColumns.DisplayName));
                var size = cursor.GetLong(cursor.GetColumnIndex(OpenableColumns.Size));

                if (size > MaxSize)
                    throw new MailViewerException("File too large.");

                if (name.EndsWith(".eml", StringComparison.CurrentCultureIgnoreCase))
                {
                    byte[] bytes;
                    using (var stream = ContentResolver.OpenInputStream(uri))
                        bytes = ReadToEnd(stream);

                    try
                    {
                        var mm = new MailMessage();
                        mm.ThrowExceptions = true;
                        mm.LoadMessage(bytes);
                        return mm;
                    }
                    catch (MailBeeException ex)
                    {
                        throw new MailViewerException("File could not be loaded.", ex);
                    }
                }

                if (name.EndsWith(".msg", StringComparison.CurrentCultureIgnoreCase))
                {
                    using (var msgStream = ContentResolver.OpenInputStream(uri))
                    using (var emlStream = new MemoryStream())
                    {
                        try
                        {
                            var msgConverter = new MsgConvert();
                            msgConverter.MsgToEml(msgStream, emlStream);

                            emlStream.Position = 0;

                            var mm = new MailMessage();
                            mm.ThrowExceptions = true;
                            mm.LoadMessage(emlStream.ToArray());
                            return mm;
                        }
                        catch (MailBeeException ex)
                        {
                            throw new MailViewerException("File could not be loaded.", ex);
                        }
                    }
                }

                throw new MailViewerException("Unsupported file.");
            }).ContinueWith(async t =>
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
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        void RefreshView()
        {
            for (var i = 0; i<linearLayout.ChildCount; i++)
            {
                var dv = linearLayout.GetChildAt(i) as MailViewerView;
                if (dv != null)
                {
                    dv.MailMessage = mailMessage;
                    dv.RefreshView();

                    var d = linearLayout.GetChildAt(i + 1) as Divider;
                    if (d != null)
                    {
                        d.Visibility = dv.Visibility;
                        i++;
                    }
                }
            }

            linearLayout.Invalidate();
            linearLayout.RequestLayout();
        }

        static byte[] ReadToEnd(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, read);
                return ms.ToArray();
            }
        }
    }
}
