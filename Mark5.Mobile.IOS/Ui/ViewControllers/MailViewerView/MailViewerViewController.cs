//
// Project: Mark5.Mobile.IOS
// File: MailViewerViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using HtmlAgilityPack;
using MailBee;
using MailBee.Html;
using MailBee.Mime;
using MailBee.Outlook;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Model.Exceptions;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView
{

    public class MailViewerViewController : AbstractViewController
    {

        const long MaxSize = 5 * 1024 * 1024; // 5MB

        readonly NSUrl url;

        UIBarButtonItem closeItem;
        UIBarButtonItem shareItem;
        UIDocumentInteractionController attachmentInteractionController;

        MailMessage mailMessage;

        public static bool CanOpen(NSUrl url)
        {
            return url.Path.EndsWith(".eml", StringComparison.CurrentCultureIgnoreCase) || url.Path.EndsWith(".msg", StringComparison.CurrentCultureIgnoreCase);
        }

        public MailViewerViewController(NSUrl url)
        {
            this.url = url;
        }

        public override void LoadView()
        {
            base.LoadView();

            Global.LicenseKey = "MN110-C50DF2550CBE0D750DF4AF2E15D9-0B99";

            closeItem = new UIBarButtonItem();
            closeItem.Title = Localization.GetString("close");
            NavigationItem.SetLeftBarButtonItem(closeItem, false);

            shareItem = new UIBarButtonItem(UIBarButtonSystemItem.Action);
            NavigationItem.SetRightBarButtonItem(shareItem, false);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            closeItem.Clicked += CloseItem_Clicked;
            shareItem.Clicked += ShareItem_Clicked;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            LoadFromUrl();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            closeItem.Clicked -= CloseItem_Clicked;
            shareItem.Clicked -= ShareItem_Clicked;
        }

        void CloseItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);

        void ShareItem_Clicked(object sender, EventArgs e)
        {
            var avc = new UIActivityViewController(new NSObject[] { url }, null);
            if (avc.PopoverPresentationController != null)
                avc.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);
            PresentViewController(avc, true, null);
        }

        void LoadFromUrl()
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("please_wait___"));

            Task.Run(async () =>
            {
                var auth = AuthenticatorFactory.Create();
                if (!(await auth.IsAuthenticatedAsync()))
                    throw new MailViewerException("You need to log in to MARK5 before you can use mail viewer.");

                if (url == null)
                    throw new MailViewerException("File could not be loaded.");

                NSObject sizeObject;
                NSError _error;
                var result = url.TryGetResource(NSUrl.FileSizeKey, out sizeObject, out _error);

                if (!result)
                    throw new MailViewerException(_error.ToString());

                var name = url.LastPathComponent;
                var size = int.Parse(sizeObject.ToString());

                if (size > MaxSize)
                    throw new MailViewerException("File too large.");

                if (name.EndsWith(".eml", StringComparison.CurrentCultureIgnoreCase))
                {
                    byte[] bytes;
                    using (var stream = new FileStream(url.Path, FileMode.Open, FileAccess.Read))
                        bytes = ReadToEnd(stream);

                    try
                    {
                        var mm = new MailMessage();
                        mm.ThrowExceptions = true;
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
                {
                    using (var inputStream = new FileStream(url.Path, FileMode.Open, FileAccess.Read))
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

                                    var mm = new MailMessage();
                                    mm.ThrowExceptions = true;
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

                    DismissViewController(true, null);
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
            /*for (var i = 0; i < linearLayout.ChildCount; i++)
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
            linearLayout.RequestLayout();*/
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void AttachmentsView_AttachmentClicked(object sender, MailBee.Mime.Attachment att)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("please_wait___"));

            try
            {
                var attFile = await CreateTempFile(att.FilenameOriginal, att.GetData());

                if (CanOpen(attFile))
                {
                    PresentViewController(new NavigationController(new MailViewerViewController(attFile), UIModalPresentationStyle.PageSheet), true, null);
                }
                else
                {
                    attachmentInteractionController = UIDocumentInteractionController.FromUrl(attFile);
                    attachmentInteractionController.Delegate = new DocumentInteractionControllerDelegate(this);

                    var previewSuccessful = attachmentInteractionController.PresentPreview(true);

                    if (!previewSuccessful)
                    {
                        CommonConfig.Logger.Info("Failed to present preview for attachment. Presenting open with instead.");

                        var openInSuccessful = attachmentInteractionController.PresentOptionsMenu(View.Frame, View, true);
                        if (!openInSuccessful)
                        {
                            CommonConfig.Logger.Warning("Failed to present open in view - there is no app that can open this type of attachment installed.");

                            await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("cannot_open_attachment_title"), Localization.GetString("cannot_open_attachment_content"));
                        }
                    }
                }

                dismissAction();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to view attachment [attachment={att}]", ex);

                dismissAction();

                await Dialogs.ShowErrorDialogAsync(this, ex);
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

        async Task<NSUrl> CreateTempFile(string filename, byte[] bytes)
        {
            var fm = NSFileManager.DefaultManager;

            var tempDir = fm.GetTemporaryDirectory();
            var specificDir = tempDir.Append("mailviewer", true).Append(Guid.NewGuid().ToString(), true);

            NSError _error;
            fm.CreateDirectory(specificDir, true, null, out _error);

            if (_error != null)
                throw new MailViewerException(_error.ToString());

            var cacheFile = specificDir.Append(filename, false);

            using (var stream = new FileStream(cacheFile.Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096, true))
                await stream.WriteAsync(bytes, 0, bytes.Length);

            return cacheFile;
        }

        class DocumentInteractionControllerDelegate : UIDocumentInteractionControllerDelegate
        {

            readonly UIViewController parentController;

            public DocumentInteractionControllerDelegate(UIViewController parentController)
            {
                this.parentController = parentController;
            }

            public override UIViewController ViewControllerForPreview(UIDocumentInteractionController controller)
            {
                return parentController;
            }

            public override UIView ViewForPreview(UIDocumentInteractionController controller)
            {
                return parentController.View;
            }

            public override CGRect RectangleForPreview(UIDocumentInteractionController controller)
            {
                return parentController.View.Frame;
            }
        }
    }
}