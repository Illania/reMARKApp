using System;
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.RecurrenceViews
{
    public class RangeView : RecurrenceParentView
    {
        HeaderView headerView;
        EndView endView;

        public RangeView(Context context) : base(context)
        {
            Orientation = Vertical;

            headerView = new HeaderView(context);
            endView = new EndView(context);
        }

        public override void Refresh()
        {
            headerView.Refresh();
            endView.Refresh();
        }

        public override void SetViewModel(RecurrenceInfo ri)
        {
            headerView.SetViewModel(ri);
            endView.SetViewModel(ri);
        }

        class HeaderView : RecurrenceSubView
        {
            public HeaderView(Context context) : base(context)
            {
                Orientation = Vertical;

            }

            public override void Refresh()
            {
                throw new NotImplementedException();
            }
        }

        class EndView : RecurrenceSubView
        {
            public EndView(Context context) : base(context)
            {
            }

            public override void Refresh()
            {
                throw new NotImplementedException();
            }
        }
    }
}
