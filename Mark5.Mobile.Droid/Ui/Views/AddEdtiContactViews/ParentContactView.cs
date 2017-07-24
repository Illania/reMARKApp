using System;
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class ParentContactView : AbstractSimpleFieldView
    {
        readonly Action onParentContactRequest;

        bool disableEditing;

        public ParentContactView(Context context, Action onParentContactRequest)
            : base(context, Resource.String.edit_contact_company_department, false, false)
        {
            this.onParentContactRequest = onParentContactRequest;
        }

        public override bool ContainsValidContent() => true;

        public override void RefreshView()
        {
            disableEditing = false;

            if (CreationMode == ContactCreationModeFlag.New && ParentContactPreview != null) //Add from parent
            {
                var name = ParentContactPreview.Name;
                Content = ParentContactPreview.Type == ContactType.Company ? name : $"{name} @ {ParentContactPreview.CompanyName}";
            }
            else if (CreationMode == ContactCreationModeFlag.Edit)
            {
                disableEditing = true;

                if (string.IsNullOrEmpty(ContactPreview.CompanyName))
                {
                    Visibility = Android.Views.ViewStates.Gone; //The view should be removed
                }
                else
                {
                    Content = ContactPreview.CompanyName;
                }
            }

            disableEditing |= ParentPreselected;
        }

        protected override void ContentClicked(object sender, EventArgs e)
        {
            if (disableEditing)
                return;

            onParentContactRequest();
        }
    }
}
