using System;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;

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

        public override void UpdateContact()
        {
            throw new NotImplementedException();
        }

        protected override Row GetNewRow(CommunicationAddress content)
        {
            return new EmailRow(Context, content);
        }

        protected class EmailRow : Row
        {
            readonly AppCompatEditText emailEditText;
            readonly AppCompatEditText descriptionEditText;

            CommunicationAddress address;

            public EmailRow(Context context, CommunicationAddress address)
                : base(context, address)
            {
                this.address = address;

                var container = new LinearLayoutCompat(context)
                {
                    Orientation = Vertical,
                    LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f)
                };

                Layout.AddView(container, 0);

                emailEditText = new AppCompatEditText(context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                };
                emailEditText.SetHint(Resource.String.edit_contact_address);
                container.AddView(emailEditText);

                descriptionEditText = new AppCompatEditText(context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                };
                descriptionEditText.SetHint(Resource.String.edit_contact_description);
                container.AddView(descriptionEditText);

                UpdateText();
            }

            void UpdateText()
            {
                if (address != null)
                {
                    emailEditText.Text = address.Address;
                    descriptionEditText.Text = address.Description;
                }
            }

            public override CommunicationAddress GetContent()
            {
                return null; //TODO correct
            }

            public override bool ContainsValidContent()
            {
                throw new NotImplementedException();
            }
        }
    }
}
