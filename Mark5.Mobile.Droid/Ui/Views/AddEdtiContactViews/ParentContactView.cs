using System;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class ParentContactView : AbstractSimpleFieldView
    {
        private Action onParentContactRequest;

        public ParentContactView(Context context, Action onParentContactRequest)
            : base(context, Resource.String.edit_contact_company_department, false, false)
        {
            this.onParentContactRequest = onParentContactRequest;
        }

        public override void RefreshView()
        {
            if (ParentContactPreview != null)
            {
                var name = ParentContactPreview.Name;
                Content = ParentContactPreview.Type == Mobile.Common.Model.ContactType.Company ? name : $"{name} @ {ParentContactPreview.CompanyName}";
            }
        }

        protected override void ContentClicked(object sender, EventArgs e)
        {
            onParentContactRequest();
        }
    }
}
