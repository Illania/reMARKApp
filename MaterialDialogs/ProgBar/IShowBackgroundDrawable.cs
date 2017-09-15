using System;
namespace MaterialDialogs.ProgBar
{
	/**
     * A {@code Drawable} that has a background.
     */
	public interface IShowBackgroundDrawable
    {
		/**
		 * Get whether this drawable is showing a background. The default is {@code true}.
		 *
		 * @return Whether this drawable is showing a background.
		 */
		bool GetShowBackground();

		/**
		 * Set whether this drawable should show a background. The default is {@code true}.
		 *
		 * @param show Whether background should be shown.
		 */
		void SetShowBackground(bool show);
    }
}
