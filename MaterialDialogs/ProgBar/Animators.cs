using Android.Animation;
using Android.Graphics;
using MaterialDialogs.ProgBar.Internal;
using static MaterialDialogs.ProgBar.IndeterminateCircularProgressDrawable;
using static MaterialDialogs.ProgBar.IndeterminateHorizontalProgressDrawable;

namespace MaterialDialogs.ProgBar
{
    public static class Animators
    {
        // M -522.59998,0
        // c 48.89972,0 166.02656,0 301.21729,0
        // c 197.58128,0 420.9827,0 420.9827,0
        static Path PathIndeterminateHorizontalRect1TranslateX;

		// M 0 0.1
		// L 1 0.826849212646
		// L 2 0.1
        static Path PathIndeterminateHorizontalRect1ScaleX;

		// M -197.60001,0
		// c 14.28182,0 85.07782,0 135.54689,0
		// c 54.26191,0 90.42461,0 168.24331,0
		// c 144.72154,0 316.40982,0 316.40982,0
        static Path PathIndeterminateHorizontalRect2TranslateX;

		// M 0.0,0.1
		// L 1.0,0.571379510698
		// L 2.0,0.909950256348
		// L 3.0,0.1
        static Path PathIndeterminateHorizontalRect2ScaleX;

        static Animators()
        {
			PathIndeterminateHorizontalRect1TranslateX = new Path();
            PathIndeterminateHorizontalRect1TranslateX.MoveTo(-522.59998f, 0);
            PathIndeterminateHorizontalRect1TranslateX.RCubicTo(48.89972f, 0, 166.02656f, 0, 301.21729f, 0);
			PathIndeterminateHorizontalRect1TranslateX.RCubicTo(197.58128f, 0, 420.9827f, 0, 420.9827f, 0);

			PathIndeterminateHorizontalRect1ScaleX = new Path();
			PathIndeterminateHorizontalRect1ScaleX.MoveTo(0, 0.1f);
			PathIndeterminateHorizontalRect1ScaleX.LineTo(1, 0.826849212646f);
			PathIndeterminateHorizontalRect1ScaleX.LineTo(2, 0.1f);

			PathIndeterminateHorizontalRect2TranslateX = new Path();
			PathIndeterminateHorizontalRect2TranslateX.MoveTo(-197.60001f, 0);
			PathIndeterminateHorizontalRect2TranslateX.RCubicTo(14.28182f, 0, 85.07782f, 0, 135.54689f, 0);
			PathIndeterminateHorizontalRect2TranslateX.RCubicTo(54.26191f, 0, 90.42461f, 0, 168.24331f, 0);
			PathIndeterminateHorizontalRect2TranslateX.RCubicTo(144.72154f, 0, 316.40982f, 0, 316.40982f, 0);

			PathIndeterminateHorizontalRect2ScaleX = new Path();
			PathIndeterminateHorizontalRect2ScaleX.MoveTo(0, 0.1f);
			PathIndeterminateHorizontalRect2ScaleX.LineTo(1, 0.571379510698f);
			PathIndeterminateHorizontalRect2ScaleX.LineTo(2, 0.909950256348f);
			PathIndeterminateHorizontalRect2ScaleX.LineTo(3, 0.1f);
        }

		/**
		 * Create a backported Animator for
		 * {@code @android:anim/progress_indeterminate_horizontal_rect1}.
		 *
		 * @param target The object whose properties are to be animated.
		 * @return An Animator object that is set up to behave the same as the its native counterpart.
		 */
		public static Animator CreateIndeterminateHorizontalRect1(Java.Lang.Object target)
		{
            var translateXAnimator = ObjectAnimatorCompat.OfFloat(target, nameof(RectTransformX.TranslateX), null, PathIndeterminateHorizontalRect1TranslateX);
			translateXAnimator.SetDuration(2000);
			translateXAnimator.SetInterpolator(Interpolators.IndeterminateHorizontalRect1TranslateX.Instance);
            translateXAnimator.RepeatCount = ValueAnimator.Infinite;

            var scaleXAnimator = ObjectAnimatorCompat.OfFloat(target, null, nameof(RectTransformX.ScaleX), PathIndeterminateHorizontalRect1ScaleX);
			scaleXAnimator.SetDuration(2000);
			scaleXAnimator.SetInterpolator(Interpolators.IndeterminateHorizontalRect1ScaleX.Instance);
            scaleXAnimator.RepeatCount = ValueAnimator.Infinite;

			var animatorSet = new AnimatorSet();
            animatorSet.PlayTogether(translateXAnimator, scaleXAnimator);
			return animatorSet;
		}

		/**
		 * Create a backported Animator for
		 * {@code @android:anim/progress_indeterminate_horizontal_rect2}.
		 *
		 * @param target The object whose properties are to be animated.
		 * @return An Animator object that is set up to behave the same as the its native counterpart.
		 */
		public static Animator CreateIndeterminateHorizontalRect2(Java.Lang.Object target)
		{
            var translateXAnimator = ObjectAnimatorCompat.OfFloat(target, nameof(RectTransformX.TranslateX), null, PathIndeterminateHorizontalRect2TranslateX);
            translateXAnimator.SetDuration(2000);
			translateXAnimator.SetInterpolator(Interpolators.IndeterminateHorizontalRect2TranslateX.Instance);
            translateXAnimator.RepeatCount = ValueAnimator.Infinite;

            var scaleXAnimator = ObjectAnimatorCompat.OfFloat(target, null, nameof(RectTransformX.ScaleX), PathIndeterminateHorizontalRect2ScaleX);
            scaleXAnimator.SetDuration(2000);
			scaleXAnimator.SetInterpolator(Interpolators.IndeterminateHorizontalRect2ScaleX.Instance);
            scaleXAnimator.RepeatCount = ValueAnimator.Infinite;

			var animatorSet = new AnimatorSet();
            animatorSet.PlayTogether(translateXAnimator, scaleXAnimator);
			return animatorSet;
		}

		/**
		 * Create a backported Animator for {@code @android:anim/progress_indeterminate_material}.
		 *
		 * @param target The object whose properties are to be animated.
		 * @return An Animator object that is set up to behave the same as the its native counterpart.
		 */
		public static Animator CreateIndeterminate(Java.Lang.Object target)
		{
            var trimPathStartAnimator = ObjectAnimator.OfFloat(target, nameof(RingPathTransform.TrimPathStart), 0, 0.75f);
            trimPathStartAnimator.SetDuration(1333);
            trimPathStartAnimator.SetInterpolator(Interpolators.TrimPathStart.Instance);
            trimPathStartAnimator.RepeatCount = ValueAnimator.Infinite;

            var trimPathEndAnimator = ObjectAnimator.OfFloat(target, nameof(RingPathTransform.TrimPathEnd), 0, 0.75f);
			trimPathEndAnimator.SetDuration(1333);
			trimPathEndAnimator.SetInterpolator(Interpolators.TrimPathEnd.Instance);
            trimPathEndAnimator.RepeatCount = ValueAnimator.Infinite;

            var trimPathOffsetAnimator = ObjectAnimator.OfFloat(target, nameof(RingPathTransform.TrimPathOffset), 0, 0.25f);
            trimPathOffsetAnimator.SetDuration(1333);
			trimPathOffsetAnimator.SetInterpolator(Interpolators.Linear.Instance);
            trimPathOffsetAnimator.RepeatCount = ValueAnimator.Infinite;

			var animatorSet = new AnimatorSet();
			animatorSet.PlayTogether(trimPathStartAnimator, trimPathEndAnimator, trimPathOffsetAnimator);
			return animatorSet;
		}

		/**
		 * Create a backported Animator for
		 * {@code @android:anim/progress_indeterminate_rotation_material}.
		 *
		 * @param target The object whose properties are to be animated.
		 * @return An Animator object that is set up to behave the same as the its native counterpart.
		 */
        public static Animator CreateIndeterminateRotation(Java.Lang.Object target)
		{
            var rotationAnimator = ObjectAnimator.OfFloat(target, nameof(RingRotation.Rotation), 0, 720);
			rotationAnimator.SetDuration(6665);
			rotationAnimator.SetInterpolator(Interpolators.Linear.Instance);
            rotationAnimator.RepeatCount = ValueAnimator.Infinite;
			return rotationAnimator;
		}
    }
}
