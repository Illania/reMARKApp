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
        public event EventHandler BoldClicked = delegate { };
        public event EventHandler ItalicClicked = delegate { };
        public event EventHandler UnderlineClicked = delegate { };

        public FormattingView(Context context)
            : base(context)
        {
            LayoutParameters = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.MatchParent, FrameLayout.LayoutParams.WrapContent)
            {
                Gravity = GravityFlags.Bottom
            };
            Orientation = Vertical;

            SetBackgroundColor(Android.Graphics.Color.White);

            var separator = new View(context)
            {
                LayoutParameters = new LayoutParams(LayoutParams.MatchParent, Conversion.ConvertDpToPixels(1))
            };
            separator.SetBackgroundColor(Android.Graphics.Color.Gray);

            var internalLayout = new LinearLayoutCompat(Context)
            {
                Orientation = Horizontal,
                LayoutParameters = new LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent)
            };

            AddView(separator);
            AddView(internalLayout);

            var boldButton = GetButton(context, "B");
            boldButton.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
            boldButton.Click += BoldButton_Click;

            var italicButton = GetButton(context, "I");
            italicButton.SetTypeface(null, Android.Graphics.TypefaceStyle.Italic);
            italicButton.Click += ItalicButton_Click;

            var underlineButton = GetButton(context, "U");
            underlineButton.PaintFlags |= Android.Graphics.PaintFlags.UnderlineText;
            underlineButton.Click += UnderlineButton_Click;

            internalLayout.AddView(boldButton);
            internalLayout.AddView(italicButton);
            internalLayout.AddView(underlineButton);

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
            var button = new AppCompatButton(context)
            {
                Text = text,
                Gravity = GravityFlags.Center,
                LayoutParameters = new LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent, 1),
            };
            button.TextSize = 18;

            var minimumWidth = Conversion.ConvertDpToPixels(45);
            button.SetMinHeight(minimumWidth);
            button.SetMinWidth(minimumWidth);
            button.SetMinimumHeight(minimumWidth);
            button.SetMinimumWidth(minimumWidth);

            return button;
        }
    }
}
