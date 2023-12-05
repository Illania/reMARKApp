using System;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using System.Threading.Tasks;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class DocumentPickerListViewController : DocumentsListViewController
    {
        readonly TaskCompletionSource<Document> tcs = new TaskCompletionSource<Document>();
        public Task<Document> Result => tcs.Task;

        public override async void DocumentSelected(DocumentPreview documentPreview)
        {
            try
            {
                var doc = await Managers.DocumentsManager.GetDocumentAsync(Folder, documentPreview.Id);
                tcs.SetResult(doc);
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
