using System;
using System.Collections.Generic;
using System.IO;
using Contacts;
using Foundation;
using Mark5.Mobile.IOS.Common.ShareExtension;
using Social;
using UIKit;
using ContentType = MobileCoreServices.UTType;

namespace reMARK.Mobile.IOS.Extensions.Share
{
    public partial class ShareViewController : SLComposeServiceViewController
    {

        protected ShareViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();
            List<string> attachmentList = new();


            var attachments = (ExtensionContext?.InputItems[0])?.Attachments;


            foreach (var provider in attachments)
            {
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

                                var sharedFolder = containerUrl.Append($"Library", true).Append("Caches", true);

                                if (!NSFileManager.DefaultManager.FileExists(sharedFolder.AbsoluteString))
                                    NSFileManager.DefaultManager.CreateDirectory(sharedFolder.AbsoluteString, true, new NSFileAttributes());

                                var sharedFileUrl = sharedFolder.Append("contacts", false).AppendPathExtension("vcf");

                                nsData.Save(sharedFileUrl.Path, true, out var error);

                                if (error != null)
                                    ShareExtensionErrorLogger.WriteToLog(error.GetDebugDescription(), new NSErrorException(error));

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
                //for images and files
                if (provider.HasItemConformingTo(ContentType.Image) || provider.HasItemConformingTo(ContentType.URL))
                {
                        var contentType = provider.HasItemConformingTo(ContentType.Image) ? ContentType.Image : ContentType.URL;

                        var result = await provider.LoadItemAsync(contentType, null);

                        if (result is NSObject dataObject)
                        {

                            var fileUrl = (NSUrl)dataObject;
                            var fileData = NSData.FromUrl(fileUrl);
                            var fileName = fileUrl.LastPathComponent;
                         
                            using (var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(ShareExtensionContainerUtilities.AppGroupId))
                            {

                                var sharedFolder = containerUrl.Append($"Library", true).Append("Caches", true);

                                if (!NSFileManager.DefaultManager.FileExists(sharedFolder.AbsoluteString))
                                    NSFileManager.DefaultManager.CreateDirectory(sharedFolder.AbsoluteString, true, new NSFileAttributes());
                               
                                var sharedFileUrl = sharedFolder.Append(fileName, false);

                                fileData.Save(sharedFileUrl.Path, true, out var error);

                                if (error != null)
                                    ShareExtensionErrorLogger.WriteToLog(error.GetDebugDescription(), new NSErrorException(error));
                                  
                                attachmentList.Add(sharedFileUrl.Path);
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

                                var sharedFolder = containerUrl.Append($"Library", true).Append("Caches", true);

                                if (!NSFileManager.DefaultManager.FileExists(sharedFolder.AbsoluteString))
                                    NSFileManager.DefaultManager.CreateDirectory(sharedFolder.AbsoluteString, true, new NSFileAttributes());

                                var sharedFileUrl = sharedFolder.Append("text", false).AppendPathExtension("txt");

                                File.WriteAllText(sharedFileUrl.Path, mutableString.ToString());

                                UIApplication.SharedApplication.OpenUrl(NSUrl.FromString($"remark.share.text://{sharedFileUrl.Path}"));
                                ExtensionContext.CompleteRequest(new NSExtensionItem[0], null);
                                return;
                            }
                        }
                    }
                }
            
               var pathString = string.Join(';', attachmentList);
               UIApplication.SharedApplication.OpenUrl(NSUrl.FromString($"remark.share.url://{pathString}"));
               ExtensionContext.CompleteRequest(new NSExtensionItem[0], null);

           
        }

        public override bool IsContentValid()
        {
            // Do validation of contentText and/or NSExtensionContext attachments here
            return true;
        }

        public override async void DidSelectPost()
        {
            ExtensionContext.CompleteRequest(new NSExtensionItem[0], null);
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
