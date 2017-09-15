using System;
using Android.Content;
using Android.Graphics;
using Android.Animation;
using MaterialDialogs.MaterialProgressBar.Internal;
using Java.Interop;

namespace MaterialDialogs.MaterialProgressBar
{
    class IndeterminateHorizontalProgressDrawable : BaseIndeterminateProgressDrawable, IShowBackgroundDrawable
    {
        static readonly int ProgressIntrinsicHeightDP = 4;
        static readonly int PaddedIntrinsicHeightDP = 16;
        static readonly RectF RectBound = new RectF(-180, -1, 180, 1);
        static readonly RectF RectPaddedBound = new RectF(-180, -4, 180, 4);
        static readonly RectF RectProgress = new RectF(-144, -1, 144, 1);
        static readonly RectTransformX Rect1TransformX = new RectTransformX(-522.6f, 0.1f);
        static readonly RectTransformX Rect2TransformX = new RectTransformX(-197.6f, 0.1f);

		bool showBackground = true;
		float backgroundAlpha;
        int progressIntrinsicHeight;
        int paddedIntrinsicHeight;

		RectTransformX mRect1TransformX = new RectTransformX(Rect1TransformX);
		RectTransformX mRect2TransformX = new RectTransformX(Rect2TransformX);

		/**
		 * Creates a new IndeterminateHorizontalProgressDrawable.
		 */
        public IndeterminateHorizontalProgressDrawable(Context context) 
            : base(context)
		{
            var density = context.Resources.DisplayMetrics.Density;
            progressIntrinsicHeight = (int)Math.Round(ProgressIntrinsicHeightDP * density);
			paddedIntrinsicHeight = (int)Math.Round(PaddedIntrinsicHeightDP * density);
            backgroundAlpha = ThemeUtils.GetFloatFromAttrRes(Android.Resource.Attribute.DisabledAlpha, 0, context);

            Animators = new Animator[] {
                MaterialProgressBar.Animators.CreateIndeterminateHorizontalRect1(mRect1TransformX),
                MaterialProgressBar.Animators.CreateIndeterminateHorizontalRect2(mRect2TransformX),

		    };
		}

        public bool GetShowBackground()
        {
            return showBackground;
        }

        public void SetShowBackground(bool show)
        {
            if (showBackground != show)
            {
                showBackground = show;
                InvalidateSelf();
            }
        }

        public override int IntrinsicHeight
        {
            get
            {
                return UseIntrinsicPadding ? paddedIntrinsicHeight : progressIntrinsicHeight;
            }
        }

        protected override void OnPreparePaint(Paint paint)
        {
            paint.SetStyle(Paint.Style.Fill);
        }

        protected override void OnDraw(Canvas canvas, int width, int height, Paint paint)
        {
            if (UseIntrinsicPadding)
            {
                canvas.Scale(width / RectPaddedBound.Width(), height / RectPaddedBound.Height());
                canvas.Translate(RectPaddedBound.Width() / 2, RectPaddedBound.Height() / 2);
            }
            else
            {
                canvas.Scale(width / RectBound.Width(), height / RectBound.Height());
                canvas.Translate(RectBound.Width() / 2, RectBound.Height() / 2);
            }

            if (showBackground)
            {
                paint.Alpha = (int)Math.Round(Alpha * backgroundAlpha);
                DrawBackgroundRect(canvas, paint);
                paint.Alpha = Alpha;
            }
            DrawProgressRect(canvas, mRect2TransformX, paint);
            DrawProgressRect(canvas, mRect1TransformX, paint);
        }

		static void DrawBackgroundRect(Canvas canvas, Paint paint)
		{
			canvas.DrawRect(RectBound, paint);
		}

		static void DrawProgressRect(Canvas canvas, RectTransformX transformX, Paint paint)
		{
			var saveCount = canvas.Save();
			canvas.Translate(transformX.TranslateX, 0);
			canvas.Scale(transformX.ScaleX, 1);
			canvas.DrawRect(RectProgress, paint);
			canvas.RestoreToCount(saveCount);
		}

        public class RectTransformX : Java.Lang.Object
		{
            public float TranslateX 
            {
                [Export("get" + nameof(TranslateX))]
                get;
                [Export("set" + nameof(TranslateX))]
                set;
            }
            public float ScaleX
            {
                [Export("get" + nameof(ScaleX))]
                get;
                [Export("set" + nameof(ScaleX))]
                set;
            }

			public RectTransformX(float translateX, float scaleX)
			{
				TranslateX = translateX;
				ScaleX = scaleX;
			}

			public RectTransformX(RectTransformX that)
			{
				TranslateX = that.TranslateX;
				ScaleX = that.ScaleX;
			}
		}
    }
}
