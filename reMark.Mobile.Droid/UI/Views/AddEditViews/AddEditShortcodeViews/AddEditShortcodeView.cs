using Android.Content;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Droid.Ui.Views.AddEditViews;

namespace reMark.Mobile.Droid.Ui.Views.AddEditShortcodeViews
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
