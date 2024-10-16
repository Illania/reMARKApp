using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
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
    public class DocumentsSearchCriteriaViewController : AbstractSearchCriteriaViewController, IUIViewControllerRestoration
    {
        SearchDocumentsCriteria criteria = new();
        public SavedDocumentsSearch CurrentSavedSearch { get; set; }
        DocumentSavedSearchesView savedSearchesView = null;

        public override void LoadView()
        {
            base.LoadView();

            CommonConfig.UsageAnalytics.LogEvent(new OpenSearchEvent());
            if(ServerConfig.SystemSettings?.SystemInfo?.SavedSearchesAvailable == true)
            {
                savedSearchesView = new DocumentSavedSearchesView(this);
                StackView.AddArrangedSubview(savedSearchesView);
            }
            StackView.AddArrangedSubview(new DocumentDirectionSearchView());
            StackView.AddArrangedSubview(new MessageSubjectView());
            StackView.AddArrangedSubview(new FromToView());
            StackView.AddArrangedSubview(new DateRangeView());
            StackView.AddArrangedSubview(new LineCategoriesPriorityNameView(this));
            StackView.AddArrangedSubview(new ReferenceCommentsAttachmentNameView());
            if (ServerConfig.SystemSettings.DocumentsModuleInfo.ExtraFieldInfos.Any())
                StackView.AddArrangedSubview(new ExtraFieldsView());
            StackView.AddArrangedSubview(new AttachmentsUnreadSearchView());
            if (ServerConfig.SystemSettings.DocumentsModuleInfo.HandledFieldEnabled)
                StackView.AddArrangedSubview(new HandledSearchView());

            var transparentView = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Alpha = 0,
            };
            transparentView.AddConstraint(transparentView.HeightAnchor.ConstraintEqualTo(50f));

            StackView.AddArrangedSubview(transparentView);

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(DocumentsSearchCriteriaViewController);
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

            criteria = new SearchDocumentsCriteria();
            CurrentSavedSearch = null;
            savedSearchesView?.ResetView();

            RefreshView();
            await SaveCriteria();
        }

        protected override void SearchButton_TouchUpInside(object sender, EventArgs e)
        {
            
            criteria.PartialWordSearch = PlatformConfig.Preferences.PartialWordSearch;
            criteria.MaxToFetch = PlatformConfig.Preferences.DocumentsToSearch;

            CommonConfig.Logger.Info($"Starting search... [criteria={Serializer.Serialize(criteria)}]");

            if (Integration.IsIPad())
                PresentViewController(new DocumentsSplitSearchViewController(criteria), true, null);
            else
                NavigationController.PushViewController(new DocumentsSearchResultsViewController { Criteria = criteria }, true);
        }

        protected override async void SaveButton_TouchUpInside(object sender, EventArgs e)
        {
            if(CurrentSavedSearch != null)
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
                        await Managers.SearchManager.UpdateSavedDocumentsSearchAsync(CurrentSavedSearch.Id, CurrentSavedSearch);
                    break;
                case 1:
                    await AddNewSavedSearch();
                    break;

            }
        }

        protected override async Task SaveCriteria()
        {
            try
            {
                await Managers.SearchManager.SaveLastSearchDocumentsCriteriaAsync(criteria);
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
                criteria = await Managers.SearchManager.GetLastSearchDocumentsCriteriaAsync();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to restore last search criteria", ex);
            }
        }

        protected override void RefreshView()
        {
            foreach (var view in StackView.Subviews.OfType<AbstractDocumentsSearchView>())
                view.SetCriteria(criteria);
        }

        public void ReloadCriteria(SearchDocumentsCriteria savedSearchCriteria)
        {
            this.criteria = savedSearchCriteria;
            foreach (var view in StackView.Subviews.OfType<AbstractDocumentsSearchView>())
                view.SetCriteria(this.criteria);
        }

        public void UpdateCurrentSavedSearch(SavedDocumentsSearch savedSearch)
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
                var newSavedSearch = new SavedDocumentsSearch() { Criteria = CurrentSavedSearch?.Criteria ?? criteria, Name = newName };
                var newSavedSearchSaved = await Managers.SearchManager.AddSavedDocumentsSearchAsync(newSavedSearch);
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

        #region Views

        abstract class AbstractDocumentsSearchView : AbstractSearchView
        {
            public SearchDocumentsCriteria Criteria;

            public void SetCriteria(SearchDocumentsCriteria criteria)
            {
                Criteria = criteria;
                UpdateRow();
            }
        }

        class DocumentDirectionSearchView : AbstractDocumentsSearchView
        {
            readonly UILabelScalable allView;
            readonly UILabelScalable inboxView;
            readonly UILabelScalable outboxView;
            readonly UILabelScalable draftView;

            public DocumentDirectionSearchView()
            {
                allView = new UILabelScalable
                {
                    Text = Localization.GetString("search_all").ToUpper(),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                allView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                allView.Layer.CornerRadius = CornerRadius;
                allView.Layer.MasksToBounds = true;
                AddArrangedSubview(allView);

                inboxView = new UILabelScalable
                {
                    Text = Localization.GetString("search_incoming").ToUpper(),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                inboxView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                inboxView.Layer.CornerRadius = CornerRadius;
                inboxView.Layer.MasksToBounds = true;
                AddArrangedSubview(inboxView);

                outboxView = new UILabelScalable
                {
                    Text = Localization.GetString("search_outgoing").ToUpper(),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                outboxView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                outboxView.Layer.CornerRadius = CornerRadius;
                outboxView.Layer.MasksToBounds = true;
                AddArrangedSubview(outboxView);

                draftView = new UILabelScalable
                {
                    Text = Localization.GetString("search_draft").ToUpper(),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                draftView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                draftView.Layer.CornerRadius = CornerRadius;
                draftView.Layer.MasksToBounds = true;
                AddArrangedSubview(draftView);
            }

            protected override void UpdateRow()
            {
                var directions = Criteria.Directions;

                if (!directions.Any())
                {
                    SetLabelActive(allView, true);
                    SetLabelActive(inboxView, false);
                    SetLabelActive(outboxView, false);
                    SetLabelActive(draftView, false);
                    return;
                }

                SetLabelActive(allView, false);
                SetLabelActive(inboxView, directions.Contains(DocumentDirection.Incoming));
                SetLabelActive(outboxView, directions.Contains(DocumentDirection.Outgoing));
                SetLabelActive(draftView, directions.Contains(DocumentDirection.Draft));
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                var directions = Criteria.Directions;

                if (recognizer.View == allView)
                {
                    directions.Clear();
                }
                else
                {
                    if (recognizer.View == inboxView)
                        if (directions.Contains(DocumentDirection.Incoming))
                            directions.Remove(DocumentDirection.Incoming);
                        else
                            directions.Add(DocumentDirection.Incoming);
                    else if (recognizer.View == outboxView)
                        if (directions.Contains(DocumentDirection.Outgoing))
                            directions.Remove(DocumentDirection.Outgoing);
                        else
                            directions.Add(DocumentDirection.Outgoing);
                    else if (recognizer.View == draftView)
                        if (directions.Contains(DocumentDirection.Draft))
                            directions.Remove(DocumentDirection.Draft);
                        else
                            directions.Add(DocumentDirection.Draft);

                    if (directions.Count > 2)
                        directions.Clear();
                }
                Criteria.SavedSearchFilterHash = null;

                UpdateRow();
            }
        }

        class MessageSubjectView : AbstractDocumentsSearchView
        {
            readonly UILabelScalable titleLabel;
            readonly UIView valueTextFieldAccessoryView;
            readonly UISegmentedControl valueTextFieldSegmentedControl;
            readonly UITextFieldScalable valueTextField;

            public MessageSubjectView()
            {
                var mainView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                mainView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                mainView.Layer.CornerRadius = CornerRadius;
                mainView.Layer.MasksToBounds = true;

                var icon = new UIImageView
                {
                    Image = UIImage.FromBundle("Search-Small").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                    TintColor = Theme.LightGray,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false
                };

                titleLabel = new UILabelScalable
                {
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false
                };

                valueTextFieldAccessoryView = new UIView(new CGRect(0f, 0f, 0f, 44f))
                {
                    BackgroundColor = Theme.LightGray
                };

                var labels = GetSubjectMessageCriteriaLabels();

                valueTextFieldSegmentedControl = new UISegmentedControl(labels)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                valueTextFieldSegmentedControl.AddTarget(this, new Selector("segmentedControlChanged:"), UIControlEvent.ValueChanged);
                valueTextFieldAccessoryView.AddSubview(valueTextFieldSegmentedControl);
                valueTextFieldAccessoryView.AddConstraints(new[]
                {
                    valueTextFieldSegmentedControl.CenterXAnchor.ConstraintEqualTo(valueTextFieldAccessoryView.CenterXAnchor),
                    valueTextFieldSegmentedControl.CenterYAnchor.ConstraintEqualTo(valueTextFieldAccessoryView.CenterYAnchor),
                    valueTextFieldSegmentedControl.WidthAnchor.ConstraintEqualTo(valueTextFieldAccessoryView.WidthAnchor, 1f, -10f),
                    valueTextFieldSegmentedControl.HeightAnchor.ConstraintLessThanOrEqualTo(valueTextFieldAccessoryView.HeightAnchor, 1f, -5f)
                });

                valueTextField = new UITextFieldScalable
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
                    WeakDelegate = this,
                    InputAccessoryView = valueTextFieldAccessoryView,
                    UserInteractionEnabled = false
                };
                valueTextField.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                mainView.Add(icon);
                mainView.Add(titleLabel);
                mainView.Add(valueTextField);
                mainView.AddConstraints(new[]
                {
                    icon.BottomAnchor.ConstraintEqualTo(mainView.BottomAnchor,-8f),
                    icon.LeftAnchor.ConstraintEqualTo(mainView.LeftAnchor,8f),
                    icon.WidthAnchor.ConstraintEqualTo(15f),
                    icon.HeightAnchor.ConstraintEqualTo(15f),
                    titleLabel.TopAnchor.ConstraintEqualTo(mainView.TopAnchor,4f),
                    titleLabel.LeftAnchor.ConstraintEqualTo(icon.RightAnchor,8f),
                    titleLabel.RightAnchor.ConstraintEqualTo(mainView.RightAnchor,-8f),
                    valueTextField.TopAnchor.ConstraintEqualTo(titleLabel.BottomAnchor,2f),
                    valueTextField.LeftAnchor.ConstraintEqualTo(titleLabel.LeftAnchor),
                    valueTextField.RightAnchor.ConstraintEqualTo(mainView.RightAnchor,-8f),
                    valueTextField.BottomAnchor.ConstraintEqualTo(mainView.BottomAnchor,-4f),
                    titleLabel.HeightAnchor.ConstraintEqualTo(valueTextField.HeightAnchor)
                });

                AddArrangedSubview(mainView);
            }

            private static string[] GetSubjectMessageCriteriaLabels()
            {
                if(ServerConfig.SystemSettings?.SystemInfo?.SubjectAndMessageSearchAvailable == true)
                    return new[]
                    {
                        Localization.GetString("search_subject"),
                        Localization.GetString("search_message"),
                        Localization.GetString("search_and"),
                        Localization.GetString("search_or")
                    };

                else
                    return new[]
                    {
                        Localization.GetString("search_subject"),
                        Localization.GetString("search_message"),
                        Localization.GetString("search_both"),
                    };
            }

            protected override void UpdateRow()
            {
                titleLabel.Text = Localization.GetString("search_where");

                switch (Criteria.SubjectMessageClause)
                {
                    case SubjectMessageClause.SubjectOnly:
                        titleLabel.Text += Localization.GetString("search_subject");
                        break;
                    case SubjectMessageClause.MessageOnly:
                        titleLabel.Text += Localization.GetString("search_message");
                        break;
                    case SubjectMessageClause.SubjectAndMessage:
                        titleLabel.Text += Localization.GetString("search_subject_and_message");
                        break;
                    default:
                        titleLabel.Text += Localization.GetString("search_subject_or_message");
                        break;
                }

                valueTextField.Text = Criteria.SubjectMessageField;
            }

            [Export("tapped:")]
            void Tapped(UIView sender)
            {
                valueTextField.UserInteractionEnabled = true;
                valueTextField.BecomeFirstResponder();
                SetAsActive();
            }

            [Export("segmentedControlChanged:")]
            void SegmentedControlChanged(UISegmentedControl segmentedControl)
            {
                if (ServerConfig.SystemSettings?.SystemInfo?.SubjectAndMessageSearchAvailable == true)
                {
                    switch (segmentedControl.SelectedSegment)
                    {
                        case 0:
                            Criteria.SubjectMessageClause = SubjectMessageClause.SubjectOnly;
                            break;

                        case 1:
                            Criteria.SubjectMessageClause = SubjectMessageClause.MessageOnly;
                            break;

                        case 2:
                            Criteria.SubjectMessageClause = SubjectMessageClause.SubjectAndMessage;
                            break;

                        case 3:
                            Criteria.SubjectMessageClause = SubjectMessageClause.SubjectOrMessage;
                            break;
                    }
                }
                else
                {
                    switch (segmentedControl.SelectedSegment)
                    {
                        case 0:
                            Criteria.SubjectMessageClause = SubjectMessageClause.SubjectOnly;
                            break;

                        case 1:
                            Criteria.SubjectMessageClause = SubjectMessageClause.MessageOnly;
                            break;

                        case 2:
                            Criteria.SubjectMessageClause = SubjectMessageClause.SubjectOrMessage;
                            break;
                    }
                }     

                UpdateRow();
            }

            [Export("textFieldShouldBeginEditing:")]
            bool TextFieldShouldBeginEditing(UITextFieldScalable textField)
            {
                switch (Criteria.SubjectMessageClause)
                {
                    case SubjectMessageClause.SubjectOnly:
                        valueTextFieldSegmentedControl.SelectedSegment = 0;
                        break;
                    case SubjectMessageClause.MessageOnly:
                        valueTextFieldSegmentedControl.SelectedSegment = 1;
                        break;
                    case SubjectMessageClause.SubjectOrMessage:
                        if (ServerConfig.SystemSettings?.SystemInfo?.SubjectAndMessageSearchAvailable == true)
                        {
                            valueTextFieldSegmentedControl.SelectedSegment = 3;
                            break;
                        }
                        else
                        {
                            valueTextFieldSegmentedControl.SelectedSegment = 2;
                            break;
                        }
                    case SubjectMessageClause.SubjectAndMessage:
                        valueTextFieldSegmentedControl.SelectedSegment = 2;
                        break;
                    default:
                        valueTextFieldSegmentedControl.SelectedSegment =
                            (ServerConfig.SystemSettings?.SystemInfo?.SubjectAndMessageSearchAvailable == true) ? 3 : 2;
                        break;
                }

                return true;
            }

            [Export("textFieldDidChange:")]
            void TextFieldDidChange(UITextFieldScalable textField)
            {
                Criteria.SubjectMessageField = textField.Text;
                Criteria.SavedSearchFilterHash = null;
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
                textField.UserInteractionEnabled = false;
            }
        }

        class FromToView : AbstractDocumentsSearchView
        {
            readonly UILabelScalable titleLabel;
            readonly UIView valueTextFieldAccessoryView;
            readonly UISegmentedControl valueTextFieldSegmentedControl;
            readonly UITextFieldScalable valueTextField;

            public FromToView()
            {
                var mainView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor
                };
                mainView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                mainView.Layer.CornerRadius = CornerRadius;
                mainView.Layer.MasksToBounds = true;

                var icon = new UIImageView
                {
                    Image = UIImage.FromBundle("Search-Small").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                    TintColor = Theme.LightGray,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false
                };

                titleLabel = new UILabelScalable
                {
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false
                };

                valueTextFieldAccessoryView = new UIView(new CGRect(0f, 0f, 0f, 44f))
                {
                    BackgroundColor = Theme.LightGray
                };
                valueTextFieldSegmentedControl = new UISegmentedControl(new[]
                {
                    Localization.GetString("search_from"),
                    Localization.GetString("search_to"),
                    Localization.GetString("search_both")
                })
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                valueTextFieldSegmentedControl.AddTarget(this, new Selector("segmentedControlChanged:"), UIControlEvent.ValueChanged);
                valueTextFieldAccessoryView.AddSubview(valueTextFieldSegmentedControl);
                valueTextFieldAccessoryView.AddConstraints(new[]
                {
                    valueTextFieldSegmentedControl.CenterXAnchor.ConstraintEqualTo(valueTextFieldAccessoryView.CenterXAnchor),
                    valueTextFieldSegmentedControl.CenterYAnchor.ConstraintEqualTo(valueTextFieldAccessoryView.CenterYAnchor),
                    valueTextFieldSegmentedControl.WidthAnchor.ConstraintEqualTo(valueTextFieldAccessoryView.WidthAnchor, 1f, -10f),
                    valueTextFieldSegmentedControl.HeightAnchor.ConstraintLessThanOrEqualTo(valueTextFieldAccessoryView.HeightAnchor, 1f, -5f)
                });

                valueTextField = new UITextFieldScalable
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_enter_address"), new UIStringAttributes
                    {
                        ForegroundColor = Theme.LightGray
                    }),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    WeakDelegate = this,
                    InputAccessoryView = valueTextFieldAccessoryView,
                    UserInteractionEnabled = false
                };
                valueTextField.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                mainView.Add(icon);
                mainView.Add(titleLabel);
                mainView.Add(valueTextField);
                mainView.AddConstraints(new[]
                {
                    icon.BottomAnchor.ConstraintEqualTo(mainView.BottomAnchor,-8f),
                    icon.LeftAnchor.ConstraintEqualTo(mainView.LeftAnchor,8f),
                    icon.WidthAnchor.ConstraintEqualTo(15f),
                    icon.HeightAnchor.ConstraintEqualTo(15f),
                    titleLabel.TopAnchor.ConstraintEqualTo(mainView.TopAnchor,4f),
                    titleLabel.LeftAnchor.ConstraintEqualTo(icon.RightAnchor,8f),
                    titleLabel.RightAnchor.ConstraintEqualTo(mainView.RightAnchor,-8f),
                    valueTextField.TopAnchor.ConstraintEqualTo(titleLabel.BottomAnchor,2f),
                    valueTextField.LeftAnchor.ConstraintEqualTo(titleLabel.LeftAnchor),
                    valueTextField.RightAnchor.ConstraintEqualTo(mainView.RightAnchor,-8f),
                    valueTextField.BottomAnchor.ConstraintEqualTo(mainView.BottomAnchor,-4f),
                    titleLabel.HeightAnchor.ConstraintEqualTo(valueTextField.HeightAnchor)
                });

                AddArrangedSubview(mainView);
            }

            protected override void UpdateRow()
            {
                titleLabel.Text = Localization.GetString("search_search_addresses");

                switch (Criteria.FromToClause)
                {
                    case FromToClause.FromOnly:
                        titleLabel.Text += Localization.GetString("search_from");
                        break;
                    case FromToClause.ToOnly:
                        titleLabel.Text += Localization.GetString("search_to");
                        break;
                    default:
                        titleLabel.Text += Localization.GetString("search_from_or_to");
                        break;
                }

                valueTextField.Text = Criteria.FromToField;
            }

            [Export("tapped:")]
            void Tapped(UIView sender)
            {
                valueTextField.UserInteractionEnabled = true;
                valueTextField.BecomeFirstResponder();
                SetAsActive();
            }

            [Export("segmentedControlChanged:")]
            void SegmentedControlChanged(UISegmentedControl segmentedControl)
            {
                switch (segmentedControl.SelectedSegment)
                {
                    case 0:
                        Criteria.FromToClause = FromToClause.FromOnly;
                        break;

                    case 1:
                        Criteria.FromToClause = FromToClause.ToOnly;
                        break;

                    case 2:
                        Criteria.FromToClause = FromToClause.FromOrTo;
                        break;
                }

                UpdateRow();
            }

            [Export("textFieldShouldBeginEditing:")]
            bool TextFieldShouldBeginEditing(UITextFieldScalable textField)
            {
                switch (Criteria.FromToClause)
                {
                    case FromToClause.FromOnly:
                        valueTextFieldSegmentedControl.SelectedSegment = 0;
                        break;
                    case FromToClause.ToOnly:
                        valueTextFieldSegmentedControl.SelectedSegment = 1;
                        break;
                    default:
                        valueTextFieldSegmentedControl.SelectedSegment = 2;
                        break;
                }

                return true;
            }

            [Export("textFieldDidChange:")]
            void TextFieldDidChange(UITextFieldScalable textField)
            {
                Criteria.FromToField = textField.Text;
                Criteria.SavedSearchFilterHash = null;
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
                textField.UserInteractionEnabled = false;
            }
        }

        class DateRangeView : AbstractDocumentsSearchView
        {
            readonly UIView fromView;
            readonly UILabelScalable fromLabel;
            readonly UITextFieldScalable fromValue;
            readonly UIView toView;
            readonly UILabelScalable toLabel;
            readonly UITextFieldScalable toValue;

            readonly UIToolbar fromDatePickerToolbar;
            readonly UIBarButtonItem fromDateCancelButton;
            readonly UIBarButtonItem fromDateDoneButton;
            readonly UIDatePickerStyled fromDatePicker;

            readonly UIToolbar toDatePickerToolbar;
            readonly UIBarButtonItem toDateCancelButton;
            readonly UIBarButtonItem toDateDoneButton;
            readonly UIDatePickerStyled toDatePicker;

            public DateRangeView()
            {
                var mainView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor
                };
                mainView.Layer.CornerRadius = CornerRadius;
                mainView.Layer.MasksToBounds = true;

                fromView = new UIView
                {
                    UserInteractionEnabled = true,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                fromView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                fromLabel = new UILabelScalable
                {
                    Text = Localization.GetString("search_from_date"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                fromDatePickerToolbar = new UIToolbar(new CGRect(0f, 0f, 0f, 44f))
                {
                    Items = new[]
                    {
                        fromDateCancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, this, new Selector("cancelTapped:"))
                        {
                            TintColor = Theme.DarkerBlue
                        },
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                        fromDateDoneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, this, new Selector("doneTapped:"))
                        {
                            TintColor = Theme.DarkerBlue
                        }
                    }
                };

                fromDatePicker = new UIDatePickerStyled
                {
                    Mode = UIDatePickerMode.Date
                };

                fromValue = new UITextFieldScalable
                {
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.Clear,
                    TextAlignment = UITextAlignment.Center,
                    InputView = fromDatePicker,
                    InputAccessoryView = fromDatePickerToolbar,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false
                };

                fromView.Add(fromLabel);
                fromView.Add(fromValue);
                fromView.AddConstraints(new[]
                {
                    fromLabel.TopAnchor.ConstraintEqualTo(fromView.TopAnchor,4f),
                    fromLabel.LeftAnchor.ConstraintEqualTo(fromView.LeftAnchor,4f),
                    fromLabel.RightAnchor.ConstraintEqualTo(fromView.RightAnchor,-4f),
                    fromValue.TopAnchor.ConstraintEqualTo(fromLabel.BottomAnchor,2f),
                    fromValue.LeftAnchor.ConstraintEqualTo(fromView.LeftAnchor,4f),
                    fromValue.RightAnchor.ConstraintEqualTo(fromView.RightAnchor,-4f),
                    fromValue.BottomAnchor.ConstraintEqualTo(fromView.BottomAnchor,-4f),
                    fromLabel.HeightAnchor.ConstraintEqualTo(fromValue.HeightAnchor)
                });

                var lineView = new UIView
                {
                    BackgroundColor = Theme.LightGray,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                toView = new UIView
                {
                    UserInteractionEnabled = true,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                toView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                toLabel = new UILabelScalable
                {
                    Text = Localization.GetString("search_to_date"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                toDatePickerToolbar = new UIToolbar(new CGRect(0f, 0f, 0f, 44f))
                {
                    Items = new[]
                    {
                        toDateCancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, this, new Selector("cancelTapped:"))
                        {
                            TintColor = Theme.DarkerBlue
                        },
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                        toDateDoneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, this, new Selector("doneTapped:"))
                        {
                            TintColor = Theme.DarkerBlue
                        }
                    }
                };

                toDatePicker = new UIDatePickerStyled
                {
                    Mode = UIDatePickerMode.Date
                };

                toValue = new UITextFieldScalable
                {
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.Clear,
                    TextAlignment = UITextAlignment.Center,
                    InputView = toDatePicker,
                    InputAccessoryView = toDatePickerToolbar,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false
                };

                toView.Add(toLabel);
                toView.Add(toValue);
                toView.AddConstraints(new[]
                {
                    toLabel.TopAnchor.ConstraintEqualTo(toView.TopAnchor,4f),
                    toLabel.LeftAnchor.ConstraintEqualTo(toView.LeftAnchor,4f),
                    toLabel.RightAnchor.ConstraintEqualTo(toView.RightAnchor,-4f),
                    toValue.TopAnchor.ConstraintEqualTo(toLabel.BottomAnchor,2f),
                    toValue.LeftAnchor.ConstraintEqualTo(toView.LeftAnchor,4f),
                    toValue.RightAnchor.ConstraintEqualTo(toView.RightAnchor,-4f),
                    toValue.BottomAnchor.ConstraintEqualTo(toView.BottomAnchor,-4f),
                    toLabel.HeightAnchor.ConstraintEqualTo(toValue.HeightAnchor)
                });

                mainView.AddSubview(fromView);
                mainView.AddSubview(lineView);
                mainView.AddSubview(toView);
                mainView.AddConstraints(new[]
                {
                    fromView.TopAnchor.ConstraintEqualTo(mainView.TopAnchor),
                    fromView.LeftAnchor.ConstraintEqualTo(mainView.LeftAnchor),
                    fromView.BottomAnchor.ConstraintEqualTo(mainView.BottomAnchor),
                    lineView.LeftAnchor.ConstraintEqualTo(fromView.RightAnchor,10f),
                    lineView.WidthAnchor.ConstraintEqualTo(15f),
                    lineView.HeightAnchor.ConstraintEqualTo(1f),
                    lineView.CenterYAnchor.ConstraintEqualTo(mainView.CenterYAnchor),
                    toView.TopAnchor.ConstraintEqualTo(mainView.TopAnchor),
                    toView.LeftAnchor.ConstraintEqualTo(lineView.RightAnchor,10f),
                    toView.BottomAnchor.ConstraintEqualTo(mainView.BottomAnchor),
                    toView.RightAnchor.ConstraintEqualTo(mainView.RightAnchor),
                    toView.WidthAnchor.ConstraintEqualTo(fromView.WidthAnchor)
                });

                AddArrangedSubview(mainView);
            }

            protected override void UpdateRow()
            {
                if (Criteria.DateRange == null)
                    Criteria.DateRange = new DateRange();

                var dateRange = Criteria.DateRange;

                if (!dateRange.Enabled)
                {
                    fromValue.Text = Localization.GetString("search_dash");
                    toValue.Text = Localization.GetString("search_today");

                    fromDatePicker.MinimumDate = null;
                    fromDatePicker.MaximumDate = NSDate.Now;
                    fromDatePicker.SetDate(NSDate.Now, false);

                    toDatePicker.MinimumDate = null;
                    toDatePicker.MaximumDate = NSDate.Now;
                    toDatePicker.SetDate(NSDate.Now, false);
                }
                else
                {
                    if (dateRange.StartTimestamp == -1)
                    {
                        fromDatePicker.MinimumDate = null;
                        fromDatePicker.MaximumDate = NSDate.Now;
                        fromValue.Text = Localization.GetString("search_dash");

                        fromDatePicker.SetDate(NSDate.Now, false);
                    }
                    else
                    {
                        var fromDate = dateRange.StartTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime();
                        var fromComponents = new NSDateComponents
                        {
                            Day = fromDate.Day,
                            Month = fromDate.Month,
                            Year = fromDate.Year,
                            TimeZone = NSTimeZone.FromName("UTC")
                        };

                        var fromNSDate = NSCalendar.CurrentCalendar.DateFromComponents(fromComponents);
                        fromValue.Text = DateTimeFormatter.ShortDateFormatter.StringFor(fromNSDate);

                        toDatePicker.MinimumDate = fromNSDate;
                        fromDatePicker.SetDate(fromNSDate, false);
                    }

                    if (dateRange.EndTimestamp == -1)
                    {
                        toValue.Text = Localization.GetString("search_today");

                        toDatePicker.MaximumDate = NSDate.Now;
                        toDatePicker.SetDate(NSDate.Now, false);
                    }
                    else
                    {
                        var toDate = dateRange.EndTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime();
                        var toComponents = new NSDateComponents
                        {
                            Day = toDate.Day,
                            Month = toDate.Month,
                            Year = toDate.Year,
                            TimeZone = NSTimeZone.FromName("UTC")
                        };

                        var toNSDate = NSCalendar.CurrentCalendar.DateFromComponents(toComponents);
                        toValue.Text = Utilities.DateTimeFormatter.ShortDateFormatter.StringFor(toNSDate);

                        fromDatePicker.MaximumDate = toNSDate;
                        toDatePicker.SetDate(toNSDate, false);
                    }
                }
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                if (recognizer.View == fromView)
                {
                    fromValue.UserInteractionEnabled = true;
                    fromValue.BecomeFirstResponder();
                }

                if (recognizer.View == toView)
                {
                    toValue.UserInteractionEnabled = true;
                    toValue.BecomeFirstResponder();
                }
            }

            [Export("doneTapped:")]
            void DoneTapped(UIBarButtonItem sender)
            {
                var dateRange = Criteria.DateRange;

                if (sender == fromDateDoneButton)
                {
                    fromValue.UserInteractionEnabled = false;

                    var selectedDate = fromDatePicker.Date;
                    var selectedDateComponents = NSCalendar.CurrentCalendar.Components(NSCalendarUnit.Day | NSCalendarUnit.Month | NSCalendarUnit.Year, selectedDate);
                    var fromDate = new DateTime((int)selectedDateComponents.Year, (int)selectedDateComponents.Month, (int)selectedDateComponents.Day, 0, 0, 0, DateTimeKind.Utc);

                    dateRange.Enabled = true;
                    dateRange.StartTimestamp = fromDate.ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();

                    if (dateRange.EndTimestamp < 0)
                    {
                        var now = DateTime.Now;
                        var endOfToday = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, DateTimeKind.Unspecified);
                        dateRange.EndTimestamp = endOfToday.ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();
                    }
                }

                if (sender == toDateDoneButton)
                {
                    toValue.UserInteractionEnabled = false;

                    var selectedDate = toDatePicker.Date;
                    var selectedDateComponents = NSCalendar.CurrentCalendar.Components(NSCalendarUnit.Day | NSCalendarUnit.Month | NSCalendarUnit.Year, selectedDate);
                    var toDate = new DateTime((int)selectedDateComponents.Year, (int)selectedDateComponents.Month, (int)selectedDateComponents.Day, 23, 59, 59, DateTimeKind.Utc);

                    dateRange.Enabled = true;
                    dateRange.EndTimestamp = toDate.ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();
                }
                
                Criteria.SavedSearchFilterHash = null;

                UpdateRow();
            }

            [Export("cancelTapped:")]
            void CancelTapped(UIBarButtonItem sender)
            {
                var dateRange = Criteria.DateRange;

                if (sender == fromDateCancelButton)
                    fromValue.UserInteractionEnabled = false;

                toValue.UserInteractionEnabled &= sender != toDateCancelButton;
            }
        }

        class LineCategoriesPriorityNameView : AbstractDocumentsSearchView
        {
            readonly WeakReference<UIViewController> parentViewControllerWeakReference;

            readonly UIView lineView;
            readonly UILabelScalable lineLabel;
            readonly UILabelScalable lineValue;
            readonly UIView categoriesView;
            readonly UILabelScalable categoriesLabel;
            readonly UILabelScalable categoriesValue;
            readonly UIView priorityView;
            readonly UILabelScalable priorityLabel;
            readonly UILabelScalable priorityValue;

            public LineCategoriesPriorityNameView(UIViewController parentViewController)
            {
                parentViewControllerWeakReference = parentViewController.Wrap();

                lineView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                lineView.Layer.CornerRadius = CornerRadius;
                lineView.Layer.MasksToBounds = true;
                lineView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                lineLabel = new UILabelScalable
                {
                    Text = Localization.GetString("search_lines"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                lineValue = new UILabelScalable
                {
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                lineView.Add(lineLabel);
                lineView.Add(lineValue);
                lineView.AddConstraints(new[]
                {
                    lineLabel.TopAnchor.ConstraintEqualTo(lineView.TopAnchor,4f),
                    lineLabel.LeftAnchor.ConstraintEqualTo(lineView.LeftAnchor,4f),
                    lineLabel.RightAnchor.ConstraintEqualTo(lineView.RightAnchor,-4f),
                    lineValue.TopAnchor.ConstraintEqualTo(lineLabel.BottomAnchor,2f),
                    lineValue.LeftAnchor.ConstraintEqualTo(lineView.LeftAnchor,4f),
                    lineValue.RightAnchor.ConstraintEqualTo(lineView.RightAnchor,-4f),
                    lineValue.BottomAnchor.ConstraintEqualTo(lineView.BottomAnchor,-4f),
                    lineLabel.HeightAnchor.ConstraintEqualTo(lineValue.HeightAnchor)
                });

                AddArrangedSubview(lineView);

                categoriesView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                categoriesView.Layer.CornerRadius = CornerRadius;
                categoriesView.Layer.MasksToBounds = true;
                categoriesView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                categoriesLabel = new UILabelScalable
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

                categoriesValue = new UILabelScalable
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
                    categoriesLabel.TopAnchor.ConstraintEqualTo(categoriesView.TopAnchor,4f),
                    categoriesLabel.LeftAnchor.ConstraintEqualTo(categoriesView.LeftAnchor,4f),
                    categoriesLabel.RightAnchor.ConstraintEqualTo(categoriesView.RightAnchor,-4f),
                    categoriesValue.TopAnchor.ConstraintEqualTo(categoriesLabel.BottomAnchor,2f),
                    categoriesValue.LeftAnchor.ConstraintEqualTo(categoriesView.LeftAnchor,4f),
                    categoriesValue.RightAnchor.ConstraintEqualTo(categoriesView.RightAnchor,-4f),
                    categoriesValue.BottomAnchor.ConstraintEqualTo(categoriesView.BottomAnchor,-4f),
                    categoriesLabel.HeightAnchor.ConstraintEqualTo(categoriesValue.HeightAnchor)
                });

                AddArrangedSubview(categoriesView);

                priorityView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                priorityView.Layer.CornerRadius = CornerRadius;
                priorityView.Layer.MasksToBounds = true;
                priorityView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                priorityLabel = new UILabelScalable
                {
                    Text = Localization.GetString("search_priority"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                priorityValue = new UILabelScalable
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
                priorityView.Add(priorityLabel);
                priorityView.Add(priorityValue);
                priorityView.AddConstraints(new[]
                {
                    priorityLabel.TopAnchor.ConstraintEqualTo(priorityView.TopAnchor,4f),
                    priorityLabel.LeftAnchor.ConstraintEqualTo(priorityView.LeftAnchor,4f),
                    priorityLabel.RightAnchor.ConstraintEqualTo(priorityView.RightAnchor,-4f),
                    priorityValue.TopAnchor.ConstraintEqualTo(priorityLabel.BottomAnchor,2f),
                    priorityValue.LeftAnchor.ConstraintEqualTo(priorityView.LeftAnchor,4f),
                    priorityValue.RightAnchor.ConstraintEqualTo(priorityView.RightAnchor,-4f),
                    priorityValue.BottomAnchor.ConstraintEqualTo(priorityView.BottomAnchor,-4f),
                    priorityLabel.HeightAnchor.ConstraintEqualTo(priorityValue.HeightAnchor)
                });

                AddArrangedSubview(priorityView);
            }

            protected override void UpdateRow()
            {
                lineValue.Text = Criteria.LineGuids.Count < 1 ? Localization.GetString("search_any") : Criteria.LineGuids.Count.ToString();
                categoriesValue.Text = Criteria.CategoryIds.Count < 1 ? Localization.GetString("search_any") : Criteria.CategoryIds.Count.ToString();
                priorityValue.Text = Criteria.Priorities.Count < 1 ? Localization.GetString("search_any") : Criteria.Priorities.Count.ToString();
            }

            [Export("tapped:")]
            async void Tapped(UITapGestureRecognizer recognizer)
            {
                if (recognizer.View == lineView)
                {
                    var data = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Union(ServerConfig.SystemSettings.DocumentsModuleInfo.IncomingLines, LambdaEqualityComparer<Line>.Create(l => l.Guid))
                                           .OrderBy(l => l.Name).ToArray();
                    var preselected = data.Where(l => Criteria.LineGuids.Contains(l.Guid)).ToArray();

                    var result = await Dialogs.ShowMultiSelectViewControllerAsync(parentViewControllerWeakReference.Unwrap(),
                                                                                  Localization.GetString("search_lines"),
                                                                                  data,
                                                                                  preselected,
                                                                                  l => l.Name,
                                                                                  LambdaEqualityComparer<Line>.Create(l => l.Guid),
                                                                                  false);
                    if (result == null)
                        return;

                    Criteria.LineGuids = result.Select(l => l.Guid).ToList();
                }

                if (recognizer.View == categoriesView)
                {
                    var vc = new SelectCategoriesListViewController
                    {
                        Module = ModuleType.Documents,
                        PreselectedItemIds = Criteria.CategoryIds
                    };
                    parentViewControllerWeakReference.Unwrap()?.PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);

                    var result = await vc.Result;
                    if (result == null)
                        return;

                    Criteria.CategoryIds = result;
                }

                if (recognizer.View == priorityView)
                {
                    var data = new[]
                    {
                        Priority.Urgent,
                        Priority.Normal,
                        Priority.Low
                    };
                    var preselected = data.Where(p => Criteria.Priorities.Contains(p)).ToArray();

                    Func<Priority, string> description = p =>
                    {
                        switch (p)
                        {
                            case Priority.Urgent:
                                return Localization.GetString("priority_urgent");
                            case Priority.Normal:
                                return Localization.GetString("priority_normal");
                            case Priority.Low:
                                return Localization.GetString("priority_low");
                            default:
                                return string.Empty;
                        }
                    };

                    var result = await Dialogs.ShowMultiSelectViewControllerAsync(parentViewControllerWeakReference.Unwrap(),
                                                                                  Localization.GetString("priority"),
                                                                                  data,
                                                                                  preselected,
                                                                                  description,
                                                                                  LambdaEqualityComparer<Priority>.Create(p => p),
                                                                                  false);
                    if (result == null)
                        return;

                    Criteria.Priorities = result.ToList();
                }
                
                Criteria.SavedSearchFilterHash = null;

                UpdateRow();
            }
        }

        class ReferenceCommentsAttachmentNameView : AbstractDocumentsSearchView
        {
            readonly UIView referenceView;
            readonly UILabelScalable referenceLabel;
            readonly UITextFieldScalable referenceTextField;
            readonly UIView commentView;
            readonly UILabelScalable commentLabel;
            readonly UITextFieldScalable commentTextField;
            readonly UIView attachmentNameView;
            readonly UILabelScalable attachmentNameLabel;
            readonly UITextFieldScalable attachmentNameTextField;

            public ReferenceCommentsAttachmentNameView()
            {
                referenceView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                referenceView.Layer.CornerRadius = CornerRadius;
                referenceView.Layer.MasksToBounds = true;
                referenceView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                referenceLabel = new UILabelScalable
                {
                    Text = Localization.GetString("search_ref"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                referenceTextField = new UITextFieldScalable
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
                referenceTextField.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                referenceView.Add(referenceLabel);
                referenceView.Add(referenceTextField);
                referenceView.AddConstraints(new[]
                {
                    referenceLabel.TopAnchor.ConstraintEqualTo(referenceView.TopAnchor,4f),
                    referenceLabel.LeftAnchor.ConstraintEqualTo(referenceView.LeftAnchor,4f),
                    referenceLabel.RightAnchor.ConstraintEqualTo(referenceView.RightAnchor,-4f),
                    referenceTextField.TopAnchor.ConstraintEqualTo(referenceLabel.BottomAnchor,2f),
                    referenceTextField.LeftAnchor.ConstraintEqualTo(referenceView.LeftAnchor,4f),
                    referenceTextField.RightAnchor.ConstraintEqualTo(referenceView.RightAnchor,-4f),
                    referenceTextField.BottomAnchor.ConstraintEqualTo(referenceView.BottomAnchor,-4f),
                    referenceLabel.HeightAnchor.ConstraintEqualTo(referenceTextField.HeightAnchor)
                });

                AddArrangedSubview(referenceView);

                commentView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                commentView.Layer.CornerRadius = CornerRadius;
                commentView.Layer.MasksToBounds = true;
                commentView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                commentLabel = new UILabelScalable
                {
                    Text = Localization.GetString("search_comments"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                commentTextField = new UITextFieldScalable
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
                commentTextField.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                commentView.Add(commentLabel);
                commentView.Add(commentTextField);
                commentView.AddConstraints(new[]
                {
                    commentLabel.TopAnchor.ConstraintEqualTo(commentView.TopAnchor,4f),
                    commentLabel.LeftAnchor.ConstraintEqualTo(commentView.LeftAnchor,4f),
                    commentLabel.RightAnchor.ConstraintEqualTo(commentView.RightAnchor,-4f),
                    commentTextField.TopAnchor.ConstraintEqualTo(commentLabel.BottomAnchor,2f),
                    commentTextField.LeftAnchor.ConstraintEqualTo(commentView.LeftAnchor,4f),
                    commentTextField.RightAnchor.ConstraintEqualTo(commentView.RightAnchor,-4f),
                    commentTextField.BottomAnchor.ConstraintEqualTo(commentView.BottomAnchor,-4f),
                    commentLabel.HeightAnchor.ConstraintEqualTo(commentTextField.HeightAnchor)
                });

                AddArrangedSubview(commentView);

                attachmentNameView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                attachmentNameView.Layer.CornerRadius = CornerRadius;
                attachmentNameView.Layer.MasksToBounds = true;
                attachmentNameView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                attachmentNameLabel = new UILabelScalable
                {
                    Text = Localization.GetString("search_attachments"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                attachmentNameTextField = new UITextFieldScalable
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
                attachmentNameTextField.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                attachmentNameView.Add(attachmentNameLabel);
                attachmentNameView.Add(attachmentNameTextField);
                attachmentNameView.AddConstraints(new[]
                {
                    attachmentNameLabel.TopAnchor.ConstraintEqualTo(attachmentNameView.TopAnchor,4f),
                    attachmentNameLabel.LeftAnchor.ConstraintEqualTo(attachmentNameView.LeftAnchor,4f),
                    attachmentNameLabel.RightAnchor.ConstraintEqualTo(attachmentNameView.RightAnchor,-4f),
                    attachmentNameTextField.TopAnchor.ConstraintEqualTo(attachmentNameLabel.BottomAnchor,2f),
                    attachmentNameTextField.LeftAnchor.ConstraintEqualTo(attachmentNameView.LeftAnchor,4f),
                    attachmentNameTextField.RightAnchor.ConstraintEqualTo(attachmentNameView.RightAnchor,-4f),
                    attachmentNameTextField.BottomAnchor.ConstraintEqualTo(attachmentNameView.BottomAnchor,-4f),
                    attachmentNameLabel.HeightAnchor.ConstraintEqualTo(attachmentNameTextField.HeightAnchor)
                });

                AddArrangedSubview(attachmentNameView);
            }

            protected override void UpdateRow()
            {
                referenceTextField.Text = Criteria.Reference;
                commentTextField.Text = Criteria.Comment;
                attachmentNameTextField.Text = Criteria.AttachmentName;
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                if (recognizer.View == referenceView)
                    AnimateNotify(AnimationLength,
                        () =>
                        {
                            commentView.Hidden = true;
                            attachmentNameView.Hidden = true;
                        },
                        ch =>
                        {
                            referenceTextField.UserInteractionEnabled = true;
                            referenceTextField.BecomeFirstResponder();
                        });

                if (recognizer.View == commentView)
                    AnimateNotify(AnimationLength,
                        () =>
                        {
                            referenceView.Hidden = true;
                            attachmentNameView.Hidden = true;
                        },
                        ch =>
                        {
                            commentTextField.UserInteractionEnabled = true;
                            commentTextField.BecomeFirstResponder();
                        });

                if (recognizer.View == attachmentNameView)
                    AnimateNotify(AnimationLength,
                        () =>
                        {
                            referenceView.Hidden = true;
                            commentView.Hidden = true;
                        },
                        ch =>
                        {
                            attachmentNameTextField.UserInteractionEnabled = true;
                            attachmentNameTextField.BecomeFirstResponder();
                        });

                UpdateRow();
                SetAsActive();
            }

            [Export("textFieldDidChange:")]
            void TextFieldDidChange(UITextFieldScalable textField)
            {
                if (textField == referenceTextField)
                    Criteria.Reference = textField.Text;
                if (textField == commentTextField)
                    Criteria.Comment = textField.Text;
                if (textField == attachmentNameTextField)
                    Criteria.AttachmentName = textField.Text;
                
                Criteria.SavedSearchFilterHash = null;
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
                if (textField == referenceTextField)
                    AnimateNotify(AnimationLength,
                        () =>
                        {
                            commentView.Hidden = false;
                            attachmentNameView.Hidden = false;
                        },
                        ch =>
                        {
                            referenceTextField.ResignFirstResponder();
                            referenceTextField.UserInteractionEnabled = false;
                        });

                if (textField == commentTextField)
                    AnimateNotify(AnimationLength,
                        () =>
                        {
                            referenceView.Hidden = false;
                            attachmentNameView.Hidden = false;
                        },
                        ch =>
                        {
                            commentTextField.ResignFirstResponder();
                            commentTextField.UserInteractionEnabled = false;
                        });

                if (textField == attachmentNameTextField)
                    AnimateNotify(AnimationLength,
                        () =>
                        {
                            referenceView.Hidden = false;
                            commentView.Hidden = false;
                        },
                        ch =>
                        {
                            attachmentNameTextField.ResignFirstResponder();
                            attachmentNameTextField.UserInteractionEnabled = false;
                        });
            }
        }

        class ExtraFieldsView : AbstractDocumentsSearchView
        {
            readonly UIView view;
            readonly UILabelScalable label;
            readonly UITextFieldScalable text;

            public ExtraFieldsView()
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
                    Text = Localization.GetString("search_extra_fields"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                text = new UITextFieldScalable
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
                text.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
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
                text.Text = Criteria.ExtraFields;
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
                Criteria.ExtraFields = textField.Text;
                Criteria.SavedSearchFilterHash = null;
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

        class AttachmentsUnreadSearchView : AbstractDocumentsSearchView
        {
            readonly UILabelScalable attachmentsView;
            readonly UILabelScalable unreadView;

            public AttachmentsUnreadSearchView()
            {
                attachmentsView = new UILabelScalable
                {
                    Text = Localization.GetString("search_with_attachments").ToUpper(),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                attachmentsView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                attachmentsView.Layer.CornerRadius = CornerRadius;
                attachmentsView.Layer.MasksToBounds = true;
                AddArrangedSubview(attachmentsView);

                unreadView = new UILabelScalable
                {
                    Text = Localization.GetString("search_unread").ToUpper(),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                unreadView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                unreadView.Layer.CornerRadius = CornerRadius;
                unreadView.Layer.MasksToBounds = true;
                AddArrangedSubview(unreadView);
            }

            protected override void UpdateRow()
            {
                SetLabelActive(attachmentsView, Criteria.HavingAttachmentsOnly);
                SetLabelActive(unreadView, Criteria.UnreadOnly);
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                if (recognizer.View == attachmentsView)
                    Criteria.HavingAttachmentsOnly = !Criteria.HavingAttachmentsOnly;
                if (recognizer.View == unreadView)
                    Criteria.UnreadOnly = !Criteria.UnreadOnly;
                
                Criteria.SavedSearchFilterHash = null;

                UpdateRow();
            }
        }

        class HandledSearchView : AbstractDocumentsSearchView
        {
            readonly UILabelScalable allView;
            readonly UILabelScalable handledView;
            readonly UILabelScalable unhadledView;

            public HandledSearchView()
            {
                allView = new UILabelScalable
                {
                    Text = Localization.GetString("search_all").ToUpper(),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                allView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                allView.Layer.CornerRadius = CornerRadius;
                allView.Layer.MasksToBounds = true;
                AddArrangedSubview(allView);

                handledView = new UILabelScalable
                {
                    Text = Localization.GetString("search_handled").ToUpper(),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                handledView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                handledView.Layer.CornerRadius = CornerRadius;
                handledView.Layer.MasksToBounds = true;
                AddArrangedSubview(handledView);

                unhadledView = new UILabelScalable
                {
                    Text = Localization.GetString("search_unhandled").ToUpper(),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                unhadledView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                unhadledView.Layer.CornerRadius = CornerRadius;
                unhadledView.Layer.MasksToBounds = true;
                AddArrangedSubview(unhadledView);
            }

            protected override void UpdateRow()
            {
                SetLabelActive(allView, Criteria.Handled == null);
                SetLabelActive(handledView, Criteria.Handled == true);
                SetLabelActive(unhadledView, Criteria.Handled == false);
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                if (recognizer.View == allView)
                    Criteria.Handled = null;
                else if (recognizer.View == handledView)
                    Criteria.Handled = true;
                else
                    Criteria.Handled = false;

                Criteria.SavedSearchFilterHash = null;
                UpdateRow();
            }
        }

        class DocumentSavedSearchesView : AbstractDocumentsSearchView
        {
            readonly WeakReference<DocumentsSearchCriteriaViewController> parentViewControllerWeakReference;

            readonly UIView view;
            readonly UILabelScalable label;
            readonly UILabelScalable text;
            string currentSearchName;

            public DocumentSavedSearchesView(DocumentsSearchCriteriaViewController parentViewController)
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
                if(!string.IsNullOrEmpty(currentSearchName))
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
                var vc = new SavedDocumentSearchesViewController();
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



        #endregion

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
            criteria = Serializer.DeserializeFromByteArray<SearchDocumentsCriteria>(coder.DecodeBytes("criteria"));
            RestoreCriteriaFromStorage = coder.DecodeBool("restoreCriteriaFromStorage");
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            return new DocumentsSearchCriteriaViewController();
        }

        #endregion

    }
}
