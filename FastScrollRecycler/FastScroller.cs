//
// Project: Mark5.Mobile.Droid
// File: FastScroller.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.View.Animation;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;

namespace FastScrollRecycler
{

    class FastScroller : Java.Lang.Object
    {

        const int DefaultAutoHideDelay = 1500;

        FastScrollRecyclerView recyclerView;
        FastScrollPopup popup;

        int thumbHeight;
        int width;

        Paint thumb;
        Paint track;

        Rect tmpRect = new Rect();
        Rect invalidateRect = new Rect();
        Rect invalidateTmpRect = new Rect();

        int touchInset;
        int touchOffset;

        Point thumbPosition = new Point(-1, -1);
        Point offset = new Point(0, 0);

        bool isDragging;

        Animator autoHideAnimator;
        bool animatingShow;
        int autoHideDelay = DefaultAutoHideDelay;
        bool autoHideEnabled = true;

        readonly Action hideAction;

        public FastScroller(Context ctx, FastScrollRecyclerView recyclerView, IAttributeSet attrs)
        {
            var resources = ctx.Resources;

            this.recyclerView = recyclerView;
            popup = new FastScrollPopup(resources, recyclerView);

            thumbHeight = Utils.ToPixels(resources, 48f);
            width = Utils.ToPixels(resources, 8f);

            touchInset = Utils.ToPixels(resources, -24f);

            thumb = new Paint(PaintFlags.AntiAlias);
            track = new Paint(PaintFlags.AntiAlias);

            var typedArray = ctx.Theme.ObtainStyledAttributes(attrs, Resource.Styleable.FastScrollRecyclerView, 0, 0);

            try
            {
                autoHideEnabled = typedArray.GetBoolean(Resource.Styleable.FastScrollRecyclerView_fastScrollAutoHide, true);
                autoHideDelay = typedArray.GetInteger(Resource.Styleable.FastScrollRecyclerView_fastScrollAutoHideDelay, DefaultAutoHideDelay);

                var color1f000000 = Color.Argb(0x1f, 0x00, 0x00, 0x00);
                var colorffffffff = Color.Argb(0xff, 0xff, 0xff, 0xff);

                var trackColor = typedArray.GetColor(Resource.Styleable.FastScrollRecyclerView_fastScrollTrackColor, color1f000000);
                var thumbColor = typedArray.GetColor(Resource.Styleable.FastScrollRecyclerView_fastScrollThumbColor, color1f000000);
                var popupBackgroundColor = typedArray.GetColor(Resource.Styleable.FastScrollRecyclerView_fastScrollPopupBackgroundColor, color1f000000);
                var popupTextColor = typedArray.GetColor(Resource.Styleable.FastScrollRecyclerView_fastScrollPopupTextColor, colorffffffff);
                var popupTextSize = typedArray.GetColor(Resource.Styleable.FastScrollRecyclerView_fastScrollPopupTextSize, Utils.ToPixels(resources, 56f));
                var popupBackgroundSize = typedArray.GetColor(Resource.Styleable.FastScrollRecyclerView_fastScrollPopupBackgroundSize, Utils.ToPixels(resources, 88f));
                var popupPosition = (FastScrollerPosition)typedArray.GetInteger(Resource.Styleable.FastScrollRecyclerView_fastScrollPopupPosition, (int)FastScrollerPosition.Adjacent);

                track.Color = trackColor;
                thumb.Color = thumbColor;
                popup.SetBackgroundColor(popupBackgroundColor);
                popup.SetTextColor(popupTextColor);
                popup.SetTextSize(popupTextSize);
                popup.SetBackgroundSize(popupBackgroundSize);
                popup.SetPopupPosition(popupPosition);
            }
            finally
            {
                typedArray?.Recycle();
            }

            hideAction = () =>
            {
                if (isDragging)
                    return;

                autoHideAnimator?.Cancel();

                autoHideAnimator = ObjectAnimator.OfInt(this, "offsetX", (Utils.IsRtl(resources) ? -1 : 1) * width);
                autoHideAnimator.SetInterpolator(new FastOutLinearInInterpolator());
                autoHideAnimator.SetDuration(200L);
                autoHideAnimator.Start();
            };

            recyclerView.AddOnScrollListener(new ActionOnScrollListener((rv, dx, dy) => Show()));

            if (autoHideEnabled)
                PostAutoHideDelayed();
        }

        public int GetThumbHeight()
        {
            return thumbHeight;
        }

        public int GetWidth()
        {
            return width;
        }

        public bool IsDragging()
        {
            return isDragging;
        }

        public void HandleTouchEvent(MotionEvent ev, int downX, int downY, int lastY, IOnFastScrollStateChangeListener stateChangedListener)
        {

            var config = ViewConfiguration.Get(recyclerView.Context);

            var action = ev.Action;
            var y = (int)ev.GetY();

            switch (action)
            {
                case MotionEventActions.Down:
                    if (IsNearPoint(downX, downY))
                        touchOffset = downY - thumbPosition.Y;
                    break;
                case MotionEventActions.Move:
                    if (!isDragging && IsNearPoint(downX, downY) && Math.Abs(y - downY) > config.ScaledTouchSlop)
                    {
                        recyclerView.Parent.RequestDisallowInterceptTouchEvent(true);
                        isDragging = true;
                        touchOffset += (lastY - downY);
                        popup.AnimateVisibility(true);
                        stateChangedListener?.OnFastScrollStart();
                    }
                    if (isDragging)
                    {
                        var top = 0;
                        var bottom = recyclerView.Height - thumbHeight;
                        var boundedY = (float)Math.Max(top, Math.Min(bottom, y - touchOffset));
                        var sectionName = recyclerView.ScrollToPositionAtProgress((boundedY - top) / (bottom - top));
                        popup.SetSectionName(sectionName);
                        popup.AnimateVisibility(!string.IsNullOrEmpty(sectionName));
                        recyclerView.Invalidate(popup.UpdateFastScrollerBounds(recyclerView, thumbPosition.Y));
                    }
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    touchOffset = 0;
                    if (isDragging)
                    {
                        isDragging = false;
                        popup.AnimateVisibility(false);
                        stateChangedListener?.OnFastScrollStop();
                    }
                    break;
            }

        }

        public void Draw(Canvas canvas)
        {
            if (thumbPosition.X < 0 || thumbPosition.Y < 0)
                return;

            canvas.DrawRect(thumbPosition.X + offset.X, thumbHeight / 2 + offset.Y, thumbPosition.X - offset.X + width, recyclerView.Height + offset.Y - thumbHeight / 2, track);
            canvas.DrawRect(thumbPosition.X + offset.X, thumbPosition.Y + offset.Y, thumbPosition.X + offset.X + width, thumbPosition.Y + offset.Y + thumbHeight, thumb);
            popup.Draw(canvas);
        }

        bool IsNearPoint(int x, int y)
        {
            tmpRect.Set(thumbPosition.X, thumbPosition.Y, thumbPosition.X + width,
                        thumbPosition.Y + thumbHeight);
            tmpRect.Inset(touchInset, touchInset);
            return tmpRect.Contains(x, y);
        }

        public void SetThumbPosition(int x, int y)
        {
            if (thumbPosition.X == x && thumbPosition.Y == y)
                return;

            invalidateRect.Set(thumbPosition.X + offset.X, offset.Y, thumbPosition.X + offset.X + width, recyclerView.Height + offset.Y);
            thumbPosition.Set(x, y);
            invalidateTmpRect.Set(thumbPosition.X + offset.X, offset.Y, thumbPosition.X + offset.X + width, recyclerView.Height + offset.Y);
            invalidateRect.Union(invalidateTmpRect);
            recyclerView.Invalidate(invalidateRect);
        }

        public void SetOffset(int x, int y)
        {
            if (offset.X == x && offset.Y == y)
                return;

            invalidateRect.Set(thumbPosition.X + offset.X, offset.Y, thumbPosition.X + offset.X + width, recyclerView.Height + offset.Y);
            offset.Set(x, y);
            invalidateTmpRect.Set(thumbPosition.X + offset.X, offset.Y, thumbPosition.X + offset.X + width, recyclerView.Height + offset.Y);
            invalidateRect.Union(invalidateTmpRect);
            recyclerView.Invalidate(invalidateRect);
        }

        public void SetOffsetX(int x) => SetOffset(x, offset.Y);

        public void SetOffsetY(int y) => SetOffset(offset.X, y);

        public void Show()
        {
            if (!animatingShow)
            {
                autoHideAnimator?.Cancel();
                autoHideAnimator = ObjectAnimator.OfInt(this, "offsetX", 0);
                autoHideAnimator.SetInterpolator(new LinearOutSlowInInterpolator());
                autoHideAnimator.AddListener(new ActionAnimatorListenerAdapter(() => animatingShow = false, () => animatingShow = false));
                animatingShow = true;
                autoHideAnimator.Start();
            }

            if (autoHideEnabled)
                PostAutoHideDelayed();
            else
                CancelAutoHide();
        }

        void PostAutoHideDelayed()
        {
            if (recyclerView == null)
                return;

            CancelAutoHide();
            recyclerView.PostDelayed(hideAction, autoHideDelay);
        }

        void CancelAutoHide() => recyclerView?.RemoveCallbacks(hideAction);

        public void SetThumbColor(Color color)
        {
            thumb.Color = color;
            recyclerView?.Invalidate(invalidateRect);
        }

        public void SetTrackColor(Color color)
        {
            track.Color = color;
            recyclerView?.Invalidate(invalidateRect);
        }

        public void SetPopupBackgroundColor(Color color) => popup.SetBackgroundColor(color);

        public void SetPopupTextColor(Color color) => popup.SetTextColor(color);

        public void SetPopupTextSize(int size) => popup.SetTextSize(size);

        public void SetPopupTypeface(Typeface typeface) => popup.SetTypeFace(typeface);

        public void SetAutoHideDelay(int autoHideDelay)
        {
            this.autoHideDelay = autoHideDelay;

            if (autoHideEnabled)
                PostAutoHideDelayed();
        }

        public void SetAutoHideEnabled(bool autoHideEnabled)
        {
            this.autoHideEnabled = autoHideEnabled;

            if (autoHideEnabled)
                PostAutoHideDelayed();
            else
                CancelAutoHide();
        }

        public void SetPopupPosition(FastScrollerPosition position) => popup.SetPopupPosition(position);

        class ActionOnScrollListener : RecyclerView.OnScrollListener
        {

            readonly Action<RecyclerView, int, int> action;

            public ActionOnScrollListener(Action<RecyclerView, int, int> action)
            {
                this.action = action;
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                base.OnScrolled(recyclerView, dx, dy);
                action(recyclerView, dx, dy);
            }
        }

        class ActionAnimatorListenerAdapter : AnimatorListenerAdapter
        {

            readonly Action actionCancel;
            readonly Action actionEnd;

            public ActionAnimatorListenerAdapter(Action actionCancel = null, Action actionEnd = null)
            {
                this.actionCancel = actionCancel;
                this.actionEnd = actionEnd;
            }

            public override void OnAnimationCancel(Animator animation)
            {
                base.OnAnimationCancel(animation);
                actionCancel();
            }

            public override void OnAnimationEnd(Animator animation)
            {
                base.OnAnimationEnd(animation);
                actionEnd();
            }
        }
    }
}
