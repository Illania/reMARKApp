using Android.Content;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Views.AddEditViews;

namespace Mark5.Mobile.Droid.Ui.Views.AddEditShortcodeViews
{
    public abstract class AddEditShortcodeView : AddEditView
    {
        public Shortcode Shortcode { get; set; }
        public ShortcodePreview ShortcodePreview { get; set; }
        public ShortcodeCreationModeFlag CreationModeFlag { get; set; }

        protected AddEditShortcodeView(Context context)
            : base(context)
        {
        }
    }
}
