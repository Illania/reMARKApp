using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Common.HubMessages;
using Mark5.Mobile.Droid.Ui.Views.AddEditContactViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class AddEditContactFragment : RetainableStateFragment
    {
        const string ContactBundleKey = "Contact_e06436ef-89bb-44b0-9419-3131796d126b";
        const string ContactPreviewBundleKey = "ContactPreview_d09e0cb6-e224-4327-8d09-43ce921f53c6";
        const string ContactIdBundleKey = "ContactId_0ce4ab20-2835-4cda-a724-68b635c70438";
        const string ContactTypeBundleKey = "ContactType_cbec87c5-03a4-421e-a043-33d416faaf51";
        const string CreationModeFlagBundleKey = "CreationModeFlag_ab9071da-34f6-45fc-9a03-a0b348814dcd";
        const string ParentContactPreviewBundleKey = "ParentContactPreview_a2a2d7c1-b571-4aef-8f7e-7c4348ba8c47";
        const string ParentPreselectedBundleKey = "ParentPreselected_6a214835-2ade-4503-a9b0-163046ac394e";

        Contact contact;
        ContactPreview contactPreview;
        int? contactid;
        ContactType contactType;
        ContactCreationModeFlag creationModeFlag;
        ContactPreview parentContactPreview;
        bool parentPreselected;
 
        LinearLayoutCompat linearLayout;
        LinearLayoutCompat secondaryLinearLayout;
        ProgressBar progressBar;
        ScrollView scrollView;
        FloatingActionButton fab;

        PersonNameView personNameView;
        NameView nameView;
        ParentContactView parentContactView;

        List<AddEditContactView> subviews = new List<AddEditContactView>();
        List<AddEditContactView> secondarySubviews = new List<AddEditContactView>();

        AppCompatButton showMoreButton;

        bool secondaryLayoutShown;

        public static (AddEditContactFragment fragment, string tag) NewInstance(Contact contact, ContactPreview contactPreview, int? contactId, ContactType? contactType, ContactCreationModeFlag? creationModeFlag,
                                                                                ContactPreview parentContactPreview, bool? parentPreselected)
        {
            Bundle args = new Bundle();

            if (contact != null)
                args.PutString(ContactBundleKey, Serializer.Serialize(contact));

            if (contactPreview != null)
                args.PutString(ContactPreviewBundleKey, Serializer.Serialize(contactPreview));
            
            if (contactId != null)
                args.PutString(ContactIdBundleKey, Serializer.Serialize(contactId));
            
            if (contactType != null)
                args.PutString(ContactTypeBundleKey, Serializer.Serialize(contactType));

            if (creationModeFlag != null)
                args.PutString(CreationModeFlagBundleKey, Serializer.Serialize(creationModeFlag));

            if (parentContactPreview != null)
                args.PutString(ContactPreviewBundleKey, Serializer.Serialize(parentContactPreview));

            if (parentPreselected != null)
                args.PutString(ParentPreselectedBundleKey, Serializer.Serialize(parentPreselected));

            var fragment = new AddEditContactFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(AddEditContactFragment)}";

            return (fragment, tag);  
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            //TODO: Get arguments
            CommonConfig.Logger.Info($"Creating {nameof(AddEditContactFragment)} [contact.id={contactid ?? contactPreview?.Id}, " +
                                     $"type={contactType}, mode={creationModeFlag}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            linearLayout.DescendantFocusability = DescendantFocusability.BeforeDescendants;
            linearLayout.FocusableInTouchMode = true;

            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            progressBar = rootView.FindViewById<ProgressBar>(Resource.Id.progress);

            fab = ((View)container.Parent.Parent).FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.SetImageResource(Resource.Drawable.action_save_contact);
            fab.SetOnClickListener(new ActionOnClickListener(HandleSend));
            fab.Enabled = true;
            fab.Size = FloatingActionButton.SizeNormal;
            fab.Visibility = ViewStates.Visible;

            subviews.Clear();
            secondarySubviews.Clear();

            showMoreButton = new AppCompatButton(Context);
            showMoreButton.LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = (int)GravityFlags.CenterHorizontal
            };
            var typedArray = Context.ObtainStyledAttributes(new int[]
            {
                Resource.Attribute.selectableItemBackground,
            });
            showMoreButton.SetBackgroundResource(typedArray.GetResourceId(0, 0));
            showMoreButton.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));
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

            var bottomMargin = ((CoordinatorLayout.LayoutParams)fab.LayoutParameters).BottomMargin;
            var fabHeight = Conversion.ConvertDpToPixels(56);
            linearLayout.SetPadding(linearLayout.PaddingLeft, linearLayout.PaddingTop, linearLayout.PaddingRight, fabHeight + bottomMargin * 2);

            switch (contactType)
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

            SetTitle();

            return rootView;
        }

        void SetTitle()
        {
            int resId = 0;

            switch (contactType)
            {
                case ContactType.Person:
                    resId = Resource.String.edit_contact_create_person;
                    break;
                case ContactType.Company:
                    resId = Resource.String.edit_contact_create_company;
                    break;
                case ContactType.Department:
                    resId = Resource.String.edit_contact_create_department;
                    break;
                default:
                    throw new ArgumentException("Contact type needs to be defined");
            }

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(resId);

        }

        public override void OnStop()
        {
            base.OnStop();
            fab.Visibility = ViewStates.Gone;
        }

        protected void PrepareViewsForPerson()
        {
            personNameView = new PersonNameView(Context);
            parentContactView = new ParentContactView(Context, OnParentContactRequest, OnParentContactRemoved);

            subviews.Add(personNameView);
            subviews.Add(parentContactView);
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
            nameView = new NameView(Context);
            parentContactView = new ParentContactView(Context, OnParentContactRequest, OnParentContactRemoved);

            subviews.Add(nameView);
            subviews.Add(parentContactView);
            subviews.Add(new EmailsView(Context));
            subviews.Add(new PhoneView(Context));
            subviews.Add(new MobileView(Context));
            subviews.ForEach(linearLayout.AddView);

            secondarySubviews.Add(new PhysicalAddressesView(Context));
            secondarySubviews.Add(new ShortIdView(Context));
            secondarySubviews.Add(new DescriptionView(Context));
            secondarySubviews.Add(new ResponsibleUsersView(Context, this));
            secondarySubviews.Add(new AccountView(Context));
            secondarySubviews.Add(new WebpageView(Context));

            secondarySubviews.ForEach(secondaryLinearLayout.AddView);
        }

        void PrepareViewsForCompany()
        {
            nameView = new NameView(Context);

            subviews.Add(nameView);
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
            secondarySubviews.Add(new AccountView(Context));
            secondarySubviews.Add(new WebpageView(Context));

            secondarySubviews.ForEach(secondaryLinearLayout.AddView);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            CommonConfig.Logger.Info($"Created {nameof(AddEditContactFragment)} [contact.id={contactid ?? contactPreview?.Id}, " +
                                     $"type={contactType}, mode={creationModeFlag}]...");
        }

        public override void OnResume()
        {
            base.OnResume();

            fab.Enabled = true;
            fab.Visibility = ViewStates.Visible;

            RefreshData();
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
                parentContactPreview = Serializer.Deserialize<ContactPreview>(data.GetStringExtra(ParentContactSelectorFoldersListActivity.ParentContactResultKey));
                RefreshView();
            }
        }

        #region Refresh methods

        void RefreshData()
        {
            if (contact != null && contactPreview != null)
            {
                RefreshView();
                return;
            }

            if (creationModeFlag == ContactCreationModeFlag.New)
            {
                contact = new Contact();
                contactPreview = new ContactPreview();
                contactPreview.Type = contactType;
                RefreshView();
                return;
            }
        }

        void RefreshView()
        {
            progressBar.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            foreach (var subview in subviews.Union(secondarySubviews))
            {
                subview.Contact = contact;
                subview.ContactPreview = contactPreview;
                subview.ParentContactPreview = parentContactPreview;
                subview.CreationMode = creationModeFlag;
                subview.ParentPreselected = parentPreselected;
                subview.RefreshView();
            }

            if (secondaryLayoutShown)
            {
                showMoreButton.Visibility = ViewStates.Gone;
                secondaryLinearLayout.Visibility = ViewStates.Visible;
            }
        }

        #endregion

        #region Handlers

        void OnParentContactRequest()
        {
            var i = new Intent(Activity, typeof(ParentContactSelectorFoldersListActivity));
            i.PutExtra(ParentContactSelectorFoldersListActivity.ChildrenTypeIntentKey, (int)contactPreview.Type);
            StartActivityForResult(i, RequestCodes.ParentContactRequestCode);
        }

        void OnParentContactRemoved()
        {
            parentContactPreview = null;
        }

        #endregion

        #region Actions

        async void HandleSend()
        {
            if (personNameView != null && !personNameView.ContainsValidContent())
            {
                personNameView.ShowError();
                return;
            }

            if (nameView != null && !nameView.ContainsValidContent())
            {
                nameView.ShowError();
                return;
            }

            if (parentContactView != null && !parentContactView.ContainsValidContent())
            {
                parentContactView.ShowError();
                return;
            }

            var titleResource = creationModeFlag == ContactCreationModeFlag.Edit ? Resource.String.edit_contact_edit_loading : Resource.String.edit_contact_add_loading;
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, titleResource, Resource.String.please_wait);

            try
            {
                var parentId = parentContactPreview != null ? parentContactPreview.Id : -1;

                await Managers.ContactsManager.CreteOrUpdateContactAsync(contact, contactPreview, parentId);

                dismissAction();

                if (creationModeFlag == ContactCreationModeFlag.Edit)
                    CommonConfig.MessengerHub.Publish(new ContactPreviewChanged(this, contactPreview));
                
                Activity?.OnBackPressed();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while adding/editing contact [contact.id={contactid ?? contactPreview?.Id}, " +
                                     $"type={contactType}, mode={creationModeFlag}]...", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        #endregion

        #region RetainableState 

        public override IRetainableState OnRetainInstanceState()
        {
            return new AddEditContactFragmentState
            {
                ContactPreview = contactPreview,
                Contact = contact,
                ParentContactPreview = parentContactPreview,
                CreationModeFlag = creationModeFlag,
                ContactId = contactid,
                ContactType = contactType,
                SecondaryLayoutShown = secondaryLayoutShown,
                ParentPreselected = parentPreselected,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            if (restoredState is AddEditContactFragmentState state)
            {
                contact = state.Contact;
                contactPreview = state.ContactPreview;
                parentContactPreview = state.ParentContactPreview;
                creationModeFlag = state.CreationModeFlag;
                contactid = state.ContactId;
                contactType = state.ContactType;
                secondaryLayoutShown = state.SecondaryLayoutShown;
                parentPreselected = parentPreselected;
            }
        }

        class AddEditContactFragmentState : IRetainableState
        {
            public Contact Contact { get; set; }
            public ContactPreview ContactPreview { get; set; }
            public ContactPreview ParentContactPreview { get; set; }
            public ContactCreationModeFlag CreationModeFlag { get; set; }
            public int? ContactId { get; set; }
            public ContactType ContactType { get; set; }
            public bool SecondaryLayoutShown { get; set; }
            public bool ParentPreselected { get; set; }
        }

        #endregion

        static class RequestCodes
        {
            public const int ParentContactRequestCode = 111;
        }
    }
}
