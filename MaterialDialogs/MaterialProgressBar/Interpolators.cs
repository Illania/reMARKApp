using Android.Graphics;
using Android.Support.V4.View.Animation;
using Android.Views.Animations;

namespace MaterialDialogs.MaterialProgressBar.Internal
{
    class Interpolators
    {
        /**
		 * Backported Interpolator for
		 * {@code @android:interpolator/progress_indeterminate_horizontal_rect1_translatex}.
		 */
        public static class IndeterminateHorizontalRect1TranslateX
        {
            // M 0.0,0.0
            // L 0.2 0
            // C 0.3958333333336,0.0 0.47  4845090492,0.206797621729 0.5916666666664,0.417082932942
            // C 0.7151610251224,0.639379624869 0.81625,0.974556908664 1.0,1.0
            static readonly Path PathIndeterminateHorizontalRect1TranslateX;
            public static IInterpolator Instance;

            static IndeterminateHorizontalRect1TranslateX()
            {
                PathIndeterminateHorizontalRect1TranslateX = new Path();
                PathIndeterminateHorizontalRect1TranslateX.MoveTo(0, 0);
                PathIndeterminateHorizontalRect1TranslateX.LineTo(0.2f, 0);
                PathIndeterminateHorizontalRect1TranslateX.CubicTo(0.3958333333336f, 0, 0.474845090492f, 0.206797621729f, 0.5916666666664f, 0.417082932942f);
                PathIndeterminateHorizontalRect1TranslateX.CubicTo(0.7151610251224f, 0.639379624869f, 0.81625f, 0.974556908664f, 1, 1);

                Instance = PathInterpolatorCompat.Create(PathIndeterminateHorizontalRect1TranslateX);
            }
        }

        /**
		 * Backported Interpolator for
		 * {@code @android:interpolator/progress_indeterminate_horizontal_rect1_scalex}.
		 */
        public static class IndeterminateHorizontalRect1ScaleX
        {
            // M 0 0
            // L 0.3665 0
            // C 0.47252618112021,0.062409910275 0.61541608570164,0.5 0.68325,0.5
            // C 0.75475061236836,0.5 0.75725829093844,0.814510098964 1.0,1.0
            static readonly Path PathIndeterminateHorizontalRect1ScaleX;
            public static IInterpolator Instance;

            static IndeterminateHorizontalRect1ScaleX()
            {
                PathIndeterminateHorizontalRect1ScaleX = new Path();
                PathIndeterminateHorizontalRect1ScaleX.MoveTo(0, 0);
                PathIndeterminateHorizontalRect1ScaleX.LineTo(0.3665f, 0);
                PathIndeterminateHorizontalRect1ScaleX.CubicTo(0.47252618112021f, 0.062409910275f, 0.61541608570164f, 0.5f, 0.68325f, 0.5f);
                PathIndeterminateHorizontalRect1ScaleX.CubicTo(0.75475061236836f, 0.5f, 0.75725829093844f, 0.814510098964f, 1, 1);

                Instance = PathInterpolatorCompat.Create(PathIndeterminateHorizontalRect1ScaleX);
            }
        }

        /**
		 * Backported Interpolator for
		 * {@code @android:interpolator/progress_indeterminate_horizontal_rect2_translatex}.
		 */
        public static class IndeterminateHorizontalRect2TranslateX
        {
            // M 0.0,0.0
            // C 0.0375,0.0 0.128764607715,0.0895380946618 0.25,0.218553507947
            // C 0.322410320025,0.295610602487 0.436666666667,0.417591408114
            //     0.483333333333,0.489826169306
            // C 0.69,0.80972296795 0.793333333333,0.950016125212 1.0,1.0
            static readonly Path PathIndeterminateHorizontalRect2TranslateX;
            public static IInterpolator Instance;

            static IndeterminateHorizontalRect2TranslateX()
            {
                PathIndeterminateHorizontalRect2TranslateX = new Path();
                PathIndeterminateHorizontalRect2TranslateX.MoveTo(0, 0);
                PathIndeterminateHorizontalRect2TranslateX.CubicTo(0.0375f, 0, 0.128764607715f, 0.0895380946618f, 0.25f, 0.218553507947f);
                PathIndeterminateHorizontalRect2TranslateX.CubicTo(0.322410320025f, 0.295610602487f, 0.436666666667f,
                                                                   0.417591408114f, 0.483333333333f, 0.489826169306f);
                PathIndeterminateHorizontalRect2TranslateX.CubicTo(0.69f, 0.80972296795f, 0.793333333333f, 0.950016125212f, 1, 1);

                Instance = PathInterpolatorCompat.Create(PathIndeterminateHorizontalRect2TranslateX);
            }
        }

        /**
		 * Backported Interpolator for
		 * {@code @android:interpolator/progress_indeterminate_horizontal_rect2_scalex}.
		 */
        public static class IndeterminateHorizontalRect2ScaleX
        {
            // M 0,0
            // C 0.06834272400867,0.01992566661414 0.19220331656133,0.15855429260523 0.33333333333333,
            //     0.34926160892842
            // C 0.38410433133433,0.41477913453861 0.54945792615267,0.68136029463551 0.66666666666667,
            //     0.68279962777002
            // C 0.752586273196,0.68179620963216 0.737253971954,0.878896194318 1,1
            static readonly Path PathIndeterminateHorizontalRect2ScaleX;
            public static IInterpolator Instance;

            static IndeterminateHorizontalRect2ScaleX()
            {
                PathIndeterminateHorizontalRect2ScaleX = new Path();
                PathIndeterminateHorizontalRect2ScaleX.MoveTo(0, 0);
                PathIndeterminateHorizontalRect2ScaleX.CubicTo(0.06834272400867f, 0.01992566661414f, 0.19220331656133f,
                                                               0.15855429260523f, 0.33333333333333f, 0.34926160892842f);
                PathIndeterminateHorizontalRect2ScaleX.CubicTo(0.38410433133433f, 0.41477913453861f, 0.54945792615267f,
                                                               0.68136029463551f, 0.66666666666667f, 0.68279962777002f);
                PathIndeterminateHorizontalRect2ScaleX.CubicTo(0.752586273196f, 0.68179620963216f, 0.737253971954f, 0.878896194318f, 1, 1);

                Instance = PathInterpolatorCompat.Create(PathIndeterminateHorizontalRect2ScaleX);
            }
        }

        /**
		 * Backported Interpolator for {@code @android:interpolator/trim_start_interpolator}.
		 */
        public static class TrimPathStart
        {
            // L 0.5,0
            // C 0.7,0 0.6,1 1,1
            static readonly Path PathTrimPathStart;
            public static IInterpolator Instance;

            static TrimPathStart()
            {
                PathTrimPathStart = new Path();
                PathTrimPathStart.LineTo(0.5f, 0);
                PathTrimPathStart.CubicTo(0.7f, 0, 0.6f, 1, 1, 1);

                Instance = PathInterpolatorCompat.Create(PathTrimPathStart);
            }
        }

        /**
	     * Backported Interpolator for {@code @android:interpolator/trim_end_interpolator}.
	     */
        public static class TrimPathEnd
        {
            // C 0.2,0 0.1,1 0.5,1
            // L 1,1
            static readonly Path PathTrimPathEnd;
            public static IInterpolator Instance;

            static TrimPathEnd()
            {
                PathTrimPathEnd = new Path();
                PathTrimPathEnd.CubicTo(0.2f, 0, 0.1f, 1, 0.5f, 1);
                PathTrimPathEnd.LineTo(1, 1);

                Instance = PathInterpolatorCompat.Create(PathTrimPathEnd);
            }
        }

        /**
		 * Lazy-initialized singleton Interpolator for {@code @android:interpolator/linear}.
		 */
        public static class Linear
        {
            public static IInterpolator Instance = new LinearInterpolator();
        }
    }
}
