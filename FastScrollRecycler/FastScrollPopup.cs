//
// Project: Mark5.Mobile.Droid
// File: FastScrollPopup.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Animation;
using Android.Content.Res;
using Android.Graphics;

namespace FastScrollRecycler
{

    class FastScrollPopup : Java.Lang.Object
    {

        readonly FastScrollRecyclerView recyclerView;
        readonly Resources resources;

        int backgroundSize;
        int cornerRadius;

        Path backgroundPath;
        RectF backgroundRect;
        Paint backgroundPaint;
        int backgroundColor;

        Rect invalidateRect;
        Rect tmpRect;

        Rect backgroundBounds;

        string sectionName;

        Paint textPaint;
        Rect textBounds;

        float alpha = 1f;

        ObjectAnimator alphaAnimator;
        bool visible;

        FastScrollerPosition position;

        public FastScrollPopup(Resources resources, FastScrollRecyclerView recyclerView)
        {
            this.resources = resources;
            this.recyclerView = recyclerView;

            backgroundPath = new Path();
            backgroundRect = new RectF();
            backgroundPaint = new Paint(PaintFlags.AntiAlias);

            invalidateRect = new Rect();
            tmpRect = new Rect();

            backgroundBounds = new Rect();

            textPaint = new Paint(PaintFlags.AntiAlias);
            textPaint.Alpha = 0;
            textBounds = new Rect();

            SetTextSize(Utils.ToScreenPixels(resources, 56f));
            SetBackgroundSize(Utils.ToPixels(resources, 88f));
        }

        public void SetBackgroundColor(int color)
        {
            backgroundColor = color;
            backgroundPaint.Color = new Color(color);
            recyclerView.Invalidate(backgroundBounds);
        }

        public void SetTextColor(int color)
        {
            textPaint.Color = new Color(color);
            recyclerView.Invalidate(backgroundBounds);
        }

        public void SetTextSize(int size)
        {
            textPaint.TextSize = size;
            recyclerView.Invalidate(backgroundBounds);
        }

        public void SetBackgroundSize(int size)
        {
            backgroundSize = size;
            cornerRadius = backgroundSize / 2;
            recyclerView.Invalidate(backgroundBounds);
        }

        public void SetTypeFace(Typeface typeface)
        {
            textPaint.SetTypeface(typeface);
            recyclerView.Invalidate(backgroundBounds);
        }

        public void AnimateVisibility(bool visible)
        {
            if (this.visible == visible)
                return;

            this.visible = visible;

            alphaAnimator?.Cancel();

            alphaAnimator = ObjectAnimator.OfFloat(this, "alpha", visible ? 1f : 0f);
            alphaAnimator.SetDuration(visible ? 200L : 150L);
            alphaAnimator.Start();
        }

        public void SetAlpha(float alpha)
        {
            this.alpha = alpha;
            recyclerView.Invalidate(backgroundBounds);
        }

        public float GetAlpha()
        {
            return alpha;
        }

        public void SetPopupPosition(FastScrollerPosition position)
        {
            this.position = position;
        }

        public FastScrollerPosition GetPopupPosition()
        {
            return position;
        }

        float[] CreateRadii()
        {
            if (position == FastScrollerPosition.Center)
                return new float[] { cornerRadius, cornerRadius, cornerRadius, cornerRadius, cornerRadius, cornerRadius, cornerRadius, cornerRadius };

            if (Utils.IsRtl(resources))
                return new float[] { cornerRadius, cornerRadius, cornerRadius, cornerRadius, cornerRadius, cornerRadius, 0f, 0f };

            return new float[] { cornerRadius, cornerRadius, cornerRadius, cornerRadius, 0f, 0f, cornerRadius, cornerRadius };
        }

        public void Draw(Canvas canvas)
        {
            if (!IsVisible())
                return;

            var restoreCount = canvas.Save(SaveFlags.Matrix);
            canvas.Translate(backgroundBounds.Left, backgroundBounds.Top);
            tmpRect.Set(backgroundBounds);
            tmpRect.Offset(0, 0);

            backgroundPath.Reset();
            backgroundRect.Set(tmpRect);

            var radii = CreateRadii();

            backgroundPath.AddRoundRect(backgroundRect, radii, Path.Direction.Cw);

            backgroundPaint.Alpha = (int)(Color.GetAlphaComponent(backgroundColor) * alpha);
            textPaint.Alpha = (int)(alpha * 255);
            canvas.DrawPath(backgroundPath, backgroundPaint);
            canvas.DrawText(sectionName, (backgroundBounds.Width() - textBounds.Width()) / 2,
                            backgroundBounds.Height() - (backgroundBounds.Height() - textBounds.Height()) / 2,
                            textPaint);
            canvas.RestoreToCount(restoreCount);
        }

        public void SetSectionName(string sectionName)
        {
            if (this.sectionName == sectionName)
                return;

            this.sectionName = sectionName;
            textPaint.GetTextBounds(sectionName, 0, sectionName.Length, textBounds);
            textBounds.Right = (int)(textBounds.Left + textPaint.MeasureText(sectionName));
        }

        public Rect UpdateFastScrollerBounds(FastScrollRecyclerView recyclerView, int thumbOffsetY)
        {
            invalidateRect.Set(backgroundBounds);

            if (IsVisible())
            {
                var edgePadding = recyclerView.GetScrollBarWidth();
                var backgroundPadding = (backgroundSize - textBounds.Height()) / 2;
                var backgroundHeight = backgroundSize;
                var backgroundWidth = Math.Max(backgroundSize, textBounds.Width() + (2 * backgroundPadding));
                if (position == FastScrollerPosition.Center)
                {
                    backgroundBounds.Left = (recyclerView.Width - backgroundWidth) / 2;
                    backgroundBounds.Right = backgroundBounds.Left + backgroundWidth;
                    backgroundBounds.Top = (recyclerView.Height - backgroundHeight) / 2;
                }
                else
                {
                    if (Utils.IsRtl(resources))
                    {
                        backgroundBounds.Left = 2 * recyclerView.GetScrollBarWidth();
                        backgroundBounds.Right = backgroundBounds.Left + backgroundWidth;
                    }
                    else
                    {
                        backgroundBounds.Right = recyclerView.Width - (2 * recyclerView.GetScrollBarWidth());
                        backgroundBounds.Left = backgroundBounds.Right - backgroundWidth;
                    }
                    backgroundRect.Top = thumbOffsetY - backgroundHeight + recyclerView.GetScrollBarThumbHeight() / 2;
                    backgroundRect.Top = Math.Max(edgePadding, Math.Min(backgroundBounds.Top, recyclerView.Height - edgePadding - backgroundHeight));
                }
                backgroundBounds.Bottom = backgroundBounds.Top + backgroundHeight;
            }
            else
            {
                backgroundBounds.SetEmpty();
            }

            invalidateRect.Union(backgroundBounds);
            return invalidateRect;
        }

        public bool IsVisible()
        {
            return alpha > 0f && !string.IsNullOrEmpty(sectionName);
        }
    }
}
