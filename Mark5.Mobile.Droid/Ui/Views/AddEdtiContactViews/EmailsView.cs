using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class EmailsView : AbstractMultipleRowsView<CommunicationAddress>
    {
        public EmailsView(Context context)
            : base(context, Resource.String.edit_contact_email)
        {
        }

        public override void RefreshView()
        {
            Clear();

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

            OnContentChanged();
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
            emailEditText.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
            emailEditText.TextChanged += (sender, e) =>
            {
                emailEditText.Error = !Validator.IsEmailValid(emailEditText.Text) ? Context.GetString(Resource.String.email_not_valid) : null;
            };
            emailEditText.InputType = InputTypes.TextVariationEmailAddress | InputTypes.ClassText;
            container.AddView(emailEditText);

            var descriptionEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            descriptionEditText.SetHint(Resource.String.edit_contact_description);
            descriptionEditText.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
            descriptionEditText.InputType = InputTypes.TextFlagMultiLine
                   | InputTypes.TextFlagCapSentences
                | InputTypes.TextFlagAutoCorrect | InputTypes.ClassText;
            container.AddView(descriptionEditText);

            var thirdLine = new LinearLayoutCompat(Context)
            {
                Orientation = Horizontal,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    TopMargin = DistanceVerySmall,
                }
            };
            container.AddView(thirdLine);

            var preferableTextView = new AppCompatTextView(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = DistanceVerySmall
                }
            };
            preferableTextView.SetText(Resource.String.edit_contact_mark_as_preferable);
            preferableTextView.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
            thirdLine.AddView(preferableTextView);

            var preferableCheckBox = new AppCompatCheckBox(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            };
            thirdLine.AddView(preferableCheckBox);

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
                ca.Type = CommunicationAddressType.Email;

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

                OnContentChanged();
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
                ((EmailsView)ParentView).CreateDialog(this);
            }

            override public void UpdateRow()
            {
                emailEditText.Text = Content.Address;
                emailEditText.Error = !Validator.IsEmailValid(Content.Address) ? Context.GetString(Resource.String.email_not_valid) : null;
                emailEditText.SetTextAppearanceCompat(Context, Content.IsPrimary ? Resource.Style.fontPrimaryBold : Resource.Style.fontPrimary);
            }

            public override bool ContainsValidContent()
            {
                return Validator.IsEmailValid(Content.Address);
            }
        }
    }
}
