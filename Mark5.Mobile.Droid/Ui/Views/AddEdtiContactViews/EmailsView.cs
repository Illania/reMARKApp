using System;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public abstract class EmailsView : AbstractMultipleRowsView<CommunicationAddress>
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

        async void CreateDialog(CommunicationAddress ca = null, EmailRow row = null) //TODO we could also pass only the row
        {
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

            if (ca != null)
            {
                emailEditText.Text = ca.Address;
                descriptionEditText.Text = ca.Description;
            }

            container.AddView(descriptionEditText);

            if (await Dialogs.ShowCustomViewDialogAsync(Context, Resource.String.edit_contact_email, container) == true)
            {
                ca = ca ?? new CommunicationAddress();
                ca.Address = emailEditText.Text;
                ca.Description = descriptionEditText.Text;

                if (row == null)
                {
                    AddRow(ca);
                }
                else
                {
                    row.SetContent(ca);
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
                (ParentView as EmailsView).CreateDialog(Content, this);
            }

            override protected void UpdateRow()
            {
                if (Content != null)
                {
                    emailEditText.Text = Content.Address;
                }
            }

            public override bool ContainsValidContent()
            {
                throw new NotImplementedException();
            }
        }
    }
}
