using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.SearchCriteriaView
{
    public class ShortcodesSearchCriteriaViewController : AbstractSearchCriteriaViewController, IUIViewControllerRestoration
    {
        SearchShortcodesCriteria criteria = new SearchShortcodesCriteria();

        public override void LoadView()
        {
            base.LoadView();

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

        protected override async void ResetItem_Clicked(object sender, EventArgs e)
        {
            base.ResetItem_Clicked(sender, e);

            criteria = new SearchShortcodesCriteria();

            RefreshView();
            await SaveCriteria();
        }

        protected override void SearchButton_TouchUpInside(object sender, EventArgs e)
        {
            SearchButton.TouchUpInside -= SearchButton_TouchUpInside;

            criteria.MaxToFetch = PlatformConfig.Preferences.ShortcodesToSearch;

            CommonConfig.Logger.Info($"Starting search... [criteria={Serializer.Serialize(criteria)}]");

            NavigationController.PushViewController(new ShortcodesSearchResultsViewController
            {
                Criteria = criteria
            }, true);
        }

        protected override async Task SaveCriteria()
        {
            try
            {
                await Managers.SearchManager.SaveLastSearchShortcodesCrtieriaAsync(criteria);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to clear last search criteria", ex);
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
            readonly UILabel label;
            readonly UITextField text;

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

                label = new UILabel
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

                text = new UITextField
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_enter_search_text"),
                        new UIStringAttributes
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
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, view, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, view, NSLayoutAttribute.Left, 1f, 12f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Right, NSLayoutRelation.Equal, view, NSLayoutAttribute.Right, 1f, -8f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Top, NSLayoutRelation.Equal, label, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Left, NSLayoutRelation.Equal, view, NSLayoutAttribute.Left, 1f, 12f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Right, NSLayoutRelation.Equal, view, NSLayoutAttribute.Right, 1f, -8f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, view, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Height, NSLayoutRelation.Equal, text, NSLayoutAttribute.Height, 1f, 0f)
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
            void TextFieldDidChange(UITextField textField)
            {
                Criteria.Name = textField.Text;
            }


            [Export("textFieldShouldReturn:")]
            bool TextFieldShouldReturn(UITextField textField)
            {
                textField.ResignFirstResponder();
                return true;
            }

            [Export("textFieldDidEndEditing:")]
            void TextFieldDidEndEditing(UITextField textField)
            {
                text.ResignFirstResponder();
                text.UserInteractionEnabled = false;
            }
        }

        class DescritpionSearchView : AbstractShortcodesSearchView
        {
            readonly UIView view;
            readonly UILabel label;
            readonly UITextField text;

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

                label = new UILabel
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

                text = new UITextField
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_enter_search_text"),
                        new UIStringAttributes
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
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, view, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, view, NSLayoutAttribute.Left, 1f, 12f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Right, NSLayoutRelation.Equal, view, NSLayoutAttribute.Right, 1f, -8f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Top, NSLayoutRelation.Equal, label, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Left, NSLayoutRelation.Equal, view, NSLayoutAttribute.Left, 1f, 12f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Right, NSLayoutRelation.Equal, view, NSLayoutAttribute.Right, 1f, -8f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, view, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Height, NSLayoutRelation.Equal, text, NSLayoutAttribute.Height, 1f, 0f)
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
            void TextFieldDidChange(UITextField textField)
            {
                Criteria.Description = textField.Text;
            }


            [Export("textFieldShouldReturn:")]
            bool TextFieldShouldReturn(UITextField textField)
            {
                textField.ResignFirstResponder();
                return true;
            }

            [Export("textFieldDidEndEditing:")]
            void TextFieldDidEndEditing(UITextField textField)
            {
                text.ResignFirstResponder();
                text.UserInteractionEnabled = false;
            }
        }

        class AddressSearchView : AbstractShortcodesSearchView
        {
            readonly UIView view;
            readonly UILabel label;
            readonly UITextField text;

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

                label = new UILabel
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

                text = new UITextField
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_enter_search_text"),
                        new UIStringAttributes
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
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, view, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, view, NSLayoutAttribute.Left, 1f, 12f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Right, NSLayoutRelation.Equal, view, NSLayoutAttribute.Right, 1f, -8f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Top, NSLayoutRelation.Equal, label, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Left, NSLayoutRelation.Equal, view, NSLayoutAttribute.Left, 1f, 12f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Right, NSLayoutRelation.Equal, view, NSLayoutAttribute.Right, 1f, -8f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, view, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Height, NSLayoutRelation.Equal, text, NSLayoutAttribute.Height, 1f, 0f)
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
            void TextFieldDidChange(UITextField textField)
            {
                Criteria.Address = textField.Text;
            }


            [Export("textFieldShouldReturn:")]
            bool TextFieldShouldReturn(UITextField textField)
            {
                textField.ResignFirstResponder();
                return true;
            }

            [Export("textFieldDidEndEditing:")]
            void TextFieldDidEndEditing(UITextField textField)
            {
                text.ResignFirstResponder();
                text.UserInteractionEnabled = false;
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