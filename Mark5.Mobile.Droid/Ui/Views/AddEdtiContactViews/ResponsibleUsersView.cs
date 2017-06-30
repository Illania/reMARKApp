using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class ResponsibleUsersView : AbstractMultipleRowsView<int>
    {
        List<SystemUser> users;

        public ResponsibleUsersView(Context context, List<SystemUser> users)
            : base(context, Resource.String.edit_contact_responsible, false)
        {
            this.users = users;
        }

        public override void RefreshView()
        {
            foreach (var id in Contact.ResponsibleUserIds)
            {
                AddRow(id);
            }
        }

        public override void UpdateContact()
        {
            throw new NotImplementedException();
        }


        protected override Row GetNewRow(int content)
        {
            return new ResponsibleRow(Context, content, users);
        }

        protected class ResponsibleRow : Row
        {
            readonly AppCompatEditText responsibleEditText;

            List<SystemUser> users;
            SystemUser selectedUser;

            public ResponsibleRow(Context context, int responsibleId, List<SystemUser> users)
                : base(context, responsibleId)
            {
                this.users = users;
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

                if (Content > 0)
                {
                    selectedUser = users.FirstOrDefault(s => s.Id == Content);
                }

                UpdateText();
            }

            async void ResponsibleEditText_Click(object sender, EventArgs e)
            {
                var index = await Dialogs.ShowListDialog(Context, Resource.String.edit_contact_responsible, users.Select(u => u.Username).ToArray(), true);
                if (index >= 0)
                {
                    selectedUser = users[index];
                    UpdateText();
                }
            }

            void UpdateText()
            {
                if (selectedUser != null)
                {
                    responsibleEditText.Text = selectedUser.Username;
                }
            }

            public override int GetContent()
            {
                throw new NotImplementedException();
            }

            public override bool ContainsValidContent() => true;
        }
    }
}
