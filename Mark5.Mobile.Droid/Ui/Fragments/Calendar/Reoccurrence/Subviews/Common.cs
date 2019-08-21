using System;
using Android.Content;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar.Reoccurrence
{
    interface IEditable
    {
        void Refresh();
        void SetViewModel(RecurrenceInfo ri);
    }

    public abstract class RecurrenceParentView : LinearLayoutCompat, IEditable
    {
        RecurrenceInfo ri;
        public RecurrenceParentView(Context context) : base(context)
        {
        }

        public abstract void Refresh();

        public void SetViewModel(RecurrenceInfo ri)
        {
            this.ri = ri;
        }
    }
}
