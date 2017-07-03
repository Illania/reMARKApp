using System;
namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ResponsibleSelectionFragment : AbstractUserSelectionFragment
    {
        public ResponsibleSelectionFragment()
            : base(Resource.String.confirm, true)
        {
        }

        public override string GenerateTag()
        {
            return $"{nameof(ResponsibleSelectionFragment)}";
        }

        protected override void ActionButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override string GetInfo()
        {
            return $"[preselectedEntitites.Count ={PreselectedUserIds?.Count}]";
        }
    }
}
