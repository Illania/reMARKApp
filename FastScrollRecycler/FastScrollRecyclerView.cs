//
// Project: Mark5.Mobile.Droid
// File: FastScrollRecyclerView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;

namespace FastScrollRecycler
{

    public class FastScrollRecyclerView : RecyclerView, RecyclerView.IOnItemTouchListener
    {

        readonly FastScroller scrollbar;

        struct FastScrollPositionState
        {

            public int RowIndex { get; set; }

            public int RowTopOffset { get; set; }

            public int RowHeight { get; set; }
        }

        FastScrollPositionState scrollPositionState = new FastScrollPositionState();

        int downX;
        int downY;
        int lastY;

        IOnFastScrollStateChangeListener stateChangeListener;

        public FastScrollRecyclerView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            scrollbar = new FastScroller(context, this, attrs);
        }

        public FastScrollRecyclerView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            scrollbar = new FastScroller(context, this, attrs);
        }

        public int GetScrollBarWidth()
        {
            return scrollbar.GetWidth();
        }

        internal int GetScrollBarThumbHeight()
        {
            return scrollbar.GetThumbHeight();
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            AddOnItemTouchListener(this);
        }

        public bool OnInterceptTouchEvent(RecyclerView recyclerView, MotionEvent @event)
        {
            return HandleTouchEvent(@event);
        }


        public void OnTouchEvent(RecyclerView recyclerView, MotionEvent @event)
        {
            HandleTouchEvent(@event);
        }

        bool HandleTouchEvent(MotionEvent ev)
        {
            var action = ev.Action;
            var x = (int)ev.GetX();
            var y = (int)ev.GetY();
            switch (action)
            {
                case MotionEventActions.Down:
                    // Keep track of the down positions
                    downX = x;
                    downY = lastY = y;
                    scrollbar.HandleTouchEvent(ev, downX, downY, lastY, stateChangeListener);
                    break;
                case MotionEventActions.Move:
                    lastY = y;
                    scrollbar.HandleTouchEvent(ev, downX, downY, lastY, stateChangeListener);
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    scrollbar.HandleTouchEvent(ev, downX, downY, lastY, stateChangeListener);
                    break;
            }
            return scrollbar.IsDragging();
        }

        public void OnRequestDisallowInterceptTouchEvent(bool disallow)
        {
            // Nothing to do
        }

        int GetAvailableScrollHeight(int rowCount, int rowHeight, int yOffset)
        {
            var visibleHeight = Height;
            var scrollHeight = PaddingTop + yOffset + rowCount * rowHeight + PaddingBottom;
            return scrollHeight - visibleHeight;
        }

        int GetAvailableScrollBarHeight()
        {
            return Height - scrollbar.GetThumbHeight();
        }

        public override void OnDraw(Canvas c)
        {
            base.OnDraw(c);
            OnUpdateScrollbar();
            scrollbar.Draw(c);
        }

        void SynchronizeScrollBarThumbOffsetToViewScroll(FastScrollPositionState sps, int rowCount, int yOffset)
        {
            var availableScrollHeight = GetAvailableScrollHeight(rowCount, sps.RowHeight, yOffset);
            var availableScrollBarHeight = GetAvailableScrollBarHeight();

            if (availableScrollHeight <= 0)
            {
                scrollbar.SetThumbPosition(-1, -1);
                return;
            }

            var scrollY = PaddingTop + yOffset + (sps.RowIndex * sps.RowHeight) - sps.RowTopOffset;
            var scrollBarY = (int)(((float)scrollY / availableScrollHeight) * availableScrollBarHeight);

            int scrollBarX;
            if (Utils.IsRtl(Resources))
                scrollBarX = 0;
            else
                scrollBarX = Width - scrollbar.GetWidth();
            scrollbar.SetThumbPosition(scrollBarX, scrollBarY);
        }

        public string ScrollToPositionAtProgress(float touchFraction)
        {
            var itemCount = GetAdapter().ItemCount;
            if (itemCount == 0)
                return "";

            var spanCount = 1;
            var rowCount = itemCount;
            if (GetLayoutManager() is GridLayoutManager)
            {
                spanCount = ((GridLayoutManager)GetLayoutManager()).SpanCount;
                rowCount = (int)Math.Ceiling((double)rowCount / spanCount);
            }

            StopScroll();

            GetCurScrollState(scrollPositionState);

            var itemPos = itemCount * touchFraction;

            var availableScrollHeight = GetAvailableScrollHeight(rowCount, scrollPositionState.RowHeight, 0);

            var exactItemPos = (int)(availableScrollHeight * touchFraction);

            var layoutManager = ((LinearLayoutManager)GetLayoutManager());
            layoutManager.ScrollToPositionWithOffset(spanCount * exactItemPos / scrollPositionState.RowHeight, -(exactItemPos % scrollPositionState.RowHeight));

            if (!(GetAdapter() is ISectionedAdapter))
                return "";

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            var posInt = (int)((touchFraction == 1) ? itemPos - 1 : itemPos);
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

            var sectionedAdapter = (ISectionedAdapter)GetAdapter();
            return sectionedAdapter.GetSectionName(posInt);
        }

        public void OnUpdateScrollbar()
        {
            if (GetAdapter() == null)
                return;

            int rowCount = GetAdapter().ItemCount;
            if (GetLayoutManager() is GridLayoutManager)
            {
                var spanCount = ((GridLayoutManager)GetLayoutManager()).SpanCount;
                rowCount = (int)Math.Ceiling((double)rowCount / spanCount);
            }
            if (rowCount == 0)
            {
                scrollbar.SetThumbPosition(-1, -1);
                return;
            }

            if (scrollPositionState.RowIndex < 0)
            {
                scrollbar.SetThumbPosition(-1, -1);
                return;
            }

            SynchronizeScrollBarThumbOffsetToViewScroll(scrollPositionState, rowCount, 0);
        }

        void GetCurScrollState(FastScrollPositionState stateOut)
        {
            stateOut.RowIndex = -1;
            stateOut.RowTopOffset = -1;
            stateOut.RowHeight = -1;

            int itemCount = GetAdapter().ItemCount;

            // Return early if there are no items, or no children.
            if (itemCount == 0 || ChildCount == 0)
                return;

            var child = GetChildAt(0);

            stateOut.RowIndex = GetChildAdapterPosition(child);
            if (GetLayoutManager() is GridLayoutManager)
                stateOut.RowIndex = stateOut.RowIndex / ((GridLayoutManager)GetLayoutManager()).SpanCount;

            stateOut.RowTopOffset = GetLayoutManager().GetDecoratedTop(child);
            stateOut.RowHeight = child.Height + GetLayoutManager().GetTopDecorationHeight(child) + GetLayoutManager().GetBottomDecorationHeight(child);
        }

        public void SetThumbColor(Color color) => scrollbar.SetThumbColor(color);

        public void SetTrackColor(Color color) => scrollbar.SetTrackColor(color);

        public void SetPopupBackgroundColor(Color color) => scrollbar.SetPopupBackgroundColor(color);

        public void SetPopupTextColor(Color color) => scrollbar.SetPopupTextColor(color);

        public void SetPopupTextSize(int size) => scrollbar.SetPopupTextSize(size);

        public void SetPopupTypeface(Typeface typeface) => scrollbar.SetPopupTypeface(typeface);

        public void SetAutoHideDelay(int delay) => scrollbar.SetAutoHideDelay(delay);

        public void SetAutoHideEnabled(bool enabled) => scrollbar.SetAutoHideEnabled(enabled);

        public void SetStateChangeListener(IOnFastScrollStateChangeListener listener) => stateChangeListener = listener;

        public void SetPopupPosition(FastScrollerPosition position) => scrollbar.SetPopupPosition(position);
    }
}
