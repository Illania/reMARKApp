using Android.Views;
using Android.Widget;
using Android.Content;
using System.Collections;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public class CustomArrayAdapter : ArrayAdapter
    {
        public static ArrayAdapter Create(Context context, int textArrayResId, int textViewResId, int dropDownViewResId)
        {
            var strings = context.Resources.GetStringArray(textArrayResId);
            var adapter = new CustomArrayAdapter(context, textViewResId, strings, -1);
            adapter.SetDropDownViewResource(dropDownViewResId);
            return adapter;
        }

        public static ArrayAdapter CreateWithoutLeftPadding(Context context, int textArrayResId, int textViewResId, int dropDownViewResId)
        {
            var strings = context.Resources.GetStringArray(textArrayResId);
            var adapter = new CustomArrayAdapter(context, textViewResId, strings, 0);
            adapter.SetDropDownViewResource(dropDownViewResId);
            return adapter;
        }

        public static ArrayAdapter CreateWithLeftPaddingMatchingEditText(Context context, int textArrayResId, int textViewResId, int dropDownViewResId)
        {
            var strings = context.Resources.GetStringArray(textArrayResId);
            var adapter = new CustomArrayAdapter(context, textViewResId, strings, ConversionUtils.ConvertDpToPixels(4f));
            adapter.SetDropDownViewResource(dropDownViewResId);
            return adapter;
        }

        readonly int leftPadding;

        public CustomArrayAdapter(Context context, int textViewResId, IList objects, int leftPadding = -1)
            : base(context, textViewResId, objects)
        {
            this.leftPadding = leftPadding;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = base.GetView(position, convertView, parent);
            view.SetPadding(leftPadding < 0 ? view.PaddingLeft : leftPadding, view.PaddingTop, view.PaddingRight, view.PaddingBottom);
            return view;
        }
    }
}