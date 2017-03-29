//
// Project: Mark5.Mobile.IOS
// File: SearchDocumentsViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class SearchDocumentsViewController : AbstractViewController
    {

        UIBarButtonItem closeItem;
        UIBarButtonItem resetItem;
        UIScrollView scrollView;
        UIStackView stackView;

        SearchDocumentsCriteria criteria = new SearchDocumentsCriteria();

        public override void LoadView()
        {
            base.LoadView();

            AutomaticallyAdjustsScrollViewInsets = true;

            Title = Localization.GetString("search");

            closeItem = new UIBarButtonItem
            {
                Title = Localization.GetString("close")
            };
            NavigationItem.SetLeftBarButtonItem(closeItem, false);

            resetItem = new UIBarButtonItem
            {
                Title = Localization.GetString("reset")
            };
            NavigationItem.SetRightBarButtonItem(resetItem, false);

            scrollView = new UIScrollView
            {
                ShowsVerticalScrollIndicator = false,
                ShowsHorizontalScrollIndicator = false,
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.DarkerBlue
            };
            View.AddSubview(scrollView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });

            stackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                LayoutMargins = new UIEdgeInsets(10f, 10f, 10f, 10f),
                LayoutMarginsRelativeArrangement = true,
                Spacing = 10f,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            scrollView.AddSubview(stackView);
            scrollView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Width, 1f, 0f)
            });

            stackView.AddArrangedSubview(new DocumentDirectionSearchView());
            stackView.AddArrangedSubview(new DateRangeView());
            stackView.AddArrangedSubview(new LineCategoriesPriorityNameView());
            stackView.AddArrangedSubview(new ReferenceCommentsAttachmentNameView());
            stackView.AddArrangedSubview(new AttachmentsUnreadSearchView());
            // TODO if (ServerConfig.SystemSettings.DocumentsModuleInfo.HandledFieldEnabled)
            stackView.AddArrangedSubview(new HandledSearchView());

            foreach (var view in stackView.Subviews.OfType<AbstractSearchView>())
                view.SetCriteria(criteria);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExtendedLayoutIncludesOpaqueBars = true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            closeItem.Clicked += CloseItem_Clicked;
            resetItem.Clicked += ResetItem_Clicked;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            closeItem.Clicked -= CloseItem_Clicked;
            resetItem.Clicked -= ResetItem_Clicked;
        }

        void CloseItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);

        void ResetItem_Clicked(object sender, EventArgs e)
        {
            criteria = new SearchDocumentsCriteria();

            foreach (var view in stackView.Subviews.OfType<AbstractSearchView>())
                view.SetCriteria(criteria);
        }

        abstract class AbstractSearchView : UIStackView
        {

            protected const float CorderRadius = 4f;
            protected const float InnerMargin = 2f;
            protected const float RowHeight = 50f;
            protected const float AnimationLength = .1f;

            protected static readonly UIColor LabelTextColor = Theme.LightBlue;
            protected static readonly UIColor InactiveTextColor = Theme.LightGray;
            protected static readonly UIColor ActiveTextColor = Theme.DarkerBlue;
            protected static readonly UIColor InactiveBackgroundColor = Theme.DarkBlue;
            protected static readonly UIColor ActiveBackgroundColor = Theme.LightBlue;
            protected static readonly UIFont Font = Theme.DefaultFont;

            protected SearchDocumentsCriteria Criteria;

            protected AbstractSearchView()
            {
                AddConstraint(NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, RowHeight));

                Axis = UILayoutConstraintAxis.Horizontal;
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.FillEqually;
                Spacing = InnerMargin;
            }

            public void SetCriteria(SearchDocumentsCriteria criteria)
            {
                Criteria = criteria;
                UpdateRow();
            }

            protected abstract void UpdateRow();

            protected void SetLabelActive(UILabel label, bool active)
            {
                TransitionNotify(label, AnimationLength, UIViewAnimationOptions.TransitionCrossDissolve, () =>
                {
                    label.TextColor = active ? ActiveTextColor : InactiveTextColor;
                    label.BackgroundColor = active ? ActiveBackgroundColor : InactiveBackgroundColor;
                }, null);
            }
        }

        class DocumentDirectionSearchView : AbstractSearchView
        {

            readonly UILabel allView;
            readonly UILabel inboxView;
            readonly UILabel outboxView;
            readonly UILabel draftView;

            public DocumentDirectionSearchView()
            {
                allView = new UILabel
                {
                    Text = "ALL",
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                allView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                allView.Layer.CornerRadius = CorderRadius;
                allView.Layer.MasksToBounds = true;
                AddArrangedSubview(allView);

                inboxView = new UILabel
                {
                    Text = "INBOX",
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                inboxView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                inboxView.Layer.CornerRadius = CorderRadius;
                inboxView.Layer.MasksToBounds = true;
                AddArrangedSubview(inboxView);

                outboxView = new UILabel
                {
                    Text = "OUTBOX",
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                outboxView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                outboxView.Layer.CornerRadius = CorderRadius;
                outboxView.Layer.MasksToBounds = true;
                AddArrangedSubview(outboxView);

                draftView = new UILabel
                {
                    Text = "DRAFT",
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                draftView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                draftView.Layer.CornerRadius = CorderRadius;
                draftView.Layer.MasksToBounds = true;
                AddArrangedSubview(draftView);
            }

            protected override void UpdateRow()
            {
                var directions = Criteria.Directions;

                if (!directions.Any())
                    directions.AddRange(new[] { DocumentDirection.Incoming, DocumentDirection.Outgoing, DocumentDirection.Draft });

                if (directions.Count > 2)
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
                    if (directions.Count > 2)
                        directions.Clear();

                    if (recognizer.View == inboxView)
                        directions.Add(DocumentDirection.Incoming);
                    else if (recognizer.View == outboxView)
                        directions.Add(DocumentDirection.Outgoing);
                    else if (recognizer.View == draftView)
                        directions.Add(DocumentDirection.Draft);
                }

                UpdateRow();
            }
        }

        class DateRangeView : AbstractSearchView
        {

            readonly UIView fromView;
            readonly UILabel fromLabel;
            readonly UITextField fromValue;
            readonly UIView toView;
            readonly UILabel toLabel;
            readonly UITextField toValue;

            readonly UIToolbar fromDatePickerToolbar;
            readonly UIBarButtonItem fromDateCancelButton;
            readonly UIBarButtonItem fromDateDoneButton;
            readonly UIDatePicker fromDatePicker;

            readonly UIToolbar toDatePickerToolbar;
            readonly UIBarButtonItem toDateCancelButton;
            readonly UIBarButtonItem toDateDoneButton;
            readonly UIDatePicker toDatePicker;

            public DateRangeView()
            {
                var mainView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor
                };
                mainView.Layer.CornerRadius = CorderRadius;
                mainView.Layer.MasksToBounds = true;

                fromView = new UIView
                {
                    UserInteractionEnabled = true,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                fromView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                fromLabel = new UILabel
                {
                    Text = "From date",
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                fromDatePickerToolbar = new UIToolbar(new CGRect(0f, 0f, 0f, 44f))
                {
                    Items = new[]
                    {
                        fromDateCancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, this, new Selector("cancelTapped:")),
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                        fromDateDoneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, this, new Selector("doneTapped:"))
                    }
                };

                fromDatePicker = new UIDatePicker
                {
                    Mode = UIDatePickerMode.Date,
                };

                fromValue = new UITextField
                {
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = UIColor.Clear,
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
                    NSLayoutConstraint.Create(fromLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, fromView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(fromLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, fromView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(fromLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, fromView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(fromValue, NSLayoutAttribute.Top, NSLayoutRelation.Equal, fromLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(fromValue, NSLayoutAttribute.Left, NSLayoutRelation.Equal, fromView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(fromValue, NSLayoutAttribute.Right, NSLayoutRelation.Equal, fromView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(fromValue, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, fromView, NSLayoutAttribute.Bottom, 1f, -4f)
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

                toLabel = new UILabel
                {
                    Text = "To date",
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                toDatePickerToolbar = new UIToolbar(new CGRect(0f, 0f, 0f, 44f))
                {
                    Items = new[]
                    {
                        toDateCancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, this, new Selector("cancelTapped:")),
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                        toDateDoneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, this, new Selector("doneTapped:"))
                    }
                };

                toDatePicker = new UIDatePicker
                {
                    Mode = UIDatePickerMode.Date,
                };

                toValue = new UITextField
                {
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = UIColor.Clear,
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
                    NSLayoutConstraint.Create(toLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, toView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(toLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, toView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(toLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, toView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(toValue, NSLayoutAttribute.Top, NSLayoutRelation.Equal, toLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(toValue, NSLayoutAttribute.Left, NSLayoutRelation.Equal, toView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(toValue, NSLayoutAttribute.Right, NSLayoutRelation.Equal, toView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(toValue, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, toView, NSLayoutAttribute.Bottom, 1f, -4f)
                });

                mainView.AddSubview(fromView);
                mainView.AddSubview(lineView);
                mainView.AddSubview(toView);
                mainView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(fromView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, mainView, NSLayoutAttribute.Top, 1f, 0f),
                    NSLayoutConstraint.Create(fromView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, mainView, NSLayoutAttribute.Left, 1f, 0f),
                    NSLayoutConstraint.Create(fromView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, mainView, NSLayoutAttribute.Bottom, 1f, 0f),
                    NSLayoutConstraint.Create(lineView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, fromView, NSLayoutAttribute.Right, 1f, 10f),
                    NSLayoutConstraint.Create(lineView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 15f),
                    NSLayoutConstraint.Create(lineView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 1f),
                    NSLayoutConstraint.Create(lineView, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, mainView, NSLayoutAttribute.CenterY, 1f, 0f),
                    NSLayoutConstraint.Create(toView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, mainView, NSLayoutAttribute.Top, 1f, 0f),
                    NSLayoutConstraint.Create(toView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, lineView, NSLayoutAttribute.Right, 1f, 10f),
                    NSLayoutConstraint.Create(toView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, mainView, NSLayoutAttribute.Bottom, 1f, 0f),
                    NSLayoutConstraint.Create(toView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, mainView, NSLayoutAttribute.Right, 1f, 0f),
                    NSLayoutConstraint.Create(toView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, fromView, NSLayoutAttribute.Width, 1f, 0f)
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
                    fromValue.Text = "-";
                    toValue.Text = "Today";

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
                        fromValue.Text = "-";

                        fromDatePicker.SetDate(NSDate.Now, false);
                    }
                    else
                    {
                        var fromDate = dateRange.StartTimestamp.ConvertTimestampMillisecondsToDateTime();
                        var fromComponents = new NSDateComponents
                        {
                            Day = fromDate.Day,
                            Month = fromDate.Month,
                            Year = fromDate.Year,
                            TimeZone = NSTimeZone.FromName("UTC")
                        };

                        var fromNSDate = NSCalendar.CurrentCalendar.DateFromComponents(fromComponents);
                        fromValue.Text = Utilities.DateTimeUtils.DateFormatter.StringFor(fromNSDate);

                        toDatePicker.MinimumDate = fromNSDate;
                        fromDatePicker.SetDate(fromNSDate, false);
                    }

                    if (dateRange.EndTimestamp == -1)
                    {
                        toValue.Text = "Today";

                        toDatePicker.MaximumDate = NSDate.Now;
                        toDatePicker.SetDate(NSDate.Now, false);
                    }
                    else
                    {
                        var toDate = dateRange.EndTimestamp.ConvertTimestampMillisecondsToDateTime();
                        var toComponents = new NSDateComponents
                        {
                            Day = toDate.Day,
                            Month = toDate.Month,
                            Year = toDate.Year,
                            TimeZone = NSTimeZone.FromName("UTC")
                        };

                        var toNSDate = NSCalendar.CurrentCalendar.DateFromComponents(toComponents);
                        toValue.Text = Utilities.DateTimeUtils.DateFormatter.StringFor(toNSDate);

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
                    dateRange.StartTimestamp = fromDate.ConvertDateTimeToTimestampMilliseconds();
                }

                if (sender == toDateDoneButton)
                {
                    toValue.UserInteractionEnabled = false;

                    var selectedDate = toDatePicker.Date;
                    var selectedDateComponents = NSCalendar.CurrentCalendar.Components(NSCalendarUnit.Day | NSCalendarUnit.Month | NSCalendarUnit.Year, selectedDate);
                    var toDate = new DateTime((int)selectedDateComponents.Year, (int)selectedDateComponents.Month, (int)selectedDateComponents.Day, 23, 59, 59, DateTimeKind.Utc);

                    dateRange.Enabled = true;
                    dateRange.EndTimestamp = toDate.ConvertDateTimeToTimestampMilliseconds();
                }

                UpdateRow();
            }

            [Export("cancelTapped:")]
            void CancelTapped(UIBarButtonItem sender)
            {
                var dateRange = Criteria.DateRange;

                if (sender == fromDateCancelButton)
                    fromValue.UserInteractionEnabled = false;

                if (sender == toDateCancelButton)
                    toValue.UserInteractionEnabled = false;
            }
        }

        class LineCategoriesPriorityNameView : AbstractSearchView
        {

            readonly UIView lineView;
            readonly UILabel lineLabel;
            readonly UILabel lineValue;
            readonly UIView categoriesView;
            readonly UILabel categoriesLabel;
            readonly UILabel categoriesValue;
            readonly UIView priorityView;
            readonly UILabel priorityLabel;
            readonly UILabel priorityValue;

            public LineCategoriesPriorityNameView()
            {
                lineView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                lineView.Layer.CornerRadius = CorderRadius;
                lineView.Layer.MasksToBounds = true;
                lineView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                lineLabel = new UILabel
                {
                    Text = "Mailbox",
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                lineValue = new UILabel
                {
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                lineView.Add(lineLabel);
                lineView.Add(lineValue);
                lineView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(lineLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, lineView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(lineLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, lineView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(lineLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, lineView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(lineValue, NSLayoutAttribute.Top, NSLayoutRelation.Equal, lineLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(lineValue, NSLayoutAttribute.Left, NSLayoutRelation.Equal, lineView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(lineValue, NSLayoutAttribute.Right, NSLayoutRelation.Equal, lineView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(lineValue, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, lineView, NSLayoutAttribute.Bottom, 1f, -4f)
                });

                AddArrangedSubview(lineView);

                categoriesView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                categoriesView.Layer.CornerRadius = CorderRadius;
                categoriesView.Layer.MasksToBounds = true;
                categoriesView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                categoriesLabel = new UILabel
                {
                    Text = "Categories",
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false
                };

                categoriesValue = new UILabel
                {
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false
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
                    NSLayoutConstraint.Create(categoriesValue, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, categoriesView, NSLayoutAttribute.Bottom, 1f, -4f)
                });

                AddArrangedSubview(categoriesView);

                priorityView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                priorityView.Layer.CornerRadius = CorderRadius;
                priorityView.Layer.MasksToBounds = true;
                priorityView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                priorityLabel = new UILabel
                {
                    Text = "Priority",
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false
                };

                priorityValue = new UILabel
                {
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false
                };
                priorityView.Add(priorityLabel);
                priorityView.Add(priorityValue);
                priorityView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(priorityLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, priorityView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(priorityLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, priorityView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(priorityLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, priorityView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(priorityValue, NSLayoutAttribute.Top, NSLayoutRelation.Equal, priorityLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(priorityValue, NSLayoutAttribute.Left, NSLayoutRelation.Equal, priorityView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(priorityValue, NSLayoutAttribute.Right, NSLayoutRelation.Equal, priorityView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(priorityValue, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, priorityView, NSLayoutAttribute.Bottom, 1f, -4f)
                });

                AddArrangedSubview(priorityView);
            }

            protected override void UpdateRow()
            {
                lineValue.Text = Criteria.LineGuids.Count < 1 ? "Any" : Criteria.LineGuids.Count.ToString();
                categoriesValue.Text = Criteria.CategoryIds.Count < 1 ? "Any" : Criteria.CategoryIds.Count.ToString();
                priorityValue.Text = Criteria.Priorities.Count < 1 ? "Any" : Criteria.Priorities.Count.ToString();
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                if (recognizer.View == lineView)
                {
                    // TODO
                }

                if (recognizer.View == categoriesView)
                {
                    // TODO
                }

                if (recognizer.View == priorityView)
                {
                    // TODO
                }

                UpdateRow();
            }
        }

        class ReferenceCommentsAttachmentNameView : AbstractSearchView
        {

            readonly UIView referenceView;
            readonly UILabel referenceLabel;
            readonly UITextField referenceTextField;
            readonly UIView commentView;
            readonly UILabel commentLabel;
            readonly UITextField commentTextField;
            readonly UIView attachmentNameView;
            readonly UILabel attachmentNameLabel;
            readonly UITextField attachmentNameTextField;

            public ReferenceCommentsAttachmentNameView()
            {
                referenceView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                referenceView.Layer.CornerRadius = CorderRadius;
                referenceView.Layer.MasksToBounds = true;
                referenceView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                referenceLabel = new UILabel
                {
                    Text = "Reference no.",
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false
                };

                referenceTextField = new UITextField
                {
                    AttributedPlaceholder = new NSAttributedString("Type...", new UIStringAttributes { ForegroundColor = Theme.LightGray }),
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
                    NSLayoutConstraint.Create(referenceLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, referenceView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(referenceLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, referenceView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(referenceLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, referenceView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(referenceTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, referenceLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(referenceTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, referenceView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(referenceTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, referenceView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(referenceTextField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, referenceView, NSLayoutAttribute.Bottom, 1f, -4f)
                });

                AddArrangedSubview(referenceView);

                commentView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                commentView.Layer.CornerRadius = CorderRadius;
                commentView.Layer.MasksToBounds = true;
                commentView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                commentLabel = new UILabel
                {
                    Text = "Comments",
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false
                };

                commentTextField = new UITextField
                {
                    AttributedPlaceholder = new NSAttributedString("Type...", new UIStringAttributes { ForegroundColor = Theme.LightGray }),
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
                    NSLayoutConstraint.Create(commentLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, commentView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(commentLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, commentView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(commentLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, commentView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(commentTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, commentLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(commentTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, commentView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(commentTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, commentView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(commentTextField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, commentView, NSLayoutAttribute.Bottom, 1f, -4f)
                });

                AddArrangedSubview(commentView);

                attachmentNameView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                attachmentNameView.Layer.CornerRadius = CorderRadius;
                attachmentNameView.Layer.MasksToBounds = true;
                attachmentNameView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                attachmentNameLabel = new UILabel
                {
                    Text = "Attachment",
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false
                };

                attachmentNameTextField = new UITextField
                {
                    AttributedPlaceholder = new NSAttributedString("Type...", new UIStringAttributes { ForegroundColor = Theme.LightGray }),
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
                    NSLayoutConstraint.Create(attachmentNameLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, attachmentNameView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(attachmentNameLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, attachmentNameView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(attachmentNameLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, attachmentNameView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(attachmentNameTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, attachmentNameLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(attachmentNameTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, attachmentNameView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(attachmentNameTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, attachmentNameView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(attachmentNameTextField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, attachmentNameView, NSLayoutAttribute.Bottom, 1f, -4f)
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
                {
                    AnimateNotify(AnimationLength, () =>
                    {
                        commentView.Hidden = true;
                        attachmentNameView.Hidden = true;
                    }, ch =>
                    {
                        referenceTextField.UserInteractionEnabled = true;
                        referenceTextField.BecomeFirstResponder();
                    });
                }

                if (recognizer.View == commentView)
                {
                    AnimateNotify(AnimationLength, () =>
                    {
                        referenceView.Hidden = true;
                        attachmentNameView.Hidden = true;
                    }, ch =>
                    {
                        commentTextField.UserInteractionEnabled = true;
                        commentTextField.BecomeFirstResponder();
                    });
                }

                if (recognizer.View == attachmentNameView)
                {
                    AnimateNotify(AnimationLength, () =>
                    {
                        referenceView.Hidden = true;
                        commentView.Hidden = true;
                    }, ch =>
                    {
                        attachmentNameTextField.UserInteractionEnabled = true;
                        attachmentNameTextField.BecomeFirstResponder();
                    });
                }

                UpdateRow();
            }

            [Export("textFieldDidChange:")]
            void TextFieldDidChange(UITextField textField)
            {
                if (textField == referenceTextField)
                    Criteria.Reference = textField.Text;
                if (textField == commentTextField)
                    Criteria.Comment = textField.Text;
                if (textField == attachmentNameTextField)
                    Criteria.AttachmentName = textField.Text;
            }

            [Export("textFieldShouldReturn:")]
            bool TextFieldShouldReturn(UITextField textField)
            {
                if (textField == referenceTextField)
                {
                    AnimateNotify(AnimationLength, () =>
                    {
                        commentView.Hidden = false;
                        attachmentNameView.Hidden = false;
                    }, ch =>
                    {
                        referenceTextField.ResignFirstResponder();
                        referenceTextField.UserInteractionEnabled = false;
                    });
                }

                if (textField == commentTextField)
                {
                    AnimateNotify(AnimationLength, () =>
                    {
                        referenceView.Hidden = false;
                        attachmentNameView.Hidden = false;
                    }, ch =>
                    {
                        commentTextField.ResignFirstResponder();
                        commentTextField.UserInteractionEnabled = false;
                    });
                }

                if (textField == attachmentNameTextField)
                {
                    AnimateNotify(AnimationLength, () =>
                    {
                        referenceView.Hidden = false;
                        commentView.Hidden = false;
                    }, ch =>
                    {
                        attachmentNameTextField.ResignFirstResponder();
                        attachmentNameTextField.UserInteractionEnabled = false;
                    });
                }

                return true;
            }
        }

        class AttachmentsUnreadSearchView : AbstractSearchView
        {

            readonly UILabel attachmentsView;
            readonly UILabel unreadView;

            public AttachmentsUnreadSearchView()
            {
                attachmentsView = new UILabel
                {
                    Text = "WITH ATTACHMENTS",
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                attachmentsView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                attachmentsView.Layer.CornerRadius = CorderRadius;
                attachmentsView.Layer.MasksToBounds = true;
                AddArrangedSubview(attachmentsView);

                unreadView = new UILabel
                {
                    Text = "UNREAD EMAILS",
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                unreadView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                unreadView.Layer.CornerRadius = CorderRadius;
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

                UpdateRow();
            }
        }

        class HandledSearchView : AbstractSearchView
        {

            readonly UILabel allView;
            readonly UILabel handledView;
            readonly UILabel unhadledView;

            public HandledSearchView()
            {
                allView = new UILabel
                {
                    Text = "ALL",
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                allView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                allView.Layer.CornerRadius = CorderRadius;
                allView.Layer.MasksToBounds = true;
                AddArrangedSubview(allView);

                handledView = new UILabel
                {
                    Text = "HANDLED",
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                handledView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                handledView.Layer.CornerRadius = CorderRadius;
                handledView.Layer.MasksToBounds = true;
                AddArrangedSubview(handledView);

                unhadledView = new UILabel
                {
                    Text = "UNHANDLED",
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                unhadledView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                unhadledView.Layer.CornerRadius = CorderRadius;
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

                UpdateRow();
            }
        }
    }
}
