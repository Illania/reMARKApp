using System;
using Android.App;
using Android.Content;
using Android.Views;

namespace MaterialDialogs
{
    public abstract class DialogBase : Dialog, IDialogInterfaceOnShowListener
    {
        internal MDRootLayout View { get; set; }
        IDialogInterfaceOnShowListener showListener;

        protected DialogBase(Context context, int theme)
            : base(context, theme)
        {
        }

        public override View FindViewById(int id)
        {
            return View.FindViewById(id);
        }

        public override void SetOnShowListener(IDialogInterfaceOnShowListener listener)
        {
            showListener = listener;
        }

        public void SetOnShowListenerInternal()
        {
            base.SetOnShowListener(this);
        }

        public void SetViewInternal(View v)
        {
            base.SetContentView(v);
        }

        public virtual void OnShow(IDialogInterface dialog)
        {
            showListener?.OnShow(dialog);
        }

        public override void SetContentView(int layoutResID)
        {
            throw new InvalidOperationException("SetContentView() not supported in MaterialDialog.");
        }

        public override void SetContentView(View view)
        {
            throw new InvalidOperationException("SetContentView() not supported in MaterialDialog.");
        }

        public override void SetContentView(View view, ViewGroup.LayoutParams @params)
        {
            throw new InvalidOperationException("SetContentView() not supported in MaterialDialog.");
        }
    }
}