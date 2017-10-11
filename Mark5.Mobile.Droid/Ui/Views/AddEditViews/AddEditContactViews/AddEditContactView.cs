using Android.Content;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Views.AddEditViews;

namespace Mark5.Mobile.Droid.Ui.Views.AddEditContactViews
{
    public abstract class AddEditContactView : AddEditView
    {
        public Contact Contact { get; set; }
        public ContactPreview ContactPreview { get; set; }
        public ContactPreview ParentContactPreview { get; set; }
        public ContactCreationModeFlag CreationMode { get; set; }
        public bool ParentPreselected { get; set; }

        protected AddEditContactView(Context context)
            : base(context)
        {
        }
    }
}
