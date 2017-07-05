using System;
using System.Collections.Generic;
using Android.Support.V7.App;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ResponsibleSelectionFragment : AbstractUserSelectionFragment
    {
        public Action<Dictionary<int, string>> CloseRequest { get; set; }

        public ResponsibleSelectionFragment()
            : base(Resource.String.confirm, true, true)
        {
        }

        public override string GenerateTag()
        {
            return $"{nameof(ResponsibleSelectionFragment)}";
        }

        protected override void ActionButton_Click(object sender, EventArgs e)
        {
            var responsibleDict = new Dictionary<int, string>();
            foreach (var kvp in SelectedSystemUsers)
            {
                responsibleDict.Add(kvp.Key, kvp.Value.Username);
            }
            CloseRequest?.Invoke(responsibleDict);
            ((AppCompatActivity)Activity).OnBackPressed();
        }

        protected override string GetInfo()
        {
            return $"[preselectedEntitites.Count ={PreselectedUserIds?.Count}]";
        }
    }
}
