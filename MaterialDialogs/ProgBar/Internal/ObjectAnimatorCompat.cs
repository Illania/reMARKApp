using Android.Animation;
using Android.Graphics;
using Android.Util;

namespace MaterialDialogs.ProgBar.Internal
{
	/**
	 * Helper for accessing features in {@link ObjectAnimator} introduced after API level 11 (for
	 * {@link android.animation.PropertyValuesHolder}) in a backward compatible fashion.
	 */

	public static class ObjectAnimatorCompat
    {
		/**
		  * Constructs and returns an ObjectAnimator that animates between color values. A single
		  * value implies that that value is the one being animated to. Two values imply starting
		  * and ending values. More than two values imply a starting value, values to animate through
		  * along the way, and an ending value (these values will be distributed evenly across
		  * the duration of the animation).
		  *
		  * @param target The object whose property is to be animated. This object should have a public
		  *               method on it called {@code setName()}, where {@code name} is the value of the
		  *               {@code propertyName} parameter.
		  * @param propertyName The name of the property being animated.
		  * @param values A set of values that the animation will animate between over time.
		  * @return An ObjectAnimator object that is set up to animate between the given values.
		  */

		public static ObjectAnimator OfArgb(Java.Lang.Object target, string propertyName, params int[] values)
		{
            return ObjectAnimatorCompatLollipop.OfArgb(target, propertyName, values);
      	}

		/**
		 * Constructs and returns an ObjectAnimator that animates between color values. A single
		 * value implies that that value is the one being animated to. Two values imply starting
		 * and ending values. More than two values imply a starting value, values to animate through
		 * along the way, and an ending value (these values will be distributed evenly across
		 * the duration of the animation).
		 *
		 * @param target The object whose property is to be animated.
		 * @param property The property being animated.
		 * @param values A set of values that the animation will animate between over time.
		 * @return An ObjectAnimator object that is set up to animate between the given values.
		 */
		public static ObjectAnimator OfArgb<T>(T target, Property property, params int[] values) 
            where T : Java.Lang.Object
		{
			return ObjectAnimatorCompatLollipop.OfArgb(target, property, values);
		}

		/**
		 * Constructs and returns an ObjectAnimator that animates coordinates along a {@code Path} using
		 * two properties. A {@code Path} animation moves in two dimensions, animating coordinates
		 * {@code (x, y)} together to follow the line. In this variation, the coordinates are floats
		 * that are set to separate properties designated by {@code xPropertyName} and
		 * {@code yPropertyName}.
		 *
		 * @param target The object whose properties are to be animated. This object should have public
		 *               methods on it called {@code setNameX()} and {@code setNameY}, where
		 *               {@code nameX} and {@code nameY} are the value of the {@code xPropertyName} and
		 *               {@code yPropertyName} parameters, respectively.
		 * @param xPropertyName The name of the property for the x coordinate being animated.
		 * @param yPropertyName The name of the property for the y coordinate being animated.
		 * @param path The {@code Path} to animate values along.
		 * @return An ObjectAnimator object that is set up to animate along {@code path}.
		 */
        public static ObjectAnimator OfFloat(Java.Lang.Object target, string xPropertyName, string yPropertyName, Path path)
		{
			return ObjectAnimatorCompatLollipop.OfFloat(target, xPropertyName, yPropertyName, path);	
		}

		/**
		 * Constructs and returns an ObjectAnimator that animates coordinates along a {@code Path} using
		 * two properties. A {@code Path} animation moves in two dimensions, animating coordinates
		 * {@code (x, y)} together to follow the line. In this variation, the coordinates are floats
		 * that are set to separate properties, {@code xProperty} and {@code yProperty}.
		 *
		 * @param target The object whose properties are to be animated.
		 * @param xProperty The property for the x coordinate being animated.
		 * @param yProperty The property for the y coordinate being animated.
		 * @param path The {@code Path} to animate values along.
		 * @return An ObjectAnimator object that is set up to animate along {@code path}.
		 */

		public static ObjectAnimator OfFloat<T>(T target, Property xProperty, Property yProperty, Path path) 
            where T : Java.Lang.Object
		{
			return ObjectAnimatorCompatLollipop.OfFloat(target, xProperty, yProperty, path);
		}

		/**
		 * Constructs and returns an ObjectAnimator that animates coordinates along a {@code Path} using
		 * two properties. A {@code Path} animation moves in two dimensions, animating coordinates
		 * {@code (x, y)} together to follow the line. In this variation, the coordinates are integers
		 * that are set to separate properties designated by {@code xPropertyName} and
		 * {@code yPropertyName}.
		 *
		 * @param target The object whose properties are to be animated. This object should have public
		 *               methods on it called {@code setNameX()} and {@code setNameY}, where
		 *               {@code nameX} and {@code nameY} are the value of {@code xPropertyName} and
		 *               {@code yPropertyName} parameters, respectively.
		 * @param xPropertyName The name of the property for the x coordinate being animated.
		 * @param yPropertyName The name of the property for the y coordinate being animated.
		 * @param path The {@code Path} to animate values along.
		 * @return An ObjectAnimator object that is set up to animate along {@code path}.
		 */
		public static ObjectAnimator OfInt(Java.Lang.Object target, string xPropertyName, string yPropertyName, Path path)
		{
		    return ObjectAnimatorCompatLollipop.OfInt(target, xPropertyName, yPropertyName, path);
		}

		/**
		 * Constructs and returns an ObjectAnimator that animates coordinates along a {@code Path} using
		 * two properties. A {@code Path} animation moves in two dimensions, animating coordinates
		 * {@code (x, y)} together to follow the line. In this variation, the coordinates are integers
		 * that are set to separate properties, {@code xProperty} and {@code yProperty}.
		 *
		 * @param target The object whose properties are to be animated.
		 * @param xProperty The property for the x coordinate being animated.
		 * @param yProperty The property for the y coordinate being animated.
		 * @param path The {@code Path} to animate values along.
		 * @return An ObjectAnimator object that is set up to animate along {@code path}.
		 */
		public static ObjectAnimator OfInt<T>(T target, Property xProperty, Property yProperty, Path path) 
            where T : Java.Lang.Object
		{
			return ObjectAnimatorCompatLollipop.OfInt(target, xProperty, yProperty, path);
		}
    }
}
