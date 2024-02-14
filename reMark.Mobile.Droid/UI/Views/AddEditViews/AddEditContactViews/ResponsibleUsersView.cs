using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Droid.Ui.Fragments;
using reMark.Mobile.Common.Extensions;

namespace reMark.Mobile.Droid.Ui.Views.AddEditContactViews
{
    public class ResponsibleUsersView : AbstractSimpleFieldView
    {
        AddEditContactFragment parentFragment;

        public ResponsibleUsersView(Context context, AddEditContactFragment parentFragment)
            : base(context, Resource.String.edit_contact_responsible, true, false)
        {
            this.parentFragment = parentFragment;
        }

        public override void RefreshView()
        {
            if (Contact.ResponsibleUsers.Any())
            {
                Content = string.Join(", ", Contact.ResponsibleUsers.Values);
            }
        }

        protected override async void ContentClicked(object sender, EventArgs e)
        {
            var (pllf, tag) = ResponsibleSelectionFragment.NewInstance(Contact.ResponsibleUsers.Keys.ToList());

            parentFragment.ReplaceFragment(pllf, tag);

            HandleResponsibleUsersSelection(await pllf.Task);
        }

        void HandleResponsibleUsersSelection(Dictionary<int, string> selectedUsers)
        {
            if (!selectedUsers.Any())
            {
                Clear();
                return;
            }

            Contact.ResponsibleUsers.Clear();
            Contact.ResponsibleUserIds.Clear();
            selectedUsers.ForEach(kvp =>
            {
                Contact.ResponsibleUsers.Add(kvp.Key, kvp.Value);
                Contact.ResponsibleUserIds.Add(kvp.Key);
            });

            RefreshView();
        }

        void Clear()
        {
            Contact.ResponsibleUsers.Clear();
            Contact.ResponsibleUserIds.Clear();
            Content = string.Empty;
        }
    }
}
