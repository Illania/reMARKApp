using Android.Animation;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.FloatingActionButton;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Ui.Activities;
using reMark.Mobile.Droid.Ui.Common;
using reMark.Mobile.Droid.Ui.Views.SearchViews;
using reMark.Mobile.Droid.UI;
using reMark.Mobile.Droid.Utilities;
using Color = Android.Graphics.Color;
using View = Android.Views.View;

namespace reMark.Mobile.Droid.Ui.Fragments
{
    public class DocumentSearchCriteriaFragment : BaseFragment, ISearchCriteriaFragment
    {
        const string SearchCriteriaKey = "SearchCriteria_d48ddd3d-781c-45dd-bd25-8cb1ea7ab423";

        SearchDocumentsCriteria searchCriteria;
        public SavedDocumentsSearch CurrentSavedSearch { get; set; }
        DocumentSavedSearchView savedSearchView = null;

        LinearLayoutCompat containerLinearLayout;
        FloatingActionButton fab;

        List<AbstractSearchView<SearchDocumentsCriteria>> subviews = new List<AbstractSearchView<SearchDocumentsCriteria>>();

        public static (DocumentSearchCriteriaFragment Fragment, string tag) NewInstance()
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenSearchEvent());

            var fragment = new DocumentSearchCriteriaFragment();
            var tag = $"{nameof(DocumentSearchCriteriaFragment)}";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (savedInstanceState != null && savedInstanceState.ContainsKey(SearchCriteriaKey))
                searchCriteria = Serializer.Deserialize<SearchDocumentsCriteria>(savedInstanceState.GetString(SearchCriteriaKey));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(DocumentSearchCriteriaFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);

            var scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            scrollView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));

            containerLinearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            containerLinearLayout.SetBackgroundColor(Color.Transparent);
            containerLinearLayout.DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_horizontal);
            containerLinearLayout.ShowDividers = LinearLayoutCompat.ShowDividerMiddle;
            containerLinearLayout.Focusable = true;
            containerLinearLayout.FocusableInTouchMode = true;

            var paddingLinearLayout = Conversion.ConvertDpToPixels(12);
            var bottomPadding = Conversion.ConvertDpToPixels(56) + (Resources.GetDimension(Resource.Dimension.fab_margin) + 2) * 2;
            containerLinearLayout.SetPadding(paddingLinearLayout, paddingLinearLayout, paddingLinearLayout, (int)bottomPadding);

            fab = ((BaseAppCompatActivity)Activity).Fab;
            fab.AddOnLayoutChangeListener(new FloatingActionButtonLayoutChangeListener());

            var fabIcon = Resources.GetDrawable(Resource.Drawable.action_search_server, null).GetConstantState().NewDrawable().Mutate();
            fabIcon.SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)), PorterDuff.Mode.Multiply);

            fab.SetImageDrawable(fabIcon);
            fab.SetOnClickListener(new ActionOnClickListener(HandleSearchButtonClicked));
            fab.BackgroundTintList = ColorStateList.ValueOf(new Color(ContextCompat.GetColor(Context, Resource.Color.lightblue)));
            fab.RippleColor = new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)).ToArgb();

            var p = (CoordinatorLayout.LayoutParams)fab.LayoutParameters;
            p.Gravity = (int)(GravityFlags.Bottom | GravityFlags.CenterHorizontal);
            p.Behavior = new FloatingActionButtonBehavior();
            fab.LayoutParameters = p;

            
            if (ServerConfig.SystemSettings?.SystemInfo?.SavedSearchesAvailable == true)
            {
                savedSearchView = new DocumentSavedSearchView(Context, this);
                subviews.Add(savedSearchView);
            }
          
            var directionCriteria = new DocumentDirectionsSearchView(Context);
            subviews.Add(directionCriteria);

            var daterangeCriteria = new DocumentDateRangeSearchView(Context, this);
            subviews.Add(daterangeCriteria);

            var subjectMessageCriteria = new DocumentSubjectMessageSearchView(Context);
            subviews.Add(subjectMessageCriteria);

            var fromToCriteria = new DocumentFromToSearchView(Context);
            subviews.Add(fromToCriteria);

            DocumentExtraFieldsSearchView extraFieldsCriteria = null;
            if (ServerConfig.SystemSettings.DocumentsModuleInfo.ExtraFieldInfos.Any())
            {
                extraFieldsCriteria = new DocumentExtraFieldsSearchView(Context);
                subviews.Add(extraFieldsCriteria);
            }

            var attUnreadCriteria = new DocumentAttachmentUnreadSearchView(Context);
            subviews.Add(attUnreadCriteria);

            DocumentHandledSearchView handledCriteria = null;
            if (ServerConfig.SystemSettings.DocumentsModuleInfo.HandledFieldEnabled)
            {
                handledCriteria = new DocumentHandledSearchView(Context);
                subviews.Add(handledCriteria);
            }

            if (ServerConfig.SystemSettings?.SystemInfo?.SavedSearchesAvailable == true)
                containerLinearLayout.AddView(savedSearchView);
            containerLinearLayout.AddView(directionCriteria);
            containerLinearLayout.AddView(subjectMessageCriteria);
            containerLinearLayout.AddView(fromToCriteria);
            containerLinearLayout.AddView(daterangeCriteria);
            PrepareEditableTextRow();
            PrepareDropdownTextRow();
            if (extraFieldsCriteria != null)
                containerLinearLayout.AddView(extraFieldsCriteria);
            containerLinearLayout.AddView(attUnreadCriteria);
            if (handledCriteria != null)
                containerLinearLayout.AddView(handledCriteria);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.search);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.documents);

            CommonConfig.Logger.Info($"Created {nameof(DocumentSearchCriteriaFragment)}");
        }

        public override async void OnResume()
        {
            base.OnResume();

            fab.Visibility = ViewStates.Visible;

            try
            {
                searchCriteria = searchCriteria ?? await Managers.SearchManager.GetLastSearchDocumentsCriteriaAsync();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to restore last search criteria", ex);

                searchCriteria = new SearchDocumentsCriteria();
            }

            RefreshViews();
        }

        public override void OnPause()
        {
            fab.Visibility = ViewStates.Gone;
            UpdateCriteria();

            base.OnPause();
        }

        public override async void OnStop()
        {
            base.OnStop();

            try
            {
                await Managers.SearchManager.SaveLastSearchDocumentsCriteriaAsync(searchCriteria);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to clear last search criteria", ex);
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            outState.PutString(SearchCriteriaKey, Serializer.Serialize(UpdateCriteria()));
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();
            var item = menu.AddMenuItem(IMenu.None, 10, 10, Resource.String.reset, this);
            item.SetShowAsAction(ShowAsAction.Always);

            if (ServerConfig.SystemSettings?.SystemInfo?.SavedSearchesAvailable == true)
            {
                var itemSave = menu.Add(IMenu.None, 20, 20, Resource.String.save);
                itemSave.SetShowAsAction(ShowAsAction.Always);
            }   
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 10)
            {
                Reset();
                return true;
            }
            if(item.ItemId == 20)
            {
                SaveSearch();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }


        private async void SaveSearch()
        {
            if (CurrentSavedSearch != null)
            {

                var choice = await Dialogs.ShowListDialog(Context, Resource.String.save, Resource.Array.save_options, true);


                if (choice < 0)
                    return;

                HandleSaveButtonChoice(choice);
            }
            else
            {
                await AddNewSavedSearch();
            }
        }

        protected async void HandleSaveButtonChoice(int choice)
        {
            switch (choice)
            {
                case 0:
                    if (CurrentSavedSearch != null)
                    {
                        UpdateCriteria();
                        await Managers.SearchManager.UpdateSavedDocumentsSearchAsync(CurrentSavedSearch.Id,
                            new SavedDocumentsSearch()
                            {
                                Id = CurrentSavedSearch.Id,
                                Name = CurrentSavedSearch.Name,
                                Criteria = searchCriteria
                            });
                    }  
                    break;
                case 1:
                    await AddNewSavedSearch();
                    break;

            }
        }

        private async Task AddNewSavedSearch()
        {
            try
            {
                Dialogs.ShowEditTextDialog(Context, Resource.String.saved_search_name, string.Empty,
                    async (text) => {
                        var newSavedSearch = new SavedDocumentsSearch() { Criteria = CurrentSavedSearch?.Criteria ?? searchCriteria, Name = text };
                        var newSavedSearchSaved = await Managers.SearchManager.AddSavedDocumentsSearchAsync(newSavedSearch);
                        CurrentSavedSearch = newSavedSearchSaved;
                        searchCriteria = newSavedSearchSaved.Criteria;
                        savedSearchView.UpdateSavedSearch(CurrentSavedSearch);
                        ReloadCriteria(CurrentSavedSearch.Criteria);

                    }, null, Resource.String.confirm, Resource.String.cancel);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

        public void PrepareEditableTextRow()
        {
            var ll = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };

            ll.DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_vertical);
            ll.ShowDividers = LinearLayoutCompat.ShowDividerMiddle;

            var lp = new LinearLayoutCompat.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1);

            var commentCriteria = new DocumentCommentsSearchView(Context, ll);
            subviews.Add(commentCriteria);

            var attachmentCriteria = new DocumentAttachmentSearchView(Context, ll);
            subviews.Add(attachmentCriteria);

            var referenceNumberCriteria = new DocumentReferenceNumberSearchView(Context, ll);
            subviews.Add(referenceNumberCriteria);

            ll.AddView(referenceNumberCriteria, lp);
            ll.AddView(commentCriteria, lp);
            ll.AddView(attachmentCriteria, lp);
            ll.LayoutTransition = new LayoutTransition();

            containerLinearLayout.AddView(ll);
        }

        public void PrepareDropdownTextRow()
        {
            var ll = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };

            ll.DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_vertical);
            ll.ShowDividers = LinearLayoutCompat.ShowDividerMiddle;

            var lp = new LinearLayoutCompat.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1);

            var categoriesCriteria = new DocumentCategoriesSearchView(Context, this);
            subviews.Add(categoriesCriteria);

            var linesCriteria = new DocumentLinesSearchView(Context, this);
            subviews.Add(linesCriteria);

            var priorityCriteria = new DocumentPrioritySearchView(Context, this);
            subviews.Add(priorityCriteria);

            ll.AddView(categoriesCriteria, lp);
            ll.AddView(linesCriteria, lp);
            ll.AddView(priorityCriteria, lp);

            containerLinearLayout.AddView(ll);
        }

        public void ReplaceFragment(Fragment f, string tag)
        {
            var fragmentManager = ((AppCompatActivity)Activity).SupportFragmentManager;

            fragmentManager.BeginTransaction()
                           .SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right)
                           .Replace(Resource.Id.fragment_container, f, tag)
                           .AddToBackStack(tag)
                           .Commit();
        }

        void RefreshViews()
        {
            subviews.ForEach(c =>
            {
                c.Criteria = searchCriteria;
                c.Refresh();
            });
            if (!string.IsNullOrEmpty(CurrentSavedSearch?.Name))
                savedSearchView.UpdateBottomTextView(CurrentSavedSearch.Name);
               
        }

        public void ReloadCriteria(SearchDocumentsCriteria criteria)
        {
            searchCriteria = criteria;
            subviews.ForEach(c =>
            {
                c.Criteria = criteria;
                c.Refresh();
            });
        }

        void HandleSearchButtonClicked()
        {
            UpdateCriteria();

            StartActivity(SearchResultsActivity.CreateIntent(Context, ModuleType.Documents, documentCriteria: UpdateCriteria()));
        }

        SearchDocumentsCriteria UpdateCriteria()
        {
            subviews.ForEach(v => v.UpdateCriteria());

            searchCriteria.PartialWordSearch = PlatformConfig.Preferences.PartialWordSearch;
            searchCriteria.MaxToFetch = PlatformConfig.Preferences.MaxDocumentsToSearch;

            CommonConfig.Logger.Info($"Starting search... [criteria={Serializer.Serialize(searchCriteria)}]");

            return searchCriteria;
        }

        async void Reset()
        {
            searchCriteria = new SearchDocumentsCriteria();
            containerLinearLayout.RequestFocus();
            ((InputMethodManager)Context.GetSystemService(Context.InputMethodService)).HideSoftInputFromWindow(containerLinearLayout.WindowToken, HideSoftInputFlags.None);
            savedSearchView?.UpdateBottomTextView(GetString(Resource.String.saved_searches_none_selected));
            CurrentSavedSearch = null;
            RefreshViews();

            try
            {
                await Managers.SearchManager.SaveLastSearchDocumentsCriteriaAsync(searchCriteria);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to clear last search criteria", ex);
            }
        }
    }

    public interface ISearchCriteriaFragment
    {
        void ReplaceFragment(Fragment f, string tag);
    }
}