using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class FormattingView : LinearLayoutCompat
    {
        readonly int padding;

        public event EventHandler BoldClicked = delegate { };
        public event EventHandler ItalicClicked = delegate { };
        public event EventHandler UnderlineClicked = delegate { };


        public FormattingView(Context context)
            : base(context)
        {
            var lp = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = GravityFlags.Bottom
            };
            LayoutParameters = lp;
            Orientation = Horizontal;

            SetBackgroundColor(Android.Graphics.Color.Beige);

            padding = Conversion.ConvertDpToPixels(1f);

            var boldButton = GetButton(context, "B");
            boldButton.Click += BoldButton_Click;

            var italicButton = GetButton(context, "I");
            italicButton.Click += ItalicButton_Click;

            var underlineButton = GetButton(context, "U");
            underlineButton.Click += UnderlineButton_Click;

            var closeButton = GetButton(context, "X");
            closeButton.Click += CloseButton_Click;

            AddView(boldButton);
            AddView(italicButton);
            AddView(underlineButton);
            AddView(closeButton);

            Hide();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void ItalicButton_Click(object sender, EventArgs e)
        {
            ItalicClicked(this, EventArgs.Empty);
        }

        private void UnderlineButton_Click(object sender, EventArgs e)
        {
            UnderlineClicked(this, EventArgs.Empty);
        }

        private void BoldButton_Click(object sender, EventArgs e)
        {
            BoldClicked(this, EventArgs.Empty);
        }

        public void Show()
        {
            Visibility = ViewStates.Visible;
        }

        public void Hide()
        {
            Visibility = ViewStates.Gone;
        }

        private AppCompatButton GetButton(Context context, string text)
        {
            var button = new AppCompatButton(context);
            button.Text = text;
            button.Gravity = GravityFlags.Center;
            button.SetPadding(padding, padding, padding, padding);

            return button;
        }
    }
}
