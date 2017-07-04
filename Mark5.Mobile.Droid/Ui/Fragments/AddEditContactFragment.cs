using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class AddEditContactFragment : RetainableStateFragment
    {
        public Contact Contact { get; set; }
        public ContactPreview ContactPreview { get; set; }
        public Folder Folder { get; set; }
        public int? FolderId { get; set; }
        public int? ContactId { get; set; }
        public ContactType ContactType { get; set; }
        public ContactCreationModeFlag CreationModeFlag { get; set; }
        public Action CloseRequest { get; set; }

        LinearLayoutCompat linearLayout;
        LinearLayoutCompat secondaryLinearLayout;
        ProgressBar progressBar;
        ScrollView scrollView;

        List<AddEditContactView> subviews = new List<AddEditContactView>();
        List<AddEditContactView> secondarySubviews = new List<AddEditContactView>();

        AppCompatButton viewMoreButton;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AddEditContactFragment)} [folder.id={FolderId ?? Folder?.Id}, contact.id={ContactId ?? ContactPreview?.Id}, type={ContactType}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            progressBar = rootView.FindViewById<ProgressBar>(Resource.Id.progress);

            viewMoreButton = new AppCompatButton(Context);
            viewMoreButton.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            viewMoreButton.Text = "View more";
            viewMoreButton.Click += (sender, e) =>
            {
                viewMoreButton.Visibility = ViewStates.Gone;
                secondaryLinearLayout.Visibility = ViewStates.Visible;
            };

            secondaryLinearLayout = new LinearLayoutCompat(Context);
            secondaryLinearLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            switch (ContactType)
            {
                case ContactType.Person:
                    PrepareViewsForPerson();
                    break;
                case ContactType.Department:
                    PrepareViewsForDeparment();
                    break;
                case ContactType.Company:
                    PrepareViewsForCompany();
                    break;
                default:
                    throw new ArgumentException("Contact type needs to be defined");
            }

            return rootView;
        }

        protected void PrepareViewsForPerson()
        {
            subviews.Add(new FirstNameView(Context));
            subviews.Add(new MiddleNameView(Context));
            subviews.Add(new LastNameView(Context));
            subviews.Add(new EmailsView(Context));
            subviews.Add(new PhoneView(Context));
            subviews.ForEach(linearLayout.AddView);

            secondarySubviews.Add(new BirthdateView(Context));
            secondarySubviews.Add(new DescriptionView(Context));
            secondarySubviews.ForEach(secondaryLinearLayout.AddView);
        }

        void PrepareViewsForDeparment()
        {

        }

        void PrepareViewsForCompany()
        {

        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = null;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(AddEditContactFragment)} [folder.id={FolderId ?? Folder?.Id}, contact.id={ContactId ?? ContactPreview?.Id}, type={ContactType}]...");
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();
        }

        #region Refresh methods

        async Task RefreshData()
        {
            if (CreationModeFlag == ContactCreationModeFlag.New)
            {
                Contact = new Contact();
                ContactPreview = new ContactPreview();
                RefreshView();
                return;
            }
            if (CreationModeFlag == ContactCreationModeFlag.Edit)
            {
                try
                {
                    if (ContactId.HasValue && ContactPreview == null && Contact == null)
                    {
                        var container = await Managers.ContactsManager.GetContactWithPreviewAsync(FolderId ?? Folder?.Id, ContactId.Value);
                        ContactPreview = container.ContactPreview;
                        Contact = container.Contact;
                    }

                    if (ContactPreview != null && Contact == null)
                        Contact = await Managers.ContactsManager.GetContactAsync(FolderId ?? Folder?.Id, ContactPreview.Id);

                    RefreshView();
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Downloading contact failed [folder.name={Folder?.Name}, folder.id={FolderId ?? Folder?.Id}, contactId={ContactId ?? ContactPreview?.Id}]", ex);

                    await Dialogs.ShowErrorDialogAsync(Activity, ex);

                    CloseRequest?.Invoke();
                }
            }
        }

        void RefreshView()
        {
            RefreshTitle();

            progressBar.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            foreach (var subview in subviews)
            {
                linearLayout.AddView(subview);
                subview.Contact = Contact;
                subview.ContactPreview = ContactPreview;
                subview.RefreshView();
            }
        }

        void RefreshTitle() //TODO should be updated as the information are inserted
        {
            var name = ContactType == ContactType.Person ? string.Join(" ", Contact.FirstName, Contact.Patronymic, Contact.LastName) : ContactPreview.CompanyName; //TODO check if works also for deparments
            ((AppCompatActivity)Activity).SupportActionBar.Title = name;
        }

        #endregion

        #region RetainableState 

        //TODO need to add parent 

        public override string GenerateTag()
        {
            return $"{nameof(AddEditContactFragment)}";
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new AddEditContactFragmentState
            {
                ContactPreview = ContactPreview,
                Contact = Contact,
                Folder = Folder,
                FolderId = FolderId,
                ContactId = ContactId,
                ContactType = ContactType,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            if (restoredState is AddEditContactFragmentState state)
            {
                Contact = state.Contact;
                ContactPreview = state.ContactPreview;
                Folder = state.Folder;
                FolderId = state.FolderId;
                ContactId = state.ContactId;
                ContactType = state.ContactType;
            }
        }

        class AddEditContactFragmentState : IRetainableState
        {
            public Contact Contact { get; set; }
            public ContactPreview ContactPreview { get; set; }
            public Folder Folder { get; set; }
            public int? FolderId { get; set; }
            public int? ContactId { get; set; }
            public ContactType ContactType { get; set; }
        }

        #endregion
    }
}
