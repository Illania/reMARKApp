using System;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;

namespace MaterialDialogs
{
    class OnGlobalLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
    {
        readonly Action action;
        readonly RecyclerView recyclerView;

        public OnGlobalLayoutListener(RecyclerView recyclerView, Action action)
        {
            this.recyclerView = recyclerView;
            this.action = action;
        }

        public void OnGlobalLayout()
        {
            recyclerView.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);
            action();
        }
    }

    class TextWatcher : Java.Lang.Object, ITextWatcher
    {
        readonly Action<string> action;

        public TextWatcher(Action<string> action)
        {
            this.action = action;
        }

        public void AfterTextChanged(IEditable s)
        {
            // Nothing to do
        }

        public void BeforeTextChanged(Java.Lang.ICharSequence s, int start, int count, int after)
        {
            // Nothing to do
        }

        public void OnTextChanged(Java.Lang.ICharSequence s, int start, int before, int count)
        {
            action(s.ToString());
        }
    }

    class WebViewOnPreDrawListener : Java.Lang.Object, ViewTreeObserver.IOnPreDrawListener
    {
        readonly Action action;
        readonly ViewTreeObserver viewTreeObserver;

        public WebViewOnPreDrawListener(Action action, ViewTreeObserver viewTreeObserver)
        {
            this.action = action;
            this.viewTreeObserver = viewTreeObserver;
        }

        public bool OnPreDraw()
        {
            action();
            viewTreeObserver.RemoveOnPreDrawListener(this);
            return true;
        }
    }

    class RecyclerViewOnScrollListener : RecyclerView.OnScrollListener
    {
        readonly Action action;

        public RecyclerViewOnScrollListener(Action action)
        {
            this.action = action;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);
            action();
        }
    }

    class OnScrollChangedListener : Java.Lang.Object, ViewTreeObserver.IOnScrollChangedListener
    {
        readonly Action action;

        public OnScrollChangedListener(Action action)
        {
            this.action = action;
        }

        public void OnScrollChanged()
        {
            action();
        }
    }
}