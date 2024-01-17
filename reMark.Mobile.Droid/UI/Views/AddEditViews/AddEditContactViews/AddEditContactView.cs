using Android.Content;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Droid.Ui.Views.AddEditViews;
using Contact = reMark.Mobile.Common.Model.Contact;

namespace reMark.Mobile.Droid.Ui.Views.AddEditContactViews
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
