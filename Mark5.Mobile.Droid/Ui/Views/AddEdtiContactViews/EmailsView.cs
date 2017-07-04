using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class EmailsView : AbstractMultipleRowsView<CommunicationAddress>
    {
        public EmailsView(Context context)
            : base(context, Resource.String.edit_contact_email, false)
        {
        }

        public override void RefreshView()
        {
            var addresses = Contact.CommunicationAddresses.Where(a => a.Type == CommunicationAddressType.Email);
            foreach (var address in addresses)
            {
                AddRow(address);
            }
        }

        protected override Row GetNewRow()
        {
            return new EmailRow(Context, this);
        }

        protected override void AddButton_Click(object sender, EventArgs e)
        {
            CreateDialog();
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            var row = sender as EmailRow;
            var ca = row.GetContent();
            Contact.CommunicationAddresses.Remove(ca);
            RemoveRow(row);
        }

        async void CreateDialog(EmailRow row = null)
        {
            CommunicationAddress ca = null;

            var container = new LinearLayoutCompat(Context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            };

            var emailEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            emailEditText.SetHint(Resource.String.edit_contact_address);
            container.AddView(emailEditText);

            var descriptionEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            descriptionEditText.SetHint(Resource.String.edit_contact_description);
            container.AddView(descriptionEditText);

            var preferableCheckBox = new AppCompatCheckBox(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            preferableCheckBox.SetText(Resource.String.edit_contact_mark_as_preferable);
            container.AddView(preferableCheckBox);

            if (row != null)
            {
                ca = row.GetContent();
                emailEditText.Text = ca.Address;
                descriptionEditText.Text = ca.Description;
                preferableCheckBox.Checked = ca.IsPrimary;
            }

            if (await Dialogs.ShowCustomViewDialogAsync(Context, Resource.String.edit_contact_email, container) == true)
            {
                ca = ca ?? new CommunicationAddress();
                ca.Address = emailEditText.Text;
                ca.Description = descriptionEditText.Text;
                ca.IsPrimary = preferableCheckBox.Checked;

                if (ca.IsPrimary)
                {
                    SetIsPrimaryOnOtherRows(row);
                }

                if (row == null)
                {
                    AddRow(ca);
                    Contact.CommunicationAddresses.Add(ca);
                }
                else
                {
                    row.SetContent(ca);
                }
            }
        }

        void SetIsPrimaryOnOtherRows(EmailRow primaryAddressRow)
        {
            foreach (var row in Rows)
            {
                if (row != primaryAddressRow)
                {
                    var emailRow = row as EmailRow;
                    emailRow.GetContent().IsPrimary = false;
                    emailRow.UpdateRow();
                }
            }
        }

        protected class EmailRow : Row
        {
            readonly AppCompatEditText emailEditText;

            public EmailRow(Context context, EmailsView emailsView)
                : base(context, emailsView)
            {
                emailEditText = new AppCompatEditText(context)
                {
                    LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f),
                    KeyListener = null,
                    Focusable = false,
                };
                emailEditText.SetHint(Resource.String.edit_contact_address);
                emailEditText.Click += EmailEditText_Click;
                Layout.AddView(emailEditText, 0);
            }

            void EmailEditText_Click(object sender, EventArgs e)
            {
                (ParentView as EmailsView).CreateDialog(this);
            }

            override public void UpdateRow()
            {
                if (Content != null)
                {
                    emailEditText.Text = Content.Address;
                    Layout.SetBackgroundColor(Content.IsPrimary ? Color.BlanchedAlmond : Color.White);
                }
            }

            public override bool ContainsValidContent()
            {
                throw new NotImplementedException();
            }
        }
    }
}
