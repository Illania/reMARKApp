//
// Project: Mark5.Mobile.IOS
// File: MailViewerActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Support.V7.Widget;
using MailBee.Mime;
using MailBee.Outlook;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;

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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            var uri = Intent.Data;

            try
            {
                var cursor = ContentResolver.Query(uri, null, null, null, null, null);
                cursor.MoveToFirst();

                var name = cursor.GetString(cursor.GetColumnIndex(OpenableColumns.DisplayName));
                var size = cursor.GetLong(cursor.GetColumnIndex(OpenableColumns.Size));

                if (size > MaxSize)
                    throw new Exception("Too large.");

                byte[] bytes;
                using (var stream = ContentResolver.OpenInputStream(uri))
                {
                    bytes = ReadToEnd(stream);
                }

                if (name.EndsWith(".msg", StringComparison.CurrentCultureIgnoreCase))
                {
                    using (var stream = new MemoryStream())
                    {
                        var msgConverter = new MsgConvert();
                        msgConverter.MsgToEml(new MemoryStream(bytes), stream);

                        stream.Position = 0;
                        bytes = ReadToEnd(stream);
                    }
                }

                var eml = new MailMessage();
                eml.LoadMessage(bytes);

                CommonConfig.Logger.Info(eml.Subject);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);

                Dialogs.ShowErrorDialog(this, ex);
            }
        }

        static byte[] ReadToEnd(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
