//
// File: FastScrollPopup.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using Android.Animation;
using Android.Content.Res;
using Android.Graphics;
using Android.Text;
using Java.Interop;

namespace FastScrollRecycler
{
    class FastScrollPopup : Java.Lang.Object
    {
        readonly FastScrollRecyclerView recyclerView;
        readonly Resources resources;

        int backgroundSize;
        int cornerRadius;

        Path backgroundPath = new Path();
        RectF backgroundRect = new RectF();
        Paint backgroundPaint;
        Color backgroundColor = Color.Argb(0xff, 0x00, 0x00, 0x00);

        Rect invalidateRect = new Rect();
        Rect tmpRect = new Rect();

        Rect backgroundBounds = new Rect();

        string sectionName;

        Paint textPaint;
        Rect textBounds = new Rect();

        float alpha = 1f;

        ObjectAnimator alphaAnimator;
        bool visible;

        FastScrollerPosition position;

        public FastScrollPopup(Resources resources, FastScrollRecyclerView recyclerView)
        {
            this.resources = resources;
            this.recyclerView = recyclerView;

            backgroundPaint = new Paint(PaintFlags.AntiAlias);

            textPaint = new Paint(PaintFlags.AntiAlias)
            {
                Alpha = 0
            };
            SetTextSize(Utils.ToScreenPixels(resources, 56f));
            SetBackgroundSize(Utils.ToPixels(resources, 88f));
        }

        public void SetBackgroundColor(Color color)
        {
            backgroundColor = color;
            backgroundPaint.Color = color;
            recyclerView.Invalidate(backgroundBounds);
        }

        public void SetTextColor(Color color)
        {
            textPaint.Color = color;
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
            if (this.visible != visible)
            {
                this.visible = visible;

                alphaAnimator?.Cancel();

                alphaAnimator = ObjectAnimator.OfFloat(this, "alpha", visible ? 1f : 0f);
                alphaAnimator.SetDuration(visible ? 200L : 150L);
                alphaAnimator.Start();
            }
        }

        [Export("setAlpha")]
        public void SetAlpha(float alpha)
        {
            this.alpha = alpha;
            recyclerView.Invalidate(backgroundBounds);
        }

        [Export("getAlpha")]
        public float GetAlpha()
        {
            return alpha;
        }

        public void SetPopupPosition(FastScrollerPosition position) => this.position = position;

        public FastScrollerPosition GetPopupPosition()
        {
            return position;
        }

        float[] CreateRadii()
        {
            if (position == FastScrollerPosition.Center)
                return new float[]
                {
                    cornerRadius,
                    cornerRadius,
                    cornerRadius,
                    cornerRadius,
                    cornerRadius,
                    cornerRadius,
                    cornerRadius,
                    cornerRadius
                };

            if (Utils.IsRtl(resources))
                return new float[]
                {
                    cornerRadius,
                    cornerRadius,
                    cornerRadius,
                    cornerRadius,
                    cornerRadius,
                    cornerRadius,
                    0f,
                    0f
                };

            return new float[]
            {
                cornerRadius,
                cornerRadius,
                cornerRadius,
                cornerRadius,
                0f,
                0f,
                cornerRadius,
                cornerRadius
            };
        }

        public void Draw(Canvas canvas)
        {
            if (IsVisible())
            {
                var restoreCount = canvas.Save(SaveFlags.Matrix);
                canvas.Translate(backgroundBounds.Left, backgroundBounds.Top);
                tmpRect.Set(backgroundBounds);
                tmpRect.OffsetTo(0, 0);

                backgroundPath.Reset();
                backgroundRect.Set(tmpRect);

                var radii = CreateRadii();

                backgroundPath.AddRoundRect(backgroundRect, radii, Path.Direction.Cw);

                backgroundPaint.Alpha = (int) (Color.GetAlphaComponent(backgroundColor) * alpha);
                textPaint.Alpha = (int) (alpha * 255);
                canvas.DrawPath(backgroundPath, backgroundPaint);
                canvas.DrawText(sectionName, (backgroundBounds.Width() - textBounds.Width()) / 2, backgroundBounds.Height() - (backgroundBounds.Height() - textBounds.Height()) / 2, textPaint);
                canvas.RestoreToCount(restoreCount);
            }
        }

        public void SetSectionName(string sectionName)
        {
            if (this.sectionName != sectionName)
            {
                this.sectionName = sectionName;
                textPaint.GetTextBounds(sectionName, 0, sectionName.Length, textBounds);
                textBounds.Right = (int) (textBounds.Left + textPaint.MeasureText(sectionName));
            }
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
                    backgroundBounds.Top = thumbOffsetY - backgroundHeight + recyclerView.GetScrollBarThumbHeight() / 2;
                    backgroundBounds.Top = Math.Max(edgePadding, Math.Min(backgroundBounds.Top, recyclerView.Height - edgePadding - backgroundHeight));
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
            return alpha > 0f && !TextUtils.IsEmpty(sectionName);
        }
    }
}