using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Common.Utilities.Extensions;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Utilities;
using ObjCRuntime;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers.SearchView
{
    public class ShortcodesSearchCriteriaViewController : AbstractSearchCriteriaViewController, IUIViewControllerRestoration
    {
        SearchShortcodesCriteria criteria = new();  
        public SavedShortcodesSearch CurrentSavedSearch { get; set; }
        ShortcodesSavedSearchesView savedSearchesView = null;

        public override void LoadView()
        {
            base.LoadView();

            CommonConfig.UsageAnalytics.LogEvent(new OpenSearchEvent());
            if (ServerConfig.SystemSettings?.SystemInfo?.SavedSearchesAvailable == true)
            {
                savedSearchesView = new ShortcodesSavedSearchesView(this);
                StackView.AddArrangedSubview(savedSearchesView);
            }
            StackView.AddArrangedSubview(new NameSearchView());
            StackView.AddArrangedSubview(new DescritpionSearchView());
            StackView.AddArrangedSubview(new AddressSearchView());
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(ShortcodesSearchCriteriaViewController);
            RestorationClass = Class;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        protected override async void ResetItem_Clicked(object sender, EventArgs e)
        {
            base.ResetItem_Clicked(sender, e);

            criteria = new SearchShortcodesCriteria();
            CurrentSavedSearch = null;
            savedSearchesView?.ResetView();

            RefreshView();
            await SaveCriteria();
        }

        protected override void SearchButton_TouchUpInside(object sender, EventArgs e)
        {
            criteria.MaxToFetch = PlatformConfig.Preferences.ShortcodesToSearch;

            CommonConfig.Logger.Info($"Starting search... [criteria={Serializer.Serialize(criteria)}]");

            if (Integration.IsIPad())
                PresentViewController(new ShortcodesSplitSearchViewController(criteria), true, null);
            else
                NavigationController.PushViewController(new ShortcodesSearchResultsViewController { Criteria = criteria }, true);
        }

        protected override async void SaveButton_TouchUpInside(object sender, EventArgs e)
        {
            if (CurrentSavedSearch != null)
            {
                var choice = await Dialogs.ShowListActionSheetAsync(this, new[]
                {
                    Localization.GetString("save"),
                    Localization.GetString("save_as"),
                }, savedSearchesView);

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
                        await Managers.SearchManager.UpdateSavedShortcodesSearchAsync(CurrentSavedSearch.Id, CurrentSavedSearch);
                    break;
                case 1:
                    await AddNewSavedSearch();
                    break;

            }
        }

        public void ReloadCriteria(SearchShortcodesCriteria criteria)
        {
            foreach (var view in StackView.Subviews.OfType<AbstractShortcodesSearchView>())
                view.SetCriteria(criteria);
        }

        public void UpdateCurrentSavedSearch(SavedShortcodesSearch savedSearch)
        {
            CurrentSavedSearch = savedSearch;
        }

        private async Task AddNewSavedSearch()
        {
            try
            {
                //show view controller to enter new saved search title
                var dp = new StringEditorViewController
                {
                    ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext,
                    ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve
                };
                PresentViewController(dp, false, null);
                var newName = await dp.Result;
                var newSavedSearch = new SavedShortcodesSearch() { Criteria = CurrentSavedSearch?.Criteria ?? criteria, Name = newName };
                var newSavedSearchSaved = await Managers.SearchManager.AddSavedShortcodesSearchAsync(newSavedSearch);
                CurrentSavedSearch = newSavedSearchSaved;
                criteria = newSavedSearchSaved.Criteria;
                savedSearchesView.SetCurrent(newName);
                RefreshView();
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

        abstract class AbstractContactsSearchView : AbstractSearchView
        {
            protected SearchContactsCriteria Criteria;

            public void SetCriteria(SearchContactsCriteria criteria)
            {
                Criteria = criteria;
                UpdateRow();
            }
        }


        protected override async Task SaveCriteria()
        {
            try
            {
                await Managers.SearchManager.SaveLastSearchShortcodesCrtieriaAsync(criteria);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to save last search criteria", ex);
            }
        }

        protected override async Task RestoreCriteria()
        {
            try
            {
                criteria = await Managers.SearchManager.GetLastSearchShortcodesCrtieriaAsync();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to restore last search criteria", ex);
            }
        }

        protected override void RefreshView()
        {
            foreach (var view in StackView.Subviews.OfType<AbstractShortcodesSearchView>())
                view.SetCriteria(criteria);
        }

        abstract class AbstractShortcodesSearchView : AbstractSearchView
        {
            protected SearchShortcodesCriteria Criteria;

            public void SetCriteria(SearchShortcodesCriteria criteria)
            {
                Criteria = criteria;
                UpdateRow();
            }
        }

        class NameSearchView : AbstractShortcodesSearchView
        {
            readonly UIView view;
            readonly UILabelScalable label;
            readonly UITextFieldScalable text;

            public NameSearchView()
            {
                view = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                view.Layer.CornerRadius = CornerRadius;
                view.Layer.MasksToBounds = true;
                view.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                label = new UILabelScalable
                {
                    Text = Localization.GetString("search_name"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                text = new UITextFieldScalable
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_enter_search_text"), new UIStringAttributes
                    {
                        ForegroundColor = Theme.LightGray
                    }),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    WeakDelegate = this
                };
                text.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                view.Add(label);
                view.Add(text);
                view.AddConstraints(new[]
                {
                    label.TopAnchor.ConstraintEqualTo(view.TopAnchor, 4f),
                    label.LeftAnchor.ConstraintEqualTo(view.LeftAnchor, 12f),
                    label.RightAnchor.ConstraintEqualTo(view.RightAnchor, -8f),
                    text.TopAnchor.ConstraintEqualTo(label.BottomAnchor, 2f),
                    text.LeftAnchor.ConstraintEqualTo(view.LeftAnchor, 12f),
                    text.RightAnchor.ConstraintEqualTo(view.RightAnchor, -8f),
                    text.BottomAnchor.ConstraintEqualTo(view.BottomAnchor, -4f),
                    label.HeightAnchor.ConstraintEqualTo(text.HeightAnchor)
                });

                AddArrangedSubview(view);
            }

            protected override void UpdateRow()
            {
                text.Text = Criteria.Name;
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                text.UserInteractionEnabled = true;
                text.BecomeFirstResponder();

                UpdateRow();
                SetAsActive();
            }

            [Export("textFieldDidChange:")]
            void TextFieldDidChange(UITextFieldScalable textField)
            {
                Criteria.Name = textField.Text;
            }

            [Export("textFieldShouldReturn:")]
            bool TextFieldShouldReturn(UITextFieldScalable textField)
            {
                textField.ResignFirstResponder();
                return true;
            }

            [Export("textFieldDidEndEditing:")]
            void TextFieldDidEndEditing(UITextFieldScalable textField)
            {
                text.ResignFirstResponder();
                text.UserInteractionEnabled = false;
            }
        }

        class DescritpionSearchView : AbstractShortcodesSearchView
        {
            readonly UIView view;
            readonly UILabelScalable label;
            readonly UITextFieldScalable text;

            public DescritpionSearchView()
            {
                view = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                view.Layer.CornerRadius = CornerRadius;
                view.Layer.MasksToBounds = true;
                view.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                label = new UILabelScalable
                {
                    Text = Localization.GetString("search_search_description"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                text = new UITextFieldScalable
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_enter_search_text"), new UIStringAttributes
                    {
                        ForegroundColor = Theme.LightGray
                    }),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    WeakDelegate = this
                };
                text.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                view.Add(label);
                view.Add(text);
                view.AddConstraints(new[]
                {
                    label.TopAnchor.ConstraintEqualTo(view.TopAnchor, 4f),
                    label.LeftAnchor.ConstraintEqualTo(view.LeftAnchor, 12f),
                    label.RightAnchor.ConstraintEqualTo(view.RightAnchor, -8f),
                    text.TopAnchor.ConstraintEqualTo(label.BottomAnchor, 2f),
                    text.LeftAnchor.ConstraintEqualTo(view.LeftAnchor, 12f),
                    text.RightAnchor.ConstraintEqualTo(view.RightAnchor, -8f),
                    text.BottomAnchor.ConstraintEqualTo(view.BottomAnchor, -4f),
                    label.HeightAnchor.ConstraintEqualTo(text.HeightAnchor),
                });

                AddArrangedSubview(view);
            }

            protected override void UpdateRow()
            {
                text.Text = Criteria.Description;
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                text.UserInteractionEnabled = true;
                text.BecomeFirstResponder();

                UpdateRow();
                SetAsActive();
            }

            [Export("textFieldDidChange:")]
            void TextFieldDidChange(UITextFieldScalable textField)
            {
                Criteria.Description = textField.Text;
            }


            [Export("textFieldShouldReturn:")]
            bool TextFieldShouldReturn(UITextFieldScalable textField)
            {
                textField.ResignFirstResponder();
                return true;
            }

            [Export("textFieldDidEndEditing:")]
            void TextFieldDidEndEditing(UITextFieldScalable textField)
            {
                text.ResignFirstResponder();
                text.UserInteractionEnabled = false;
            }
        }

        class AddressSearchView : AbstractShortcodesSearchView
        {
            readonly UIView view;
            readonly UILabelScalable label;
            readonly UITextFieldScalable text;

            public AddressSearchView()
            {
                view = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                view.Layer.CornerRadius = CornerRadius;
                view.Layer.MasksToBounds = true;
                view.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                label = new UILabelScalable
                {
                    Text = Localization.GetString("search_emails"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                text = new UITextFieldScalable
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_enter_search_text"), new UIStringAttributes
                    {
                        ForegroundColor = Theme.LightGray
                    }),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    WeakDelegate = this
                };
                text.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                view.Add(label);
                view.Add(text);
                view.AddConstraints(new[]
                {
                    label.TopAnchor.ConstraintEqualTo(view.TopAnchor, 4f),
                    label.LeftAnchor.ConstraintEqualTo(view.LeftAnchor, 12f),
                    label.RightAnchor.ConstraintEqualTo(view.RightAnchor, -8f),
                    text.TopAnchor.ConstraintEqualTo(label.BottomAnchor, 2f),
                    text.LeftAnchor.ConstraintEqualTo(view.LeftAnchor, 12f),
                    text.RightAnchor.ConstraintEqualTo(view.RightAnchor, -8f),
                    text.BottomAnchor.ConstraintEqualTo(view.BottomAnchor, -4f),
                    label.HeightAnchor.ConstraintEqualTo(text.HeightAnchor),
                });

                AddArrangedSubview(view);
            }

            protected override void UpdateRow()
            {
                text.Text = Criteria.Address;
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                text.UserInteractionEnabled = true;
                text.BecomeFirstResponder();

                UpdateRow();
                SetAsActive();
            }

            [Export("textFieldDidChange:")]
            void TextFieldDidChange(UITextFieldScalable textField)
            {
                Criteria.Address = textField.Text;
            }


            [Export("textFieldShouldReturn:")]
            bool TextFieldShouldReturn(UITextFieldScalable textField)
            {
                textField.ResignFirstResponder();
                return true;
            }

            [Export("textFieldDidEndEditing:")]
            void TextFieldDidEndEditing(UITextFieldScalable textField)
            {
                text.ResignFirstResponder();
                text.UserInteractionEnabled = false;
            }
        }

        class ShortcodesSavedSearchesView : AbstractShortcodesSearchView
        {
            readonly WeakReference<ShortcodesSearchCriteriaViewController> parentViewControllerWeakReference;

            readonly UIView view;
            readonly UILabelScalable label;
            readonly UILabelScalable text;
            string currentSearchName;

            public ShortcodesSavedSearchesView(ShortcodesSearchCriteriaViewController parentViewController)
            {
                parentViewControllerWeakReference = parentViewController.Wrap();

                view = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                view.Layer.CornerRadius = CornerRadius;
                view.Layer.MasksToBounds = true;
                view.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                label = new UILabelScalable
                {
                    Text = Localization.GetString("saved_searches"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                text = new UILabelScalable
                {
                    Text = Localization.GetString("search_load"),
                    TextColor = Theme.LightGray,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                };
                view.Add(label);
                view.Add(text);
                view.AddConstraints(new[]
                {
                    label.TopAnchor.ConstraintEqualTo(view.TopAnchor,4f),
                    label.LeftAnchor.ConstraintEqualTo(view.LeftAnchor,4f),
                    label.RightAnchor.ConstraintEqualTo(view.RightAnchor,-4f),
                    text.TopAnchor.ConstraintEqualTo(label.BottomAnchor,2f),
                    text.LeftAnchor.ConstraintEqualTo(view.LeftAnchor,4f),
                    text.RightAnchor.ConstraintEqualTo(view.RightAnchor,-4f),
                    text.BottomAnchor.ConstraintEqualTo(view.BottomAnchor,-4f),
                    label.HeightAnchor.ConstraintEqualTo(text.HeightAnchor)
                });

                AddArrangedSubview(view);
            }

            protected override void UpdateRow()
            {
                if (!string.IsNullOrEmpty(currentSearchName))
                    text.Text = currentSearchName;
            }

            public void SetCurrent(string name)
            {
                currentSearchName = name;
                text.Text = name;
            }

            public void ResetView()
            {
                currentSearchName = string.Empty;
                text.Text = Localization.GetString("search_load");
            }

            [Export("tapped:")]
            async void Tapped(UITapGestureRecognizer recognizer)
            {
                var vc = new SavedShortcodeSearchesViewController();
                parentViewControllerWeakReference.Unwrap()?.PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);

                var result = await vc.Result;
                if (result == null)
                    return;

                Criteria = result.Criteria;
                currentSearchName = result.Name;
                (parentViewControllerWeakReference.Unwrap())?.UpdateCurrentSavedSearch(savedSearch: result);
                (parentViewControllerWeakReference.Unwrap())?.ReloadCriteria(Criteria);

                text.UserInteractionEnabled = true;
                text.BecomeFirstResponder();

                UpdateRow();
            }

        }

        #region State restoration

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);
            coder.Encode(Serializer.SerializeToByteArray(criteria), "criteria");
            coder.Encode(RestoreCriteriaFromStorage, "restoreCriteriaFromStorage");
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);
            criteria = Serializer.DeserializeFromByteArray<SearchShortcodesCriteria>(coder.DecodeBytes("criteria"));
            RestoreCriteriaFromStorage = coder.DecodeBool("restoreCriteriaFromStorage");
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            return new ShortcodesSearchCriteriaViewController();
        }

        #endregion

    }
}