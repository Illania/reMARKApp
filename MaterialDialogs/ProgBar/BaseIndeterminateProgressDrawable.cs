using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using MaterialDialogs.ProgBar.Internal;

namespace MaterialDialogs.ProgBar
{
    public abstract class BaseIndeterminateProgressDrawable : BaseProgressDrawable, IAnimatable
    {
        internal Animator[] Animators;

        public bool IsRunning 
        {
            get
            {
				foreach (Animator animator in Animators)
				{
					if (animator.IsRunning)
						return true;
				}
				return false;
            }
        }

        internal BaseIndeterminateProgressDrawable(Context context)
		{
            var controlActivatedColor = ThemeUtils.GetColorFromAttrRes(Resource.Attribute.colorControlActivated,Color.Black, context);
			// setTint() has been overridden for compatibility; DrawableCompat won't work because
			// wrapped Drawable won't be Animatable.
			SetTint(controlActivatedColor);
		}

        public override void Draw(Canvas canvas)
        {
            base.Draw(canvas);

			if (IsStarted())
				InvalidateSelf();
        }

        bool IsStarted() 
        {
			foreach (Animator animator in Animators)
			{
                if (animator.IsStarted)
					return true;
			}
			return false;
        }

        public void Start()
        {
			if (IsStarted())			
				return;
			
            foreach (Animator animator in Animators)
				animator.Start();            
            
			InvalidateSelf();
        }

        public void Stop()
        {
			foreach (Animator animator in Animators)	
				animator.End();
        }
    }
}
