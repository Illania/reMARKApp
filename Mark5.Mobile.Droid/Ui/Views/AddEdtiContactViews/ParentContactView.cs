using System;
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class ParentContactView : AbstractSimpleFieldView
    {
        readonly Action onParentContactRequest;
        readonly Action onParentContactRemoved;


        bool disableEditing;

        public ParentContactView(Context context, Action onParentContactRequest, Action onParentContactRemoved)
            : base(context, floatingHint: false, editable: false)
        {
            this.onParentContactRequest = onParentContactRequest;
            this.onParentContactRemoved = onParentContactRemoved;
        }

        public override void RefreshView()
        {
            int hintResId = -1;

            Content = string.Empty;

            switch (ContactPreview.Type)
            {
                case ContactType.Person:
                    hintResId = Resource.String.edit_contact_company_department;
                    break;
                case ContactType.Department:
                    hintResId = Resource.String.edit_contact_company;
                    break;
                default:
                    throw new ArgumentException("This view should not be visible for companies!");
            }

            SetHintResId(hintResId);

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

            if (!string.IsNullOrEmpty(Content) && !disableEditing)
            {
                AddDeleteButton();
            }
        }

        protected override void ContentClicked(object sender, EventArgs e)
        {
            if (disableEditing)
                return;

            onParentContactRequest();
        }

        protected override void DeleteButtonClicked(object sender, EventArgs e)
        {
            RemoveDeleteButton();
            ParentContactPreview = null;
            onParentContactRemoved();
            RefreshView();
        }
    }
}
