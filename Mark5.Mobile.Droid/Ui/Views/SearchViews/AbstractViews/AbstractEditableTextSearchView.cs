using System;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public abstract class AbstractEditableTextSearchView<T> : AbstractSearchView<T>
    {
        readonly AppCompatTextView bottomTextView;
        readonly AppCompatEditText bottomEditText;
        readonly LinearLayoutCompat containerLayout;
        readonly LinearLayoutCompat cancelIconLayout;

        readonly string emptyText;

        protected AbstractEditableTextSearchView(Context context, int topTextResId, LinearLayoutCompat containerLayout = null)
            : base(context)
        {
            this.containerLayout = containerLayout;
            emptyText = context.GetString(Resource.String.search_editable_empty);

            Orientation = Horizontal;
            SetBackgroundColor(BackgroundColorNormalState);

            Clickable = true;
            Click += (sender, e) => PrepareViewsExpansion();

            var leftLayout = new LinearLayoutCompat(context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent)
                {
                    Weight = 1.0f
                }
            };

            var topTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.CenterHorizontal
            };
            topTextView.Text = context.GetString(topTextResId);
            topTextView.SetTextAppearanceCompat(context, TextStyleTopLineResourceId);
            leftLayout.AddView(topTextView);

            bottomEditText = LayoutInflater.From(context).Inflate(Resource.Layout.search_edit_text_layout, null).FindViewById<AppCompatEditText>(Resource.Id.search_edit_text);
            bottomEditText.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            bottomEditText.Gravity = GravityFlags.CenterHorizontal;
            bottomEditText.SetTextAppearanceCompat(context, TextStyleBottomLineResourceId);
            bottomEditText.SetBackgroundColor(Color.Transparent);
            bottomEditText.SetPadding(0, 0, 0, 0);
            bottomEditText.Hint = context.GetString(Resource.String.search_editable_empty);
            bottomEditText.SetHintTextColor(ViewUtilities.GetColorStateList(context, Resource.Drawable.search_edit_text_selector));
            bottomEditText.FocusChange += BottomEditText_FocusChange;
            bottomEditText.Visibility = ViewStates.Gone;

            leftLayout.AddView(bottomEditText);

            bottomTextView = new AppCompatTextView(context);
            bottomTextView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            bottomTextView.Gravity = GravityFlags.CenterHorizontal;
            bottomTextView.Text = emptyText;
            bottomTextView.SetTextAppearanceCompat(context, TextStyleBottomLineResourceId);
            bottomTextView.Visibility = ViewStates.Visible;
            bottomTextView.Ellipsize = TextUtils.TruncateAt.End;
            bottomTextView.SetLines(1);
            leftLayout.AddView(bottomTextView);

            cancelIconLayout = new LinearLayoutCompat(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = (int)GravityFlags.CenterVertical,
                }
            };

            var cancelIconSize = Conversion.ConvertDpToPixels(20f);
            var cancelIconView = new AppCompatImageView(context)
            {
                LayoutParameters = new LayoutParams(cancelIconSize, cancelIconSize)
                {
                    RightMargin = Conversion.ConvertDpToPixels(4),
                }
            };
            cancelIconView.SetImageResource(Resource.Drawable.cross);
            cancelIconView.SetColorFilter(Color.White);
            cancelIconLayout.AddView(cancelIconView);
            cancelIconLayout.Clickable = true;
            cancelIconLayout.Click += CancelIconLayout_Click;
            cancelIconLayout.Visibility = ViewStates.Gone;

            AddView(leftLayout);
            AddView(cancelIconLayout);
        }

        void BottomEditText_FocusChange(object sender, FocusChangeEventArgs e)
        {
            if (!e.HasFocus)
                ((InputMethodManager)Context.GetSystemService(Context.InputMethodService)).HideSoftInputFromWindow((sender as View).WindowToken, HideSoftInputFlags.None);
            else
                PrepareViewsExpansion();
        }

        void CancelIconLayout_Click(object sender, EventArgs e)
        {
            Collapse();

            if (containerLayout == null)
                return;

            for (var i = 0; i < containerLayout.ChildCount; i++)
            {
                if (containerLayout.GetChildAt(i) is AbstractEditableTextSearchView<T> view)
                    view.Visibility = ViewStates.Visible;
            }
        }

        void PrepareViewsExpansion()
        {
            bottomEditText.Visibility = ViewStates.Visible;
            bottomTextView.Visibility = ViewStates.Gone;

            if (bottomTextView.Text != emptyText)
                bottomEditText.Text = bottomTextView.Text;

            Expand();

            if (containerLayout == null)
                return;

            for (var i = 0; i < containerLayout.ChildCount; i++)
            {
                var view = containerLayout.GetChildAt(i) as AbstractEditableTextSearchView<T>;
                if (view != this)
                    view.Visibility = ViewStates.Gone;
            }
        }

        public void Expand()
        {
            bottomEditText.RequestFocus();
            bottomEditText.SetSelection(bottomEditText.Text.Length);

            ((InputMethodManager)Context.GetSystemService(Context.InputMethodService)).ShowSoftInput(bottomEditText, ShowFlags.Implicit);

            cancelIconLayout.Visibility = ViewStates.Visible;
        }

        public void Collapse()
        {
            cancelIconLayout.Visibility = ViewStates.Gone;

            bottomEditText.Visibility = ViewStates.Gone;
            bottomTextView.Visibility = ViewStates.Visible;

            bottomTextView.Text = string.IsNullOrWhiteSpace(bottomEditText.Text) ? emptyText : bottomEditText.Text;

            bottomEditText.ClearFocus();
        }

        public void SetText(string text)
        {
            bottomTextView.Text = string.IsNullOrWhiteSpace(text) ? emptyText : text;
            bottomEditText.Text = text;
        }

        public string GetText()
        {
            return bottomEditText.Text ?? string.Empty;
        }
    }
}