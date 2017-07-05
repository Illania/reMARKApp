using System;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class AbstractSimpleFieldView : AddEditContactView
    {
        AppCompatEditText contentEditText;

        public AbstractSimpleFieldView(Context context, int hintResourceId, bool floatingHint = false)
            : base(context)
        {
            if (floatingHint)
            {
                var layout = new TextInputLayout(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                };
                AddView(layout);

                contentEditText = new TextInputEditText(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                };
                contentEditText.SetHint(hintResourceId);
                layout.AddView(contentEditText);
            }
            else
            {
                contentEditText = new AppCompatEditText(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                };
                contentEditText.SetHint(hintResourceId);
                AddView(contentEditText);
            }

            contentEditText.TextChanged += ContentEditText_TextChanged;
        }

        void ContentEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {

        }

        public override void RefreshView()
        {
            throw new NotImplementedException();
        }
    }
}
