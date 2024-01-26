using Android.Content;
using AndroidX.RecyclerView.Widget;
using Java.Lang;
using reMark.Mobile.Common;

namespace reMark.Mobile.Droid.Ui.Common
{
    public class WrapContentLinearLayoutManager: LinearLayoutManager
    {
        public WrapContentLinearLayoutManager(Context context) : base(context)
        {

        }

        public override void OnLayoutChildren(RecyclerView.Recycler recycler, RecyclerView.State state)
        {
            try
            {
                base.OnLayoutChildren(recycler, state);
            }
            catch (IndexOutOfBoundsException ex)
            {
                CommonConfig.Logger.Warning(ex.Message);
            }
        }
    }
}

