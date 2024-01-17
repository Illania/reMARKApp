using System;
using Android.Content;
using Android.Views;
using AndroidX.AppCompat.Widget;
using reMark.Mobile.Droid.Ui.Common;
using reMark.Mobile.Droid.Utilities;
using Android.Graphics;
using Color = Android.Graphics.Color;

namespace reMark.Mobile.Droid.Ui.Views.AutoReplyViews
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

