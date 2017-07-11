using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
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
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class AddEditContactFragment : RetainableStateFragment
    {
        static class RequestCodes
        {
            public const int ParentContactRequestCode = 111;
        }

        public Contact Contact { get; set; }
        public ContactPreview ContactPreview { get; set; }
        public Folder Folder { get; set; }
        public int? FolderId { get; set; }
        public int? ContactId { get; set; }
        public ContactType ContactType { get; set; }
        public ContactCreationModeFlag CreationModeFlag { get; set; }
        public Action CloseRequest { get; set; }
        public ContactPreview ParentContactPreview { get; set; }

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
            CommonConfig.Logger.Info($"Creating {nameof(AddEditContactFragment)} [folder.id={FolderId ?? Folder?.Id}, contact.id={ContactId ?? ContactPreview?.Id}, " +
                                     $"type={ContactType}, mode={CreationModeFlag}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            progressBar = rootView.FindViewById<ProgressBar>(Resource.Id.progress);

            fab = ((View)container.Parent.Parent).FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.SetImageResource(Resource.Drawable.action_save_contact);
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

            subviews.Union(secondarySubviews).ToList().ForEach(s => s.Edited += SubViews_Edited);

            linearLayout.AddView(showMoreButton);
            linearLayout.AddView(secondaryLinearLayout);

            return rootView;
        }

        //TODO NOTE
        // - We need to deal with the birthdate somehow
        // - On the service we didn't consider the editing of the parent contact (either we change it on the server,
        // or we make it umchangeable in edit mode)

        public override void OnStop()
        {
            base.OnStop();
            fab.Visibility = ViewStates.Gone;
        }

        protected void PrepareViewsForPerson()
        {
            //Company
            subviews.Add(new PersonNameView(Context, OnPersonNameChanged));
            subviews.Add(new ParentContactView(Context, OnParentContactRequest));
            subviews.Add(new PositionView(Context));
            subviews.Add(new EmailsView(Context));
            subviews.Add(new PhoneView(Context));
            subviews.Add(new MobileView(Context));
            subviews.ForEach(linearLayout.AddView);

            secondarySubviews.Add(new PhysicalAddressesView(Context));
            secondarySubviews.Add(new ShortIdView(Context));
            secondarySubviews.Add(new DescriptionView(Context));
            secondarySubviews.Add(new ResponsibleUsersView(Context, this));
            secondarySubviews.Add(new BirthdateView(Context));
            secondarySubviews.Add(new WebpageView(Context));
            secondarySubviews.ForEach(secondaryLinearLayout.AddView);
        }

        void PrepareViewsForDeparment()
        {
            subviews.Add(new NameView(Context, OnPersonNameChanged));
            subviews.Add(new ParentContactView(Context, OnParentContactRequest));
            subviews.Add(new EmailsView(Context));
            subviews.Add(new PhoneView(Context));
            subviews.Add(new MobileView(Context));
            subviews.ForEach(linearLayout.AddView);

            secondarySubviews.Add(new PhysicalAddressesView(Context));
            secondarySubviews.Add(new ShortIdView(Context));
            secondarySubviews.Add(new DescriptionView(Context));
            secondarySubviews.Add(new ResponsibleUsersView(Context, this));
            secondarySubviews.Add(new WebpageView(Context));

            secondarySubviews.ForEach(secondaryLinearLayout.AddView);
        }

        void PrepareViewsForCompany()
        {
            subviews.Add(new NameView(Context, OnPersonNameChanged));
            subviews.Add(new ParentContactView(Context, OnParentContactRequest));
            subviews.Add(new EmailsView(Context));
            subviews.Add(new PhoneView(Context));
            subviews.Add(new MobileView(Context));
            subviews.ForEach(linearLayout.AddView);

            secondarySubviews.Add(new PhysicalAddressesView(Context));
            secondarySubviews.Add(new ShortIdView(Context));
            secondarySubviews.Add(new DescriptionView(Context));
            secondarySubviews.Add(new ResponsibleUsersView(Context, this));
            secondarySubviews.Add(new LedgerView(Context));
            secondarySubviews.Add(new VatView(Context));
            secondarySubviews.Add(new WebpageView(Context));

            secondarySubviews.ForEach(secondaryLinearLayout.AddView);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            CommonConfig.Logger.Info($"Created {nameof(AddEditContactFragment)} [folder.id={FolderId ?? Folder?.Id}, contact.id={ContactId ?? ContactPreview?.Id}, " +
                                     $"type={ContactType}, mode={CreationModeFlag}]...");
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

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (requestCode == RequestCodes.ParentContactRequestCode && resultCode == (int)Android.App.Result.Ok)
            {
                ParentContactPreview = SerializationUtils.Deserialize<ContactPreview>(data.GetStringExtra(ParentContactSelectorFoldersListActivity.ParentContactResultKey));
                RefreshView();
            }
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
                ContactPreview.Type = ContactType;
                RefreshView();
                return;
            }
            if (CreationModeFlag == ContactCreationModeFlag.Edit) //TODO actually we never reach here (usually) 
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
                    CommonConfig.Logger.Error($"Downloading contact failed [folder.id={FolderId ?? Folder?.Id}, contact.id={ContactId ?? ContactPreview?.Id}, " +
                                     $"type={ContactType}, mode={CreationModeFlag}]...", ex);

                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                    Activity.OnBackPressed();
                }
            }
        }

        void RefreshView()
        {
            progressBar.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            foreach (var subview in subviews.Union(secondarySubviews))
            {
                subview.Contact = Contact;
                subview.ContactPreview = ContactPreview;
                subview.ParentContactPreview = ParentContactPreview;
                subview.RefreshView();
            }

            if (secondaryLayoutShown)
            {
                showMoreButton.Visibility = ViewStates.Gone;
                secondaryLinearLayout.Visibility = ViewStates.Visible;
            }

            UpdateButtonState();
        }

        #endregion

        #region Handlers

        void OnPersonNameChanged(string name)
        {
            ((AppCompatActivity)Activity).SupportActionBar.Title = name;
        }

        void OnParentContactRequest()
        {
            //TODO a company cannot have a department as parent
            var i = new Intent(Activity, typeof(ParentContactSelectorFoldersListActivity));
            StartActivityForResult(i, RequestCodes.ParentContactRequestCode);
        }

        void SubViews_Edited(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        void UpdateButtonState()
        {
            fab.Visibility = ViewStates.Visible;

            if (subviews.Union(secondarySubviews).All(s => s.ContainsValidContent()))
            {
                fab.Enabled = true;
                fab.Alpha = 1;
            }
            else
            {
                fab.Enabled = false;
                fab.Alpha = 0.6f;
            }
        }

        #endregion

        #region Actions

        async void HandleSend()
        {
            //TODO should we put a confirmation dialog before?

            var titleResource = CreationModeFlag == ContactCreationModeFlag.Edit ? Resource.String.edit_contact_edit_loading :
                                                                           Resource.String.edit_contact_add_loading;
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, titleResource, Resource.String.please_wait);

            try
            {
                var parentId = ParentContactPreview != null ? ParentContactPreview.Id : -1;

                await Managers.ContactsManager.CreteOrUpdateContactAsync(Contact, ContactPreview, parentId);

                dismissAction();

                CloseRequest?.Invoke();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while adding/editing contact [folder.id={FolderId ?? Folder?.Id}, contact.id={ContactId ?? ContactPreview?.Id}, " +
                                     $"type={ContactType}, mode={CreationModeFlag}]...", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        #endregion

        #region RetainableState 

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
                ParentContactPreview = ParentContactPreview,
                CreationModeFlag = CreationModeFlag,
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
                ParentContactPreview = state.ParentContactPreview;
                CreationModeFlag = state.CreationModeFlag;
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
            public ContactPreview ParentContactPreview { get; set; }
            public ContactCreationModeFlag CreationModeFlag { get; set; }
            public Folder Folder { get; set; }
            public int? FolderId { get; set; }
            public int? ContactId { get; set; }
            public ContactType ContactType { get; set; }
            public bool SecondaryLayoutShown { get; set; }
        }

        #endregion
    }
}
