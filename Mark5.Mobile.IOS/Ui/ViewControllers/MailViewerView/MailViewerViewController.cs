using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using HtmlAgilityPack;
using MailBee;
using MailBee.Html;
using MailBee.Mime;
using MailBee.Outlook;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Model.Exceptions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using Attachment = MailBee.Mime.Attachment;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView
{
    public class MailViewerViewController : AbstractWebViewController
    {
        const long MaxSize = 5 * 1024 * 1024; // 5MB

        readonly NSUrl url;

        UIBarButtonItem closeItem;
        UIBarButtonItem shareItem;

        UIStackView headerStackView;

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

            closeItem = new UIBarButtonItem
            {
                Title = Localization.GetString("close")
            };
            NavigationItem.SetLeftBarButtonItem(closeItem, false);

            shareItem = new UIBarButtonItem(UIBarButtonSystemItem.Action);
            NavigationItem.SetRightBarButtonItem(shareItem, false);


            headerStackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = 0f
            };

            headerStackView.AddArrangedSubview(new SubjectView());
            headerStackView.AddArrangedSubview(new RecipientsView(RecipientsView.Type.From));
            headerStackView.AddArrangedSubview(new RecipientsView(RecipientsView.Type.To));
            headerStackView.AddArrangedSubview(new RecipientsView(RecipientsView.Type.Cc));
            headerStackView.AddArrangedSubview(new RecipientsView(RecipientsView.Type.Bcc));
            headerStackView.AddArrangedSubview(new RecipientsView(RecipientsView.Type.ReplyTo));
            headerStackView.AddArrangedSubview(new DateReceivedView());
            headerStackView.AddArrangedSubview(new PriorityView());
            headerStackView.AddArrangedSubview(new AttachmentsView(this));

            SetHeaderView(headerStackView);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;
            }

            closeItem.Clicked += CloseItem_Clicked;
            shareItem.Clicked += ShareItem_Clicked;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (mailMessage == null)
                LoadFromUrl();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            closeItem.Clicked -= CloseItem_Clicked;
            shareItem.Clicked -= ShareItem_Clicked;
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            closeItem = null;
            shareItem = null;

            headerStackView.ArrangedSubviews.ForEach(v => v.RemoveFromSuperview());
            headerStackView.RemoveFromSuperview();
            headerStackView = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        void CloseItem_Clicked(object sender, EventArgs e) =>
            DismissViewController(true, null);

        void ShareItem_Clicked(object sender, EventArgs e)
        {
            StopLoading();
            // TODO
            //var avc = new UIActivityViewController(new NSObject[] { url }, null);
            //if (avc.PopoverPresentationController != null)
            //    avc.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);
            //PresentViewController(avc, true, null);
        }

        void LoadFromUrl()
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("please_wait___"));

            Task.Run(async () =>
            {
                var auth = AuthenticatorFactory.Create();
                if (!await auth.IsAuthenticatedAsync())
                    throw new MailViewerException("You need to log in to MARK5 before you can use mail viewer.");

                if (url == null)
                    throw new MailViewerException("File could not be loaded.");

                var result = url.TryGetResource(NSUrl.FileSizeKey, out NSObject sizeObject, out NSError _error);
                if (!result)
                    throw new MailViewerException(_error.ToString());

                var name = url.LastPathComponent;
                var size = int.Parse(sizeObject.ToString());

                if (size > MaxSize)
                {
                    CommonConfig.Logger.Error($"Attempted to open file that is too large. Size {size} bytes.");

                    throw new MailViewerException("File too large.");
                }

                if (name.EndsWith(".eml", StringComparison.CurrentCultureIgnoreCase))
                {
                    byte[] bytes;
                    using (var stream = new FileStream(url.Path, FileMode.Open, FileAccess.Read))
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
                        InlineImages(mm);
                        return mm;
                    }
                    catch (MailBeeException ex)
                    {
                        throw new MailViewerException("File could not be loaded.", ex);
                    }
                }

                if (name.EndsWith(".msg", StringComparison.CurrentCultureIgnoreCase))
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

                                    var mm = new MailMessage
                                    {
                                        ThrowExceptions = true
                                    };
                                    mm.LoadMessage(emlStream.ToArray());
                                    emlStream.Dispose();
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
            }).ContinueWith(async t =>
            {
                dismissAction();

                if (t.IsFaulted)
                {
                    var ex = t.Exception.InnerException;
                    mailMessage = null;

                    CommonConfig.Logger.Error(ex);

                    Dialogs.ShowErrorAlert(this, ex);
                }
                else
                {
                    mailMessage = t.Result;

                    await RefreshView();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        async Task RefreshView()
        {
            foreach (var sv in headerStackView.Subviews.OfType<MailViewerSubview>())
            {
                sv.MailMessage = mailMessage;
                sv.RefreshView();
                sv.UpdateVisibility();
            }

            if (mailMessage != null)
            {
                if (!string.IsNullOrWhiteSpace(mailMessage.BodyHtmlText))
                    await LoadHtmlString(mailMessage.BodyHtmlText, true, true, false);
                else if (!string.IsNullOrWhiteSpace(mailMessage.BodyPlainText))
                    LoadPlainString(mailMessage.BodyPlainText);
                else
                    LoadNoContentString();
            }
            else
                LoadEmpty();
        }

        public void OpenComposeDocumentView(string[] preconfiguredEmailAddresses)
        {
            var vc = new ComposeDocumentViewController
            {
                DocumentCreationModeFlag = DocumentCreationModeFlag.New,
                PreconfiguredEmailAddresses = new Dictionary<DocumentAddressType, string[]>
                {
                    { DocumentAddressType.To, preconfiguredEmailAddresses }
                }
            };

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        public async void OpenAttachment(Attachment attachment)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("please_wait___"));

            try
            {
                var filename = attachment.FilenameOriginal;
                if (string.IsNullOrEmpty(filename))
                    filename = attachment.Filename;

                var tempFile = await CreateTempFile(filename, attachment.GetData());

                if (CanOpen(tempFile))
                    PresentViewController(new NavigationController(new MailViewerViewController(tempFile), UIModalPresentationStyle.PageSheet), true, null);
                else
                {
                    var attachmentInteractionController = UIDocumentInteractionController.FromUrl(tempFile);
                    attachmentInteractionController.Delegate = new DocumentInteractionControllerDelegate(this);

                    var previewSuccessful = attachmentInteractionController.PresentPreview(true);

                    if (!previewSuccessful)
                    {
                        CommonConfig.Logger.Info("Failed to present preview for attachment - presenting open with instead");

                        var openInSuccessful = attachmentInteractionController.PresentOptionsMenu(View.Frame, View, true);
                        if (!openInSuccessful)
                        {
                            CommonConfig.Logger.Warning("Failed to present open in view - there is no app that can open this type of attachment installed");

                            await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("cannot_open_attachment_title"), Localization.GetString("cannot_open_attachment_content"));
                        }
                    }
                }

                dismissAction();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to view attachment [attachment={attachment}]", ex);

                dismissAction();

                await Dialogs.ShowErrorAlertAsync(this, ex);
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

        async Task<NSUrl> CreateTempFile(string filename, byte[] bytes)
        {
            var fm = NSFileManager.DefaultManager;

            var tempDir = fm.GetTemporaryDirectory();
            var specificDir = tempDir.Append("mailviewer", true).Append(Guid.NewGuid().ToString(), true);

            fm.CreateDirectory(specificDir, true, null, out NSError _error);

            if (_error != null)
                throw new MailViewerException(_error.ToString());

            var cacheFile = specificDir.Append(filename, false);

            using (var stream = new FileStream(cacheFile.Path, FileMode.OpenOrCreate))
                await stream.WriteAsync(bytes, 0, bytes.Length);

            return cacheFile;
        }
    }
}