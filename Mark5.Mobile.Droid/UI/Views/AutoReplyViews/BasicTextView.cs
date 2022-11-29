using System;
using Android.Content;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;
using Android.Graphics;

namespace Mark5.Mobile.Droid.Ui.Views.AutoReplyViews
{
    public class BasicTextView : AppCompatTextView
    {
        public BasicTextView(Context context) : base(context)
        {
            var verticalPadding = Conversion.ConvertDpToPixels(4);

            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = (GravityFlags)(int)GravityFlags.CenterVertical
            };
            SetPadding(0, verticalPadding, 0, verticalPadding);
            SetBackgroundColor(Color.Transparent);
            this.SetTextAppearanceCompat(context, Resource.Style.editAppointmentText);
        }
    }
}

