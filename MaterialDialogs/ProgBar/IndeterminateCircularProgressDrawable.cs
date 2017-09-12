using System;
using Android.Graphics;
using Android.Content;
using Android.Animation;
using Java.Interop;

namespace MaterialDialogs.ProgBar
{
    public class IndeterminateCircularProgressDrawable : BaseIndeterminateProgressDrawable
    {
        static int ProgressIntrinsicSizeDP = 42;
        static int PaddedIntrinsicSizeDP = 48;
        static RectF RectBound = new RectF(-21, -21, 21, 21);
		static RectF RectPaddedBound = new RectF(-24, -24, 24, 24);
        static RectF RectProgress = new RectF(-19, -19, 19, 19);

        int progressIntrinsicSize;
        int paddedIntrinsicSize;

        RingPathTransform ringPathTransform = new RingPathTransform();
        RingRotation ringRotation = new RingRotation();

		/**
		 * Create a new {@code IndeterminateCircularProgressDrawable}.
		 *
		 * @param context the {@code Context} for retrieving style information.
		 */
        public IndeterminateCircularProgressDrawable(Context context) 
            : base(context)
		{
            var density = context.Resources.DisplayMetrics.Density;
			progressIntrinsicSize = (int)Math.Round(ProgressIntrinsicSizeDP * density);
			paddedIntrinsicSize = (int)Math.Round(PaddedIntrinsicSizeDP * density);

            Animators = new Animator[] {
                ProgBar.Animators.CreateIndeterminate(ringPathTransform),
                ProgBar.Animators.CreateIndeterminateRotation(ringRotation)
		    };
		}

		int GetIntrinsicSize()
		{
			return UseIntrinsicPadding ? paddedIntrinsicSize : progressIntrinsicSize;
		}

        public override int IntrinsicWidth
        {
            get
            {
                return GetIntrinsicSize();
            }
        }

        public override int IntrinsicHeight
        {
            get
            {
                return GetIntrinsicSize(); 
            }
        }

		protected override void OnPreparePaint(Paint paint)
		{
            paint.SetStyle(Paint.Style.Stroke);
            paint.StrokeWidth = 4;
            paint.StrokeCap = Paint.Cap.Square;
            paint.StrokeJoin = Paint.Join.Miter;
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
			DrawRing(canvas, paint);
        }

	    internal void DrawRing(Canvas canvas, Paint paint)
		{
			var saveCount = canvas.Save();
            canvas.Rotate(ringRotation.Rotation);

			// startAngle starts at 3 o'clock on a watch.
			var startAngle = -90 + 360 * (ringPathTransform.TrimPathOffset + ringPathTransform.TrimPathStart);
			var sweepAngle = 360 * (ringPathTransform.TrimPathEnd - ringPathTransform.TrimPathStart);
			canvas.DrawArc(RectProgress, startAngle, sweepAngle, false, paint);
			canvas.RestoreToCount(saveCount);
		}

        public class RingPathTransform : Java.Lang.Object
		{
            public float TrimPathStart
            {
                [Export("get" + nameof(TrimPathStart))]
                get;
                [Export("set" + nameof(TrimPathStart))]
                set;
            }

            public float TrimPathEnd
            {
                [Export("get" + nameof(TrimPathEnd))]
                get;
                [Export("set" + nameof(TrimPathEnd))]
                set;
            }
            public float TrimPathOffset
            {
                [Export("get" + nameof(TrimPathOffset))]
                get;
                [Export("set" + nameof(TrimPathOffset))]
                set;
            }
		}

        public class RingRotation : Java.Lang.Object
		{
            public float Rotation
            {
                [Export("get" + nameof(Rotation))]
                get;
                [Export("set" + nameof(Rotation))]
                set;
            }
		}
	}
}
