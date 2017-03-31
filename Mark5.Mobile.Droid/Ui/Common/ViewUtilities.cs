//
// Project: 
// File: ViewUtilities.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Widget;

namespace Mark5.Mobile.Droid.Ui.Common
{

    public static class ViewUtilities
    {

        public static void SetTextAppearanceCompat(this TextView view, Context context, int resourceId)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                view.SetTextAppearance(context, resourceId);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                view.SetTextAppearance(resourceId);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
        }

        public static ColorStateList GetColorStateList(Context context, int resId)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                return context.Resources.GetColorStateList(resId, null);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                return context.Resources.GetColorStateList(resId);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}
