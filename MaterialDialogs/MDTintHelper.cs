using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.Annotation;
using Android.Support.V4.Content;
using Android.Util;
using Android.Widget;
using MaterialDialogs.Utils;

namespace MaterialDialogs
{
    static class MDTintHelper
    {

        public static void SetTint([NonNull] RadioButton radioButton, [NonNull] ColorStateList colors)
        {
            radioButton.ButtonTintList = colors;
        }

        public static void SetTint([NonNull] RadioButton radioButton, [ColorInt] int color)
        {
            var disabledColor = DialogUtils.GetDisabledColor(radioButton.Context);
            var stateList = new ColorStateList(new int[][] {
                new [] {Android.Resource.Attribute.StateEnabled,-Android.Resource.Attribute.StateChecked},
                new [] {Android.Resource.Attribute.StateEnabled,Android.Resource.Attribute.StateChecked},
                new [] {-Android.Resource.Attribute.StateEnabled,-Android.Resource.Attribute.StateChecked},
                new [] {-Android.Resource.Attribute.StateEnabled,Android.Resource.Attribute.StateChecked}
            }, new int[] {
                DialogUtils.ResolveColor(radioButton.Context, Resource.Attribute.colorControlNormal),
                color,
                disabledColor,
                disabledColor
            });
           SetTint(radioButton, stateList);
        }

        public static void SetTint([NonNull] CheckBox checkBox, [NonNull] ColorStateList colors)
        {
            checkBox.ButtonTintList = colors;
        }

        public static void SetTint([NonNull] CheckBox checkBox, [ColorInt] int color)
        {
            var disabledColor = DialogUtils.GetDisabledColor(checkBox.Context);
            var sl = new ColorStateList(
                    new int[][] {
                        new [] {Android.Resource.Attribute.StateEnabled,-Android.Resource.Attribute.StateChecked},
                        new [] {Android.Resource.Attribute.StateEnabled,Android.Resource.Attribute.StateChecked},
                        new [] {-Android.Resource.Attribute.StateEnabled,-Android.Resource.Attribute.StateChecked},
                        new [] {-Android.Resource.Attribute.StateEnabled,Android.Resource.Attribute.StateChecked}
                    },
                    new int[] {
                        DialogUtils.ResolveColor(checkBox.Context, Resource.Attribute.colorControlNormal),
                        color,
                        disabledColor,
                        disabledColor
                    });
            SetTint(checkBox, sl);
        }

        public static void SetTint([NonNull] SeekBar seekBar, [ColorInt] int color)
        {
            var stateList = ColorStateList.ValueOf(new Color(color));
            seekBar.ThumbTintList = stateList;
            seekBar.ProgressTintList = stateList;
        }

        public static void SetTint([NonNull] ProgressBar progressBar, [ColorInt] int color)
        {
            SetTint(progressBar, color, false);
        }

        public static void SetTint([NonNull] ProgressBar progressBar, [ColorInt] int color, bool skipIndeterminate)
        {
            var stateList = ColorStateList.ValueOf(new Color(color));
            progressBar.ProgressTintList = stateList;
            progressBar.SecondaryProgressTintList = stateList;
            if (!skipIndeterminate)
                progressBar.IndeterminateTintList = stateList;
        }

        public static ColorStateList CreateEditTextColorStateList([NonNull] Context context, [ColorInt] int color)
        {
            int[][] states = new int[3][];
            int[] colors = new int[3];
            var i = 0;
            states[0] = new [] { -Android.Resource.Attribute.StateEnabled };
            colors[0] = DialogUtils.ResolveColor(context, Android.Resource.Attribute.ColorControlNormal);
            i++;
            states[i] = new [] { -Android.Resource.Attribute.StatePressed, -Android.Resource.Attribute.StateFocused };
            colors[i] = DialogUtils.ResolveColor(context, Android.Resource.Attribute.ColorControlNormal);
            i++;
            states[i] = new int[] { };
            colors[i] = color;
            return new ColorStateList(states, colors);
        }

        public static void SetTint([NonNull] EditText editText, [ColorInt] int color)
        {
            editText.BackgroundTintList = CreateEditTextColorStateList(editText.Context, color);

            SetCursorTint(editText, color);
        }

        public static void SetCursorTint([NonNull] EditText editText, [ColorInt] int color)
        {
            try
            {
                var fCursorDrawableRes = editText.Class.GetDeclaredField("mCursorDrawableRes");
                fCursorDrawableRes.Accessible = true;
                var mCursorDrawableRes = fCursorDrawableRes.GetInt(editText);

                var fEditor = editText.Class.GetDeclaredField("mEditor");
                fEditor.Accessible = true;
                var editor = fEditor.Get(editText);

                var editorClass = editor.Class;
                var fCursorDrawable = editorClass.GetDeclaredField("mCursorDrawable");
                fCursorDrawable.Accessible = true;

                var drawables = new Drawable[2];
                drawables[0] = ContextCompat.GetDrawable(editText.Context, mCursorDrawableRes);
                drawables[1] = ContextCompat.GetDrawable(editText.Context, mCursorDrawableRes);
                drawables[0].SetColorFilter(new Color(color), PorterDuff.Mode.SrcIn);
                drawables[1].SetColorFilter(new Color(color), PorterDuff.Mode.SrcIn);

                fCursorDrawable.Set(editor, drawables);

            }
            catch (NoSuchPropertyException e1)
            {
                e1.PrintStackTrace();
            }
            catch (Java.Lang.Exception e2)
            {
                e2.PrintStackTrace();
            }
        }
    }
}