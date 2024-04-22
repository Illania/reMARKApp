using Contacts;
using CoreFoundation;
using reMark.Mobile.IOS.Common.ShareExtension;
using Social;
using UniformTypeIdentifiers;

namespace reMark.Mobile.IOS.Extensions.Share
{
    public partial class ShareViewController : SLComposeServiceViewController
    {
        public static bool IsRunningAtLeast(int major) => UIDevice.CurrentDevice.CheckSystemVersion(major, 0);
        private readonly List<string> _attachmentList = new();

        static readonly string VCardType =
            IsRunningAtLeast(14) ? UTTypes.VCard.Identifier : MobileCoreServices.UTType.VCard;

        static readonly string ImageType =
            IsRunningAtLeast(14) ? UTTypes.Image.Identifier : MobileCoreServices.UTType.Image;

        static readonly string FileUrlType =
            IsRunningAtLeast(14) ? UTTypes.FileUrl.Identifier : MobileCoreServices.UTType.FileURL;

        static readonly string TextType =
            IsRunningAtLeast(14) ? UTTypes.Text.Identifier : MobileCoreServices.UTType.Text;

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            TextView.Editable = false;
        }


        protected ShareViewController(IntPtr handle) : base(handle)
        {
            // Note: this constructor should not contain any initialization logic.
        }

        public override bool IsContentValid()
        {
            // Do validation of contentText and/or NSExtensionContext attachments here
            return true;
        }

        public override async void DidSelectPost()
        {

            // This is called after the user selects Post. Do the upload of contentText and/or NSExtensionContext attachments.
            var alert = UIAlertController.Create("Sharing to reMARK..", " ", UIAlertControllerStyle.Alert);
            PresentViewController(alert, true, () =>
            {
                var attachments = (ExtensionContext?.InputItems[0])?.Attachments;
                var contentType = string.Empty;

                if (attachments != null)
                {
                    foreach (var provider in attachments)
                    {
                        if (provider.HasItemConformingTo(ImageType) ||
                            provider.HasItemConformingTo(FileUrlType))
                        {
                            contentType = provider.HasItemConformingTo(ImageType)
                                ? ImageType
                                : FileUrlType;

                            HandleSharedFile(provider, contentType);
                        }
                        else if (provider.HasItemConformingTo(VCardType))
                        {
                            contentType = VCardType;
                            HandleSharedVCard(provider);
                        }
                        else if (provider.HasItemConformingTo(TextType))
                        {
                            contentType = TextType;
                            HandleSharedText(provider);
                        }
                    }
                }

                ShareToApp(contentType);
            });


            void HandleSharedFile(NSItemProvider provider, string contentType)
            {
                provider.LoadItem(contentType, null, async (result, error) =>
                {
                    if (result is not NSObject dataObject)
                        return;

                    var fileUrl = (NSUrl)dataObject;
                    var fileData = NSData.FromUrl(fileUrl);
                    var fileName = fileUrl.LastPathComponent;
                    if (fileName != null)
                    {
                        var urlEncodedFileName = Uri.EscapeDataString(fileName);

                        using var containerUrl =
                            NSFileManager.DefaultManager.GetContainerUrl(ShareExtensionContainerUtilities
                                .AppGroupId);
                        var sharedFileUrl = containerUrl.Append(urlEncodedFileName, false);
                        fileData.Save(sharedFileUrl, true, out var saveError);
                        if (saveError != null)
                            ShareExtensionErrorLogger.WriteToLog(error.GetDebugDescription(),
                                new NSErrorException(error));
                        else
                            _attachmentList.Add(urlEncodedFileName);
                    }
                });
            }

            void HandleSharedVCard(NSItemProvider provider)
            {
                provider.LoadItem(VCardType, null, async (result, error) =>
                {
                    if (result is not NSObject contactData)
                        return;

                    try
                    {
                        var nsContactData = (NSData)contactData;
                        var contact = CNContactVCardSerialization.GetContactsFromData(nsContactData, out _);

                        using var containerUrl = NSFileManager.DefaultManager.GetContainerUrl(
                            ShareExtensionContainerUtilities
                                .AppGroupId);
                        var sharedFileUrl = containerUrl.Append("contacts", false).AppendPathExtension("vcf");
                        nsContactData.Save(sharedFileUrl, true, out var saveError);

                        if (saveError != null)
                            ShareExtensionErrorLogger.WriteToLog(error.GetDebugDescription(),
                                new NSErrorException(error));
                        else
                            _attachmentList.Add(sharedFileUrl.LastPathComponent);
                    }
                    catch (Exception ex)
                    {
                        ShareExtensionErrorLogger.WriteToLog("Error while sharing contact vcard", ex);
                    }
                });
            }

            void HandleSharedText(NSItemProvider provider)
            {
                provider.LoadItem(TextType, null, async (result, error) =>
                {
                    if (result is not NSObject dataObject)
                        return;

                    var text = (NSMutableString)dataObject;

                    using var containerUrl =
                        NSFileManager.DefaultManager.GetContainerUrl(ShareExtensionContainerUtilities.AppGroupId);
                    var sharedFileUrl = containerUrl.Append("text", false).AppendPathExtension("txt");

                    await File.WriteAllTextAsync(sharedFileUrl.Path, text.ToString());
                    _attachmentList.Add(sharedFileUrl.LastPathComponent);
                });
            }
        }

        private void ShareToApp(string sharedFileType) =>
            DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, 2000000000), () =>
            {
                if (sharedFileType != string.Empty)
                {
                    var pathString = string.Join(';', _attachmentList);
                    var baseUrl = sharedFileType == TextType ? "remark.share.text://" : "remark.share.url://";
                    var url = NSUrl.FromString($"{baseUrl}{pathString}");

                    ShareExtensionErrorLogger.WriteToLog($"url in group.notify:{url?.AbsoluteString}");
                    UIApplication.SharedApplication.OpenUrl(url);
                }

                // Inform the host that we're done, so it un-blocks its UI.
                // Note: Alternatively you could call super's -didSelectPost, which will similarly complete the extension context.
                ExtensionContext?.CompleteRequest(Array.Empty<NSExtensionItem>(), null);
            });

        public override SLComposeSheetConfigurationItem[] GetConfigurationItems()
        {
            // To add configuration options via table cells at the bottom of the sheet,
            // return an array of SLComposeSheetConfigurationItem here.
            return new SLComposeSheetConfigurationItem[0];
        }
        

}
}
