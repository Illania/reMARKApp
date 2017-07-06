using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
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
        FloatingActionButton fab;

        List<AddEditContactView> subviews = new List<AddEditContactView>();
        List<AddEditContactView> secondarySubviews = new List<AddEditContactView>();

        AppCompatButton showMoreButton;

        bool secondaryLayoutShown;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AddEditContactFragment)} [folder.id={FolderId ?? Folder?.Id}, contact.id={ContactId ?? ContactPreview?.Id}, type={ContactType}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            progressBar = rootView.FindViewById<ProgressBar>(Resource.Id.progress);

            fab = ((View)container.Parent.Parent).FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.SetImageResource(Resource.Drawable.action_send);
            fab.SetOnClickListener(new ActionOnClickListener(HandleSend));
            fab.Enabled = false;
            fab.Alpha = 0.6f;
            fab.Visibility = ViewStates.Visible;

            subviews.Clear();
            secondarySubviews.Clear();

            showMoreButton = new AppCompatButton(Context);
            showMoreButton.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            var typedArray = Context.ObtainStyledAttributes(new int[]
            {
                Resource.Attribute.selectableItemBackground,
            });
            showMoreButton.SetBackgroundResource(typedArray.GetResourceId(0, 0));

            showMoreButton.Text = "View more";
            showMoreButton.Click += (sender, e) =>
            {
                secondaryLayoutShown = true;
                showMoreButton.Visibility = ViewStates.Gone;
                secondaryLinearLayout.Visibility = ViewStates.Visible;
            };

            secondaryLinearLayout = new LinearLayoutCompat(Context);
            secondaryLinearLayout.Orientation = LinearLayoutCompat.Vertical;
            secondaryLinearLayout.Visibility = ViewStates.Gone;
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

            linearLayout.AddView(showMoreButton);
            linearLayout.AddView(secondaryLinearLayout);

            return rootView;
        }

        protected void PrepareViewsForPerson()
        {
            //Company
            subviews.Add(new PersonNameView(Context));
            subviews.Add(new PositionView(Context));
            subviews.Add(new EmailsView(Context));
            subviews.Add(new PhoneView(Context));
            subviews.Add(new MobileView(Context));
            subviews.ForEach(linearLayout.AddView);

            secondarySubviews.Add(new ShortIdView(Context));
            secondarySubviews.Add(new PhysicalAddressesView(Context));
            secondarySubviews.Add(new DescriptionView(Context));
            secondarySubviews.Add(new ResponsibleUsersView(Context, this));
            secondarySubviews.Add(new BirthdateView(Context));
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

            CommonConfig.Logger.Info($"Created {nameof(AddEditContactFragment)} [folder.id={FolderId ?? Folder?.Id}, contact.id={ContactId ?? ContactPreview?.Id}, type={ContactType}]...");
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();
        }

        public void ReplaceFragment(Fragment f, string tag)
        {
            var fragmentManager = ((AppCompatActivity)Activity).SupportFragmentManager;

            fragmentManager.BeginTransaction()
                           .SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right)
                           .Replace(Resource.Id.fragment_container, f, tag)
                           .AddToBackStack(tag).Commit();
        }

        #region Refresh methods

        async Task RefreshData()
        {
            if (Contact != null && ContactPreview != null)
            {
                RefreshView();
                return;
            }
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

            foreach (var subview in subviews.Union(secondarySubviews))
            {
                subview.Contact = Contact;
                subview.ContactPreview = ContactPreview;
                subview.RefreshView();
            }

            if (secondaryLayoutShown)
            {
                showMoreButton.Visibility = ViewStates.Gone;
                secondaryLinearLayout.Visibility = ViewStates.Visible;
            }
        }

        void RefreshTitle() //TODO should be updated as the information are inserted
        {
            var name = ContactType == ContactType.Person ? string.Join(" ", Contact.FirstName, Contact.Patronymic, Contact.LastName) : ContactPreview.CompanyName; //TODO check if works also for deparments
            ((AppCompatActivity)Activity).SupportActionBar.Title = name;
        }

        #endregion

        void HandleSend()
        {
            throw new NotImplementedException();
        }

        #region RetainableState 

        //TODO need to add parent and check all

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
                SecondaryLayoutShown = secondaryLayoutShown,
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
                secondaryLayoutShown = state.SecondaryLayoutShown;
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
            public bool SecondaryLayoutShown { get; set; }
        }

        #endregion
    }
}
