using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.AddEditShortcodeViews
{
    public class EntryView : AbstractMultipleRowsView<DocumentAddress>
    {
        DocumentAddressType addressType;
        Action<DocumentAddressType> onContactAddressRequest;

        public EntryView(Context context, DocumentAddressType addressType, Action<DocumentAddressType> onContactAddressRequest)
            : base(context, ResourceIdForType(addressType))
        {
            this.addressType = addressType;
            this.onContactAddressRequest = onContactAddressRequest;
        }

        static int ResourceIdForType(DocumentAddressType type)
        {
            switch (type)
            {
                case DocumentAddressType.To:
                    return Resource.String.to;
                case DocumentAddressType.Cc:
                    return Resource.String.cc;
                case DocumentAddressType.Bcc:
                    return Resource.String.bcc;
                default:
                    throw new ArgumentException("Invalid type");
            }
        }

        public override void RefreshView()
        {
            Clear();

            var addresses = Shortcode.Addresses.Where(
                a => a.Type == CommunicationAddressType.Email && a.AddressType == addressType);
            foreach (var address in addresses)
            {
                AddRow(address);
            }
        }

        protected async override void AddButton_Click(object sender, EventArgs e)
        {
            var strings = new List<string> {Context.GetString(Resource.String.yes),
                Context.GetString(Resource.String.no)}.ToArray();

            var result = await Dialogs.ShowListDialog(Context, Resource.String.edit_shortcode_add_from_contact_question,
                                                      strings, true);

            if (result == 0)
            {
                onContactAddressRequest?.Invoke(addressType);
            }
            if (result == 1)
            {
                CreateDialog();
            }
        }

        public void AddEntry(Recipient recipient)
        {
            var da = new DocumentAddress
            {
                Type = CommunicationAddressType.Email,
                AddressType = addressType,
                Address = recipient.Address, //TODO as in iOS, should we add more?
            };

            AddRow(da);
        }

        protected override Row GetNewRow()
        {
            return new EntryRow(Context, this);
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            var row = sender as EntryRow;
            var ca = row.GetContent();
            Shortcode.Addresses.Remove(ca);
            RemoveRow(row);
        }

        async void CreateDialog(EntryRow row = null)
        {
            DocumentAddress da = null;

            var container = new LinearLayoutCompat(Context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            };

            var emailEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            emailEditText.SetHint(Resource.String.edit_shortcode_address);
            emailEditText.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
            emailEditText.InputType = InputTypes.TextVariationEmailAddress | InputTypes.ClassText;
            container.AddView(emailEditText);

            var nameEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            nameEditText.SetHint(Resource.String.edit_shortcode_name);
            nameEditText.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
            nameEditText.InputType = InputTypes.TextFlagCapSentences | InputTypes.ClassText; //TODO check if correct
            container.AddView(nameEditText);

            var attentionEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            attentionEditText.SetHint(Resource.String.edit_shortcode_attention);
            attentionEditText.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
            attentionEditText.InputType = InputTypes.TextFlagCapSentences | InputTypes.ClassText;
            container.AddView(attentionEditText);

            string oldAttention = string.Empty;

            if (row != null)
            {
                da = row.GetContent();
                emailEditText.Text = da.Address;
                nameEditText.Text = da.Name;
                attentionEditText.Text = oldAttention = da.FullAttention;
            }

            Func<bool> isContentValid = () =>
            {
                var isValid = Validator.IsEmailValid(emailEditText.Text);

                if (!isValid)
                {
                    emailEditText.Error = Context.GetString(Resource.String.edit_contact_invalid_email);
                    emailEditText.TextChanged -= EmailEditText_TextChanged;
                    emailEditText.TextChanged += EmailEditText_TextChanged;
                }

                return isValid;
            };

            if (await Dialogs.ShowCustomViewDialogWithValidityAsync(Context, Resource.String.edit_contact_email, container, isContentValid) == true)
            {
                da = da ?? new DocumentAddress();
                da.Type = CommunicationAddressType.Email;
                da.AddressType = addressType;
                da.Address = emailEditText.Text;
                da.Name = nameEditText.Text;

                if (attentionEditText.Text != oldAttention)
                    da.Attention = attentionEditText.Text;

                if (row == null)
                {
                    AddRow(da);
                    Shortcode.Addresses.Add(da);
                }
                else
                {
                    row.SetContent(da);
                }
            }
        }

        void EmailEditText_TextChanged(object sender, TextChangedEventArgs e)
        {
            var emailEditText = (AppCompatEditText)sender;
            emailEditText.Error = !Validator.IsEmailValid(emailEditText.Text)
                ? Context.GetString(Resource.String.edit_contact_invalid_email) : null;
        }

        protected class EntryRow : Row
        {
            readonly AppCompatEditText emailEditText;

            public EntryRow(Context context, EntryView entryView)
                : base(context, entryView)
            {
                emailEditText = new AppCompatEditText(context)
                {
                    LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f),
                    KeyListener = null,
                    Focusable = false,
                };
                emailEditText.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
                emailEditText.SetHint(Resource.String.edit_contact_address);
                emailEditText.Click += EmailEditText_Click;
                Layout.AddView(emailEditText, 0);
            }

            void EmailEditText_Click(object sender, EventArgs e)
            {
                ((EntryView)ParentView).CreateDialog(this);
            }

            override public void UpdateRow()
            {
                emailEditText.Text = Content.Address;
                emailEditText.Error = !Validator.IsEmailValid(Content.Address) ? Context.GetString(Resource.String.edit_contact_invalid_email) : null;
            }
        }
    }
}
