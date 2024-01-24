using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using AndroidX.AppCompat.Widget;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class SendAsPlainTextView : ComposeDocumentView
    {
        SwitchCompat ToggleButton;

        public event EventHandler<Android.Widget.CompoundButton.CheckedChangeEventArgs> Edited = delegate { };

        public SendAsPlainTextView(Context context) : base(context)
        {
            SetPadding(DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall,
                DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall);
            var label = new AppCompatTextView(context);
            label.Text = "Send as plain text";
            label.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent, 1)
            {
                Gravity = (GravityFlags)((int)GravityFlags.Left | (int)GravityFlags.CenterVertical),
            };

            AddView(label);

            ToggleButton = new SwitchCompat(context)
            {
                Gravity = GravityFlags.Right,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent,
                    ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = (GravityFlags)((int)GravityFlags.Right | (int)GravityFlags.CenterVertical),
                    RightMargin = 0,
                },
                SwitchPadding = 0,
            };
            ToggleButton.CheckedChange += (sender, e) => Edited(this,
                new Android.Widget.CompoundButton.CheckedChangeEventArgs(ToggleButton.Checked));

            AddView(ToggleButton);
        }

        public override Task RefreshView() => Task.CompletedTask;
        public override Task UpdateDocument() => Task.CompletedTask;
    }
}
