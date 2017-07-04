using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class ResponsibleUsersView : AbstractMultipleRowsView<Dictionary<int, string>>
    {
        AddEditContactFragment parentFragment;

        public ResponsibleUsersView(Context context, AddEditContactFragment parentFragment)
            : base(context, Resource.String.edit_contact_responsible, true)
        {
            this.parentFragment = parentFragment;
        }

        public override void RefreshView()
        {
            if (Contact.ResponsibleUsers.Any())
            {
                AddRow(Contact.ResponsibleUsers);
            }
        }

        protected override void AddButton_Click(object sender, EventArgs e)
        {
            var pllf = new ResponsibleSelectionFragment
            {
                CloseRequest = HandleAction,
            };

            parentFragment.ReplaceFragment(pllf, pllf.GenerateTag());
        }

        void HandleAction(Dictionary<int, string> obj)
        {
            AddRow(obj);
        }

        protected override Row GetNewRow()
        {
            return new ResponsibleRow(Context, this);
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            Contact.ResponsibleUsers.Clear();
            Contact.ResponsibleUserIds.Clear();
            RemoveRow(sender as ResponsibleRow);
        }

        protected class ResponsibleRow : Row
        {
            readonly AppCompatEditText responsibleEditText;

            public ResponsibleRow(Context context, ResponsibleUsersView responsibleUsersView)
                : base(context, responsibleUsersView)
            {
                responsibleEditText = new AppCompatEditText(context);

                var editTextLp = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1.0f)
                {
                    Gravity = (int)GravityFlags.CenterVertical,
                };

                responsibleEditText.Focusable = false;
                responsibleEditText.KeyListener = null;
                responsibleEditText.SetHint(Resource.String.edit_contact_responsible);
                responsibleEditText.Click += ResponsibleEditText_Click;
                Layout.AddView(responsibleEditText, 0, editTextLp);
            }

            void ResponsibleEditText_Click(object sender, EventArgs e)
            {

            }

            public override void UpdateRow() //TODO need to remove content null check on others
            {
                responsibleEditText.Text = string.Join(", ", Content.Values);
            }

            public override bool ContainsValidContent() => true;

        }
    }
}
