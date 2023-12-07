using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Contacts;
using CoreFoundation;
using Foundation;
using Mark5.Mobile.IOS.Common;
using Mark5.Mobile.IOS.Common.ShareExtension;
using ObjCRuntime;
using Social;
using UIKit;
using ContentType = MobileCoreServices.UTType;

namespace reMARK.Mobile.IOS.Extensions.Share
{
    public partial class ShareViewController : SLComposeServiceViewController
    {

        protected ShareViewController(IntPtr handle) : base(handle)
        {
            // Note: this constructor should not contain any initialization logic.
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }


        public override void ViewDidAppear(bool animated)
        {
            List<string> attachmentList = new();
            var alertView = UIAlertController.Create("Sharing to reMARK..", " ", UIAlertControllerStyle.Alert);

            PresentViewController(alertView, true, async () =>
            {
                var group = new DispatchGroup();
                var attachments = (ExtensionContext?.InputItems[0])?.Attachments;

                foreach (var provider in attachments)
                {
                    group.Enter();

                    if (provider.HasItemConformingTo(ContentType.VCard))
                    {
                        var result = await provider.LoadItemAsync(ContentType.VCard, null);

                        if (result is NSObject contactData)
                        {
                            try
                            {
                                var nsData = (NSData)contactData;
                                var contact = CNContactVCardSerialization.GetContactsFromData(nsData, out _);

                                using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(ShareExtensionContainerUtilities.AppGroupId))
                                {
                                    var sharedFileUrl = containerUrl.Append("contacts", false).AppendPathExtension("vcf");

                                    nsData.Save(sharedFileUrl, true, out var error);

                                    if (error != null)
                                        ShareExtensionErrorLogger.WriteToLog(error.GetDebugDescription(), new NSErrorException(error));
                                    else
                                        attachmentList.Add(sharedFileUrl.Path);
                                }
                            }
                            catch (Exception ex)
                            {
                                ShareExtensionErrorLogger.WriteToLog("Error while sharing contact vcard", ex);
                                continue;
                            }

                        }
                    }

                    if (provider.HasItemConformingTo(ContentType.Image) || provider.HasItemConformingTo(ContentType.FileURL))
                    {
                        var contentType = provider.HasItemConformingTo(ContentType.Image) ? ContentType.Image : ContentType.FileURL;

                        var data = await provider.LoadItemAsync(contentType, null);

                        if (data is NSObject dataObject)
                        {
                            var fileUrl = (NSUrl)dataObject;
                            var fileData = NSData.FromUrl(fileUrl);
                            var fileName = fileUrl.LastPathComponent;
                            var urlEncodedFileName = Uri.EscapeDataString(fileName);

                            using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(ShareExtensionContainerUtilities.AppGroupId))
                            {
                                var sharedFileUrl = containerUrl.Append(fileName, false);
                                fileData.Save(sharedFileUrl, true, out var error);
                                if (error != null)
                                    ShareExtensionErrorLogger.WriteToLog(error.GetDebugDescription(), new NSErrorException(error));
                                else
                                    attachmentList.Add(sharedFileUrl.AbsoluteString);
                            }
                        }
                    }

                    if (provider.HasItemConformingTo(ContentType.Text))
                    {
                        var result = await provider.LoadItemAsync(ContentType.Text, null);

                        if (result is NSObject dataObject)
                        {

                            var mutableString = (NSMutableString)dataObject;

                            using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(ShareExtensionContainerUtilities.AppGroupId))
                            {
                                var sharedFileUrl = containerUrl.Append("text", false).AppendPathExtension("txt");

                                File.WriteAllText(sharedFileUrl.Path, mutableString.ToString());

                                UIApplication.SharedApplication.OpenUrl(NSUrl.FromString($"remark.share.text://{sharedFileUrl.Path}"));
                            }
                        }
                    }

                    group.Leave();
                }

                group.Notify(DispatchQueue.MainQueue, () =>
                {
                    try
                    {
                        var pathString = string.Join(';', attachmentList);
                        var url = NSUrl.FromString($"remark.share.url://{pathString}");
                        UIApplication.SharedApplication.OpenUrl(url);
                    }
                    catch (Exception e)
                    {
                        alertView.Message = $"Error: {e.Message}";
                    }
                    DismissViewController(false, async () =>
                    {
                        await ExtensionContext.CompleteRequestAsync(null);
                    });
                });

            });
        }

        public override bool IsContentValid()
        {
            // Do validation of contentText and/or NSExtensionContext attachments here
            return true;
        }

        public override async void DidSelectPost()
        {
            await ExtensionContext.CompleteRequestAsync(new NSExtensionItem[0]);
            return;

        }

        public override SLComposeSheetConfigurationItem[] GetConfigurationItems()
        {
            // To add configuration options via table cells at the bottom of the sheet,
            // return an array of SLComposeSheetConfigurationItem here.
            return new SLComposeSheetConfigurationItem[0];
        }
    }
}
