using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.IOS.Ui.ViewControllers
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
