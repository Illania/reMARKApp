using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.AddEditContactView
{
    public class AddEditContacViewController : AbstractViewController
    {
        public Contact Contact { get; set; }
        public ContactPreview ContactPreview { get; set; }
        public Folder Folder { get; set; }
        public int? FolderId { get; set; }
        public int? ContactId { get; set; }
        public ContactType ContactType { get; set; }
        public ContactCreationModeFlag CreationModeFlag { get; set; }
        public ContactPreview ParentContactPreview { get; set; }

        UIBarButtonItem editButton;

        #region UIViewControllerOverrides

        public override void LoadView()
        {
            base.LoadView();

            InitNavigationBar();
            InitView();
        }

        #endregion

        #region Init methods

        void InitNavigationBar()
        {
            editButton = new UIBarButtonItem();
            //editButton.Image = UIImage.FromBundle(Path.Combine("icons", "compose.png"));
            editButton.Enabled = false;
            NavigationItem.SetRightBarButtonItem(editButton, false);
        }

        void InitView()
        {

        }

        #endregion

    }
}
