using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.Annotation;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views.InputMethods;

namespace MaterialDialogs.Utils
{
    static class DialogUtils
    {

        [ColorInt]
        public static int GetDisabledColor(Context context)
        {
            var primaryColor = ResolveColor(context, Android.Resource.Attribute.TextColorPrimary);
            var disabledColor = IsColorDark(primaryColor) ? Color.Black : Color.White;
            return AdjustAlpha(disabledColor, 0.3f);
        }

        [ColorInt]
        public static int AdjustAlpha([ColorInt] int color, float factor)
        {
            var alpha = (int)Math.Round(Color.GetAlphaComponent(color) * factor);
            var red = Color.GetRedComponent(color);
            var green = Color.GetGreenComponent(color);
            var blue = Color.GetBlueComponent(color);

            return Color.Argb(alpha, red, green, blue);
        }

        [ColorInt]
        public static int ResolveColor(Context context, [AttrRes] int attr)
        {
            return ResolveColor(context, attr, 0);
        }

        [ColorInt]
        public static int ResolveColor(Context context, [AttrRes] int attr, int fallback)
        {
            var a = context.Theme.ObtainStyledAttributes(new int[] { attr });
            try
            {
                return a.GetColor(0, fallback);
            }
            finally
            {
                a.Recycle();
            }
        }

        //Tries to resolve colorAttr attribute
        public static ColorStateList ResolveActionTextColorStateList(Context context, [AttrRes] int colorAttr, ColorStateList fallback)
        {
            var a = context.Theme.ObtainStyledAttributes(new int[] { colorAttr });
            try
            {
                var value = a.PeekValue(0);
                if (value == null)
                    return fallback;

                if (value.Type >= DataType.LastColorInt && value.Type <= DataType.FirstColorInt)
                    return GetActionTextStateList(context, value.Data);

                var stateList = a.GetColorStateList(0);
                if (stateList != null)
                    return stateList;

                return fallback;
            }
            finally
            {
                a.Recycle();
            }
        }

        public static ColorStateList GetActionTextColorStateList(Context context, [ColorRes] int colorId)
        {
            var v = new TypedValue();
            context.Resources.GetValue(colorId, v, true);
            if (v.Type >= DataType.FirstColorInt && v.Type <= DataType.LastColorInt)
                return GetActionTextStateList(context, v.Data);

            return context.GetColorStateList(colorId);
        }

        [ColorInt]
        public static int GetColor(Context context, [ColorRes] int colorId)
        {
            int color = 0;

            try
            {
                color = ContextCompat.GetColor(context, colorId);
            }
            catch (Exception ex)
            {
                Console.Write(ex.InnerException.ToString());
            }

            return color;
        }

        public static string ResolveString(Context context, [AttrRes] int attr)
        {
            var v = new TypedValue();
            var output = context.Theme.ResolveAttribute(attr, v, false);
            if (v.String == null)
                return null;

            return v.String.ToString();
        }

        public static GravityEnum ResolveGravityEnum(Context context, [AttrRes] int attr, GravityEnum defaultGravity)
        {
            var a = context.Theme.ObtainStyledAttributes(new int[] { attr });
            try
            {
                switch (a.GetInt(0, (int)defaultGravity))
                {
                    case 1:
                        return GravityEnum.Center;
                    case 2:
                        return GravityEnum.End;
                    default:
                        return GravityEnum.Start;
                }
            }
            finally
            {
                a.Recycle();
            }
        }

        public static Drawable ResolveDrawable(Context context, [AttrRes] int attr)
        {
            return ResolveDrawable(context, attr, null);
        }

        public static Drawable ResolveDrawable(Context context, [AttrRes] int attr, Drawable fallback)
        {
            var a = context.Theme.ObtainStyledAttributes(new int[] { attr });
            try
            {
                var d = a.GetDrawable(0);
                if (d == null && fallback != null)
                    d = fallback;

                return d;
            }
            finally
            {
                a.Recycle();
            }
        }

        public static int ResolveDimension(Context context, [AttrRes] int attr)
        {
            return ResolveDimension(context, attr, -1);
        }

        public static int ResolveDimension(Context context, [AttrRes] int attr, int fallback)
        {
            var a = context.Theme.ObtainStyledAttributes(new int[] { attr });
            try
            {
                return a.GetDimensionPixelSize(0, fallback);
            }
            finally
            {
                a.Recycle();
            }
        }

        public static bool ResolveBoolean(Context context, [AttrRes] int attr, bool fallback)
        {
            var a = context.Theme.ObtainStyledAttributes(new int[] { attr });
            try
            {
                return a.GetBoolean(0, fallback);
            }
            finally
            {
                a.Recycle();
            }
        }

        public static bool ResolveBoolean(Context context, [AttrRes] int attr)
        {
            return ResolveBoolean(context, attr, false);
        }

        public static bool IsColorDark([ColorInt] int color)
        {
            var darkness = 1 - (0.299 * Color.GetRedComponent(color) + 0.587 *
                            Color.GetGreenComponent(color) + 0.114 *
                            Color.GetBlueComponent(color)) / 255;
            return darkness >= 0.5;
        }

        public static void ShowKeyboard([NonNull] MaterialDialog dialog,
                                        [NonNull] MaterialDialog.Builder builder)
        {
            if (dialog.GetInputEditText() == null)
                return;

            dialog.GetInputEditText()
                  .Post(new Java.Lang.Runnable(() =>
            {
                dialog.GetInputEditText().RequestFocus();
                var imm = (InputMethodManager)builder.context.GetSystemService(Context.InputMethodService);
                if (imm != null)
                    imm.ShowSoftInput(dialog.GetInputEditText(), ShowFlags.Implicit);
            }));
        }

        public static void HideKeyboard([NonNull] MaterialDialog dialog,
                                        [NonNull] MaterialDialog.Builder builder)
        {
            if (dialog.GetInputEditText() == null)
                return;

            var imm = (InputMethodManager)builder.context.GetSystemService(Context.InputMethodService);
            if (imm != null)
            {
                var currentFocus = dialog.CurrentFocus;
                var windowToken = currentFocus != null ? currentFocus.WindowToken : dialog.View.WindowToken;
                if (windowToken != null)
                    imm.HideSoftInputFromWindow(windowToken, 0);
            }
        }

        public static ColorStateList GetActionTextStateList(Context context, int newPrimaryColor)
        {
            var fallBackButtonColor = ResolveColor(context, Android.Resource.Attribute.TextColorPrimary);
            if (newPrimaryColor == 0)
                newPrimaryColor = fallBackButtonColor;

            int[][] states =
                {
                    new int[] {-Android.Resource.Attribute.StateEnabled}, // disabled
                    new int[] {} // enabled
				};
            int[] colors = { AdjustAlpha(newPrimaryColor, 0.4f), newPrimaryColor };
            return new ColorStateList(states, colors);
        }

        public static int[] GetColorArray([NonNull] Context context, [ArrayRes] int array)
        {
            if (array == 0)
                return null;

            var ta = context.Resources.ObtainTypedArray(array);
            var colors = new int[ta.Length()];
            for (int i = 0; i < ta.Length(); i++)
            {
                colors[i] = ta.GetColor(i, 0);
            }
            ta.Recycle();
            return colors;
        }
    }
}