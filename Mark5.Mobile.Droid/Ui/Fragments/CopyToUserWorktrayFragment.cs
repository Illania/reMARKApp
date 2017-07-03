using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class CopyToUserWorktrayFragment : AbstractUserSelectionFragment
    {
        public List<IBusinessEntity> BusinessEntities { get; set; }
        public Action CloseRequest { get; set; }

        public CopyToUserWorktrayFragment()
            : base(Resource.String.copy_to_worktray, false)
        {
        }

        protected override string GetInfo()
        {
            return $"[businessEntities.Count ={BusinessEntities?.Count}]";
        }

        protected async override void ActionButton_Click(object sender, EventArgs e)
        {
            CommonConfig.Logger.Info($"Attempting copy to worktray [businessEntities.Count={BusinessEntities.Count}, selectedUsers.Count={SelectedSystemUsers.Count}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_worktray, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.CopyToUserWorktray(BusinessEntities, SelectedSystemUsers.Values.ToList());

                CloseRequest?.Invoke();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Copying to worktray failed [businessEntities.Count={BusinessEntities.Count}, selectedUsers.Count={SelectedSystemUsers.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }


        #region RetainableStateFragment overrides

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state [businessEntities.Count={BusinessEntities?.Count}, systemUsers.Count={Adapter?.ItemCount}, selectedSystemUsers.Count={SelectedSystemUsers.Count}]...");

            var baseState = base.OnRetainInstanceState();
            var fragmentState = new CopyToUserWorktrayFragmentState(baseState as AbstractUserSelectionFragmentState)
            {
                BusinessEntities = BusinessEntities
            };

            return fragmentState;
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            base.OnRetainedInstanceStateRestored(restoredState);

            if (restoredState is CopyToUserWorktrayFragmentState dlfs)
            {
                BusinessEntities = dlfs.BusinessEntities;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(CopyToUserWorktrayFragment)}]";
        }

        #endregion

        #region State

        class CopyToUserWorktrayFragmentState : AbstractUserSelectionFragmentState
        {
            public CopyToUserWorktrayFragmentState(AbstractUserSelectionFragmentState state)
            {
                SelectedSystemUsers = state.SelectedSystemUsers;
                SystemUsers = state.SystemUsers;
            }

            public List<IBusinessEntity> BusinessEntities { get; set; }
        }

        #endregion
    }
}