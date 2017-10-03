using System;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.SearchCriteriaView
{
    public class ContactsSearchCriteriaViewController : AbstractSearchCriteriaViewController, IUIViewControllerRestoration
    {
        SearchContactsCriteria criteria = new SearchContactsCriteria();

        public override void LoadView()
        {
            base.LoadView();

            StackView.AddArrangedSubview(new ContactTypesSearchView());
            StackView.AddArrangedSubview(new NameSearchView());
            StackView.AddArrangedSubview(new AddressSearchView());
            StackView.AddArrangedSubview(new ShortIdDescriptionPhysicalAddressView());
            StackView.AddArrangedSubview(new CountryCategoriesView(this));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(ContactsSearchCriteriaViewController);
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

            criteria = new SearchContactsCriteria();

            RefreshView();
            await SaveCriteria();
        }

        protected override void SearchButton_TouchUpInside(object sender, EventArgs e)
        {
            SearchButton.TouchUpInside -= SearchButton_TouchUpInside;

            criteria.MaxToFetch = PlatformConfig.Preferences.ContactsToSearch;

            CommonConfig.Logger.Info($"Starting search... [criteria={Serializer.Serialize(criteria)}]");

            NavigationController.PushViewController(new ContactsSearchResultsViewController { Criteria = criteria }, true);
        }

        protected override async Task SaveCriteria()
        {
            try
            {
                await Managers.SearchManager.SaveLastSearchContactsCriteriaAsync(criteria);
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
                criteria = await Managers.SearchManager.GetLastSearchContactsCriteriaAsync();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to restore last search criteria", ex);
            }
        }

        protected override void RefreshView()
        {
            foreach (var view in StackView.Subviews.OfType<AbstractContactsSearchView>())
                view.SetCriteria(criteria);
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

        class ContactTypesSearchView : AbstractContactsSearchView
        {
            readonly UILabel personView;
            readonly UILabel departmentView;
            readonly UILabel companyView;

            public ContactTypesSearchView()
            {
                personView = new UILabel
                {
                    Text = Localization.GetString("search_person").ToUpper(),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                personView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                personView.Layer.CornerRadius = CornerRadius;
                personView.Layer.MasksToBounds = true;
                AddArrangedSubview(personView);

                departmentView = new UILabel
                {
                    Text = Localization.GetString("search_department").ToUpper(),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                departmentView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                departmentView.Layer.CornerRadius = CornerRadius;
                departmentView.Layer.MasksToBounds = true;
                AddArrangedSubview(departmentView);

                companyView = new UILabel
                {
                    Text = Localization.GetString("search_company").ToUpper(),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                companyView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                companyView.Layer.CornerRadius = CornerRadius;
                companyView.Layer.MasksToBounds = true;
                AddArrangedSubview(companyView);
            }

            protected override void UpdateRow()
            {
                var types = Criteria.ContactTypes;

                if (!types.Any())
                    types.UnionWith(new[] { ContactType.Person, ContactType.Department, ContactType.Company });

                SetLabelActive(personView, types.Contains(ContactType.Person));
                SetLabelActive(departmentView, types.Contains(ContactType.Department));
                SetLabelActive(companyView, types.Contains(ContactType.Company));
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                var types = Criteria.ContactTypes;

                if (recognizer.View == personView)
                    if (types.Contains(ContactType.Person))
                        types.Remove(ContactType.Person);
                    else
                        types.Add(ContactType.Person);
                else if (recognizer.View == departmentView)
                    if (types.Contains(ContactType.Department))
                        types.Remove(ContactType.Department);
                    else
                        types.Add(ContactType.Department);
                else if (recognizer.View == companyView)
                    if (types.Contains(ContactType.Company))
                        types.Remove(ContactType.Company);
                    else
                        types.Add(ContactType.Company);


                UpdateRow();
            }
        }

        class NameSearchView : AbstractContactsSearchView
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

        class AddressSearchView : AbstractContactsSearchView
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
                    Text = Localization.GetString("search_email_number_etc"),
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
                text.Text = Criteria.ComAddress;
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
                Criteria.ComAddress = textField.Text;
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

        class ShortIdDescriptionPhysicalAddressView : AbstractContactsSearchView
        {
            readonly UIView shortIdView;
            readonly UILabel shortIdLabel;
            readonly UITextField shortIdTextField;
            readonly UIView descriptionView;
            readonly UILabel descriptionLabel;
            readonly UITextField descriptionTextField;
            readonly UIView physicalAddressView;
            readonly UILabel physicalAddressLabel;
            readonly UITextField physicalAddressTextField;

            public ShortIdDescriptionPhysicalAddressView()
            {
                shortIdView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                shortIdView.Layer.CornerRadius = CornerRadius;
                shortIdView.Layer.MasksToBounds = true;
                shortIdView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                shortIdLabel = new UILabel
                {
                    Text = Localization.GetString("search_short_id"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                shortIdTextField = new UITextField
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_type"), new UIStringAttributes
                    {
                        ForegroundColor = Theme.LightGray
                    }),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    WeakDelegate = this
                };
                shortIdTextField.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                shortIdView.Add(shortIdLabel);
                shortIdView.Add(shortIdTextField);
                shortIdView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(shortIdLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, shortIdView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(shortIdLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, shortIdView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(shortIdLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, shortIdView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(shortIdTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, shortIdLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(shortIdTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, shortIdView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(shortIdTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, shortIdView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(shortIdTextField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, shortIdView, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(shortIdLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, shortIdTextField, NSLayoutAttribute.Height, 1f, 0f)
                });

                AddArrangedSubview(shortIdView);

                descriptionView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                descriptionView.Layer.CornerRadius = CornerRadius;
                descriptionView.Layer.MasksToBounds = true;
                descriptionView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                descriptionLabel = new UILabel
                {
                    Text = Localization.GetString("search_description"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                descriptionTextField = new UITextField
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_type"), new UIStringAttributes
                    {
                        ForegroundColor = Theme.LightGray
                    }),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    WeakDelegate = this
                };
                descriptionTextField.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                descriptionView.Add(descriptionLabel);
                descriptionView.Add(descriptionTextField);
                descriptionView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(descriptionLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, descriptionView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(descriptionLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, descriptionView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(descriptionLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, descriptionView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, descriptionLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, descriptionView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, descriptionView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, descriptionView, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(descriptionLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, descriptionTextField, NSLayoutAttribute.Height, 1f, 0f)
                });

                AddArrangedSubview(descriptionView);

                physicalAddressView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                physicalAddressView.Layer.CornerRadius = CornerRadius;
                physicalAddressView.Layer.MasksToBounds = true;
                physicalAddressView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                physicalAddressLabel = new UILabel
                {
                    Text = Localization.GetString("search_physical_address"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                physicalAddressTextField = new UITextField
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_type"),
                        new UIStringAttributes
                        {
                            ForegroundColor = Theme.LightGray
                        }),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    WeakDelegate = this
                };
                physicalAddressTextField.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                physicalAddressView.Add(physicalAddressLabel);
                physicalAddressView.Add(physicalAddressTextField);
                physicalAddressView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(physicalAddressLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, physicalAddressView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(physicalAddressLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, physicalAddressView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(physicalAddressLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, physicalAddressView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(physicalAddressTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, physicalAddressLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(physicalAddressTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, physicalAddressView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(physicalAddressTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, physicalAddressView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(physicalAddressTextField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, physicalAddressView, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(physicalAddressLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, physicalAddressTextField, NSLayoutAttribute.Height, 1f, 0f)
                });

                AddArrangedSubview(physicalAddressView);
            }

            protected override void UpdateRow()
            {
                shortIdTextField.Text = Criteria.ShortId;
                descriptionTextField.Text = Criteria.Description;
                physicalAddressTextField.Text = Criteria.PostAddress;
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                if (recognizer.View == shortIdView)
                    AnimateNotify(AnimationLength,
                        () =>
                        {
                            descriptionView.Hidden = true;
                            physicalAddressView.Hidden = true;
                        },
                        ch =>
                        {
                            shortIdTextField.UserInteractionEnabled = true;
                            shortIdTextField.BecomeFirstResponder();
                        });

                if (recognizer.View == descriptionView)
                    AnimateNotify(AnimationLength,
                        () =>
                        {
                            shortIdView.Hidden = true;
                            physicalAddressView.Hidden = true;
                        },
                        ch =>
                        {
                            descriptionTextField.UserInteractionEnabled = true;
                            descriptionTextField.BecomeFirstResponder();
                        });

                if (recognizer.View == physicalAddressView)
                    AnimateNotify(AnimationLength,
                        () =>
                        {
                            shortIdView.Hidden = true;
                            descriptionView.Hidden = true;
                        },
                        ch =>
                        {
                            physicalAddressTextField.UserInteractionEnabled = true;
                            physicalAddressTextField.BecomeFirstResponder();
                        });

                UpdateRow();
                SetAsActive();
            }

            [Export("textFieldDidChange:")]
            void TextFieldDidChange(UITextField textField)
            {
                if (textField == shortIdTextField)
                    Criteria.ShortId = textField.Text;
                if (textField == descriptionTextField)
                    Criteria.Description = textField.Text;
                if (textField == physicalAddressTextField)
                    Criteria.PostAddress = textField.Text;
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
                if (textField == shortIdTextField)
                    AnimateNotify(AnimationLength,
                        () =>
                        {
                            descriptionView.Hidden = false;
                            physicalAddressView.Hidden = false;
                        },
                        ch =>
                        {
                            shortIdTextField.ResignFirstResponder();
                            shortIdTextField.UserInteractionEnabled = false;
                        });

                if (textField == descriptionTextField)
                    AnimateNotify(AnimationLength,
                        () =>
                        {
                            shortIdView.Hidden = false;
                            physicalAddressView.Hidden = false;
                        },
                        ch =>
                        {
                            descriptionTextField.ResignFirstResponder();
                            descriptionTextField.UserInteractionEnabled = false;
                        });

                if (textField == physicalAddressTextField)
                    AnimateNotify(AnimationLength,
                        () =>
                        {
                            shortIdView.Hidden = false;
                            descriptionView.Hidden = false;
                        },
                        ch =>
                        {
                            physicalAddressTextField.ResignFirstResponder();
                            physicalAddressTextField.UserInteractionEnabled = false;
                        });
            }
        }

        class CountryCategoriesView : AbstractContactsSearchView
        {
            readonly WeakReference<UIViewController> parentViewControllerWeakReference;

            readonly UIView countryView;
            readonly UILabel countryLabel;
            readonly UITextField countryValue;
            readonly UIView categoriesView;
            readonly UILabel categoriesLabel;
            readonly UILabel categoriesValue;

            readonly UIToolbar countryPickerToolbar;
            readonly Source countrySource;
            readonly UIPickerView countryPicker;

            public CountryCategoriesView(UIViewController parentViewController)
            {
                parentViewControllerWeakReference = parentViewController.Wrap();

                countryView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                countryView.Layer.CornerRadius = CornerRadius;
                countryView.Layer.MasksToBounds = true;
                countryView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                countryPickerToolbar = new UIToolbar(new CGRect(0f, 0f, 0f, 44f))
                {
                    Items = new[]
                    {
                        new UIBarButtonItem(UIBarButtonSystemItem.Cancel, this, new Selector("cancelTapped:"))
                        {
                            TintColor = Theme.DarkerBlue
                        },
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                        new UIBarButtonItem(UIBarButtonSystemItem.Done, this, new Selector("doneTapped:"))
                        {
                            TintColor = Theme.DarkerBlue
                        }
                    }
                };

                countryLabel = new UILabel
                {
                    Text = Localization.GetString("search_country"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                countrySource = new Source();
                countryPicker = new UIPickerView
                {
                    DataSource = countrySource,
                    Delegate = countrySource
                };

                countryValue = new UITextField
                {
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = UIColor.Clear,
                    TextAlignment = UITextAlignment.Center,
                    InputView = countryPicker,
                    InputAccessoryView = countryPickerToolbar,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false
                };
                countryView.Add(countryLabel);
                countryView.Add(countryValue);
                countryView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(countryLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, countryView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(countryLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, countryView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(countryLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, countryView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(countryValue, NSLayoutAttribute.Top, NSLayoutRelation.Equal, countryLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(countryValue, NSLayoutAttribute.Left, NSLayoutRelation.Equal, countryView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(countryValue, NSLayoutAttribute.Right, NSLayoutRelation.Equal, countryView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(countryValue, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, countryView, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(countryLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, countryValue, NSLayoutAttribute.Height, 1f, 0f)
                });

                AddArrangedSubview(countryView);

                categoriesView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                categoriesView.Layer.CornerRadius = CornerRadius;
                categoriesView.Layer.MasksToBounds = true;
                categoriesView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                categoriesLabel = new UILabel
                {
                    Text = Localization.GetString("search_categories"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                categoriesValue = new UILabel
                {
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                categoriesView.Add(categoriesLabel);
                categoriesView.Add(categoriesValue);
                categoriesView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(categoriesLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, categoriesView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(categoriesLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, categoriesView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(categoriesLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, categoriesView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(categoriesValue, NSLayoutAttribute.Top, NSLayoutRelation.Equal, categoriesLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(categoriesValue, NSLayoutAttribute.Left, NSLayoutRelation.Equal, categoriesView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(categoriesValue, NSLayoutAttribute.Right, NSLayoutRelation.Equal, categoriesView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(categoriesValue, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, categoriesView, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(categoriesLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, categoriesValue, NSLayoutAttribute.Height, 1f, 0f)
                });

                AddArrangedSubview(categoriesView);
            }

            protected override void UpdateRow()
            {
                if (Criteria.CountryPrefix == -1)
                {
                    countryValue.Text = Localization.GetString("search_any");
                }
                else
                {
                    var ci = countrySource.CountryByPrefix(Criteria.CountryPrefix);
                    if (string.IsNullOrWhiteSpace(ci.CCode))
                        countryValue.Text = ci.Name;
                    else
                        countryValue.Text = ci.CCode;
                }

                categoriesValue.Text = Criteria.CategoryIds.Count < 1 ? Localization.GetString("search_any") : Criteria.CategoryIds.Count.ToString();
            }

            [Export("tapped:")]
            async void Tapped(UITapGestureRecognizer recognizer)
            {
                if (recognizer.View == countryView)
                {
                    countryPicker.ReloadAllComponents();

                    if (Criteria.CountryPrefix >= 0)
                        countrySource.SelectCountryByFaxPrefix(countryPicker, Criteria.CountryPrefix);
                    else
                        countryPicker.Select(0, 0, false);

                    countryValue.UserInteractionEnabled = true;
                    countryValue.BecomeFirstResponder();
                }

                if (recognizer.View == categoriesView)
                {
                    var vc = new SelectCategoriesListViewController
                    {
                        Module = ModuleType.Contacts,
                        PreselectedItemIds = Criteria.CategoryIds
                    };
                    parentViewControllerWeakReference.Unwrap().PresentViewController(new NavigationController(vc, UIModalPresentationStyle.FormSheet), true, null);

                    var result = await vc.Result;
                    if (result == null)
                        return;

                    Criteria.CategoryIds = result;
                }

                UpdateRow();
            }

            [Export("doneTapped:")]
            void DoneTapped(UIBarButtonItem sender)
            {
                countryValue.UserInteractionEnabled = false;
                Criteria.CountryPrefix = countrySource.SelectedCountryFaxPrefix(countryPicker);

                UpdateRow();
            }

            [Export("cancelTapped:")]
            void CancelTapped(UIBarButtonItem sender)
            {
                countryValue.UserInteractionEnabled = false;
            }

            class Source : UIPickerViewDataSource, IUIPickerViewDelegate
            {
                readonly CountryInfo[] countries = ServerConfig.SystemSettings.ContactsModuleInfo.Countries.OrderBy(ci => ci.Name).ToArray();

                public override nint GetComponentCount(UIPickerView pickerView)
                {
                    return 1;
                }

                public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
                {
                    return countries.Length;
                }

                [Export("pickerView:titleForRow:forComponent:")]
                public string GetTitle(UIPickerView picker, nint row, nint component)
                {
                    var ci = countries[row];

                    if (string.IsNullOrWhiteSpace(ci.CCode))
                        return ci.Name;

                    return $"{ci.Name} [{ci.CCode}/{ci.CCode3}]";
                }

                public void SelectCountryByFaxPrefix(UIPickerView picker, int faxPrefix)
                {
                    var index = 0;
                    for (var i = 0; i < countries.Length; i++)
                        if (countries[i].FaxPrefix == faxPrefix)
                        {
                            index = i;
                            break;
                        }

                    picker.Select(index, 0, true);
                }

                public int SelectedCountryFaxPrefix(UIPickerView picker)
                {
                    var selectedIndex = picker.SelectedRowInComponent(0);
                    return countries[selectedIndex].FaxPrefix;
                }

                public CountryInfo CountryByPrefix(int faxPrefix)
                {
                    for (var i = 0; i < countries.Length; i++)
                        if (countries[i].FaxPrefix == faxPrefix)
                            return countries[i];

                    return null;
                }
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
            criteria = Serializer.DeserializeFromByteArray<SearchContactsCriteria>(coder.DecodeBytes("criteria"));
            RestoreCriteriaFromStorage = coder.DecodeBool("restoreCriteriaFromStorage");
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            return new ContactsSearchCriteriaViewController();
        }

        #endregion

    }
}