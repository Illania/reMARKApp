using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class ExternalDocumentsListViewController : DocumentsListViewController
    {
        readonly TaskCompletionSource<List<AttachmentDescription>> tcs = new TaskCompletionSource<List<AttachmentDescription>>();
        public Task<List<AttachmentDescription>> Result => tcs.Task;

        public override async void DocumentSelected(DocumentPreview documentPreview)
        {
            try
            {
                var doc = await Managers.DocumentsManager.GetDocumentAsync(Folder, documentPreview.Id);
                var ads = doc.Attachments;

                foreach (var ad in ads)
                {
                    ad.DocumentId = doc.Id;
                }

                tcs.SetResult(doc.Attachments);
                DismissViewController(true, null);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading document failed [folder.name={Folder?.Name}, folder.id={Folder?.Id}, documentPreview.Id={documentPreview.Id}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        protected override void InitializeNavigationBar()
        {
            //Not needed.
        }
    }
}
