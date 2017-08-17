using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Fragments;
using Mark5.Mobile.Common.Extensions;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
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

        protected override void ContentClicked(object sender, EventArgs e)
        {
            var pllf = new ResponsibleSelectionFragment
            {
                CloseRequest = HandleResponsibleUsersSelection,
                PreselectedUserIds = Contact.ResponsibleUsers.Keys.ToList(),
            };

            parentFragment.ReplaceFragment(pllf, pllf.GenerateTag());
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
