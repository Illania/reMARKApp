using System;
namespace MaterialDialogs.ProgBar
{
	/**
	 * A {@code Drawable} that has an intrinsic padding.
	 */
	public interface IIntrinsicPaddingDrawable
    {
		/**
		  * Get whether this drawable is using an intrinsic padding. The default is {@code true}.
		  *
		  * @return Whether this drawable is using an intrinsic padding.
		  */
		bool GetUseIntrinsicPadding();

		/**
		 * Set whether this drawable should use an intrinsic padding. The default is {@code true}.
		 *
		 * @param useIntrinsicPadding Whether this drawable should use its intrinsic padding.
		 */
		void SetUseIntrinsicPadding(bool useIntrinsicPadding);
    }
}
