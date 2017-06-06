using System;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public class ActionOnClickListener : Java.Lang.Object, View.IOnClickListener
    {
        readonly Action action;

        public ActionOnClickListener(Action action)
        {
            this.action = action;
        }

        public void OnClick(View v)
        {
            if (action != null)
                action();
        }
    }
}