namespace MaterialDialogs.MaterialProgressBar
{
    abstract class BaseProgressDrawable : BasePaintDrawable, IIntrinsicPaddingDrawable
    {
        internal bool UseIntrinsicPadding = true;

		public bool GetUseIntrinsicPadding()
        {
            return UseIntrinsicPadding;
        }

		public void SetUseIntrinsicPadding(bool useIntrinsicPadding)
        {
			if (UseIntrinsicPadding != useIntrinsicPadding)
			{
				UseIntrinsicPadding = useIntrinsicPadding;
				base.InvalidateSelf();
			}
        }
    }
}
