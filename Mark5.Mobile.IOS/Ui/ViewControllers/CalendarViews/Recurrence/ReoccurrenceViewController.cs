using System;
using System.Collections.Generic;
using CoreGraphics;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews.Subviews
{
    public class RecurrenceViewController : AbstractViewController
    {
        PatternView patternView;
        RangeView rangeView;

        EditAppointmentViewModel ap = new EditAppointmentViewModel();


        public override void LoadView()
        {
            base.LoadView();

            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            patternView.SetViewModel(ap);
            patternView.Refresh();
        }

        void InitializeView()
        {
            ap.RecurrenceInfo = new RecurrenceInfo();
            ap.RecurrenceInfo.Type = RecurrenceType.Yearly;

            NavigationItem.Title = "Custom recurrence"; //TODO remove support for iOS 10
            View.BackgroundColor = UIColor.GroupTableViewBackgroundColor;

            UIScrollView scrollView = new UIScrollView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.White,
                ShowsVerticalScrollIndicator = false,
                ShowsHorizontalScrollIndicator = false,
            };

            View.AddSubview(scrollView);

            View.AddConstraints(new[]
            {
                scrollView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                scrollView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                scrollView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                scrollView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });

            patternView = new PatternView();
            patternView.BackgroundColor = UIColor.Blue;
            rangeView = new RangeView();

            scrollView.AddSubview(patternView);
            //scrollView.AddSubview(rangeView);

            var paddingValue = 20f;

            scrollView.AddConstraints(new[]
            {
                    patternView.LeftAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.LeftAnchor, paddingValue),
                    patternView.TopAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.TopAnchor, paddingValue),
                    patternView.RightAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.RightAnchor, -paddingValue),
                    patternView.WidthAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.WidthAnchor),

                    //rangeView.TopAnchor.ConstraintEqualTo(patternView.BottomAnchor, 10f),
                    //rangeView.LeftAnchor.ConstraintEqualTo(patternView.LeftAnchor),
                    //rangeView.RightAnchor.ConstraintEqualTo(patternView.RightAnchor),
                    //rangeView.BottomAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.BottomAnchor),
            });

            var gestureRecognizer = new UITapGestureRecognizer(() => View.EndEditing(true));
            View.AddGestureRecognizer(gestureRecognizer);
        }

        class PatternView : UIStackView, IEditable
        {
            PatternHeaderView patternHeaderView;

            public PatternView()
            {
                InitializeView();
            }

            public void Refresh()
            {
                patternHeaderView.Refresh();
            }

            public void SetViewModel(EditAppointmentViewModel ca)
            {
                patternHeaderView.SetViewModel(ca);
            }

            void InitializeView()
            {
                Axis = UILayoutConstraintAxis.Vertical;
                Alignment = UIStackViewAlignment.Leading;
                Distribution = UIStackViewDistribution.Fill;
                Spacing = 10f;
                TranslatesAutoresizingMaskIntoConstraints = false;

                patternHeaderView = new PatternHeaderView();

                AddArrangedSubview(patternHeaderView);
                AddArrangedSubview(new SeparatorSubView());
                AddArrangedSubview(new DailyView());
            }

            class PatternHeaderView : UIStackView, IEditable
            {
                EditAppointmentViewModel viewModel;
                PickerTextField typeField;

                List<RecurrenceType> recurrenceTypes = new List<RecurrenceType> { RecurrenceType.Daily, RecurrenceType.Monthly, RecurrenceType.Weekly, RecurrenceType.Yearly };
                List<string> recurrenceStrings = new List<string> { "Daily", "Monthly", "Weekly", "Yearly" }; //TODO can be improved 

                public PatternHeaderView()
                {
                    Axis = UILayoutConstraintAxis.Horizontal;
                    Alignment = UIStackViewAlignment.Fill;
                    Distribution = UIStackViewDistribution.Fill;
                    Spacing = 20f;
                    TranslatesAutoresizingMaskIntoConstraints = false;

                    var label = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = Theme.DefaultFont,
                        TextColor = Theme.DarkGray,
                        Text = "Repeats",
                    };

                    typeField = new PickerTextField();
                    typeField.InputView = new UIPickerView
                    {
                        Model = new PickerViewModel(recurrenceStrings, UpdateModel)
                    };

                    AddArrangedSubview(label);
                    AddArrangedSubview(typeField);
                }

                public void UpdateModel(int i)
                {
                    viewModel.RecurrenceInfo.Type = recurrenceTypes[i];
                    Refresh();
                }

                public void Refresh()
                {
                    typeField.Text = RecurrenceToString(viewModel.RecurrenceInfo.Type);
                    var index = recurrenceTypes.FindIndex(i => i == viewModel.RecurrenceInfo.Type);
                    (typeField.InputView as UIPickerView).Select(index, 0, false);
                }

                public void SetViewModel(EditAppointmentViewModel ca)
                {
                    viewModel = ca;
                }

                string RecurrenceToString(RecurrenceType t)
                {
                    switch (t)
                    {
                        case RecurrenceType.Daily:
                            return "Daily";
                        case RecurrenceType.Weekly:
                            return "Weekly";
                        case RecurrenceType.Monthly:
                            return "Monthly";
                        case RecurrenceType.Yearly:
                            return "Yearly";
                        default:
                            throw new Exception("ERRROR!");
                    }
                }
            }

            class DailyView : UIStackView, IEditable
            {
                EditAppointmentViewModel viewModel;

                public DailyView()
                {
                    Axis = UILayoutConstraintAxis.Vertical;
                    Alignment = UIStackViewAlignment.Fill;
                    Distribution = UIStackViewDistribution.Fill;
                    Spacing = 20f;
                    TranslatesAutoresizingMaskIntoConstraints = false;

                    var firstLine = new UIView
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        UserInteractionEnabled = true,
                    };

                    var everyLabel = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = Theme.DefaultFont,
                        Text = "Every",
                    };

                    var radioButton1 = new RadioButton();

                    var daysLabel = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = Theme.DefaultFont,
                        Text = "days",
                    };

                    var daysTextField = new UITextField();
                    daysTextField.TranslatesAutoresizingMaskIntoConstraints = false;
                    daysTextField.KeyboardType = UIKeyboardType.NumberPad;
                    daysTextField.Text = "122";

                    //firstLine.AddGestureRecognizer(new UITapGestureRecognizer(radioButton1.SetEnabled));
                    firstLine.BackgroundColor = UIColor.Orange;
                    firstLine.AddSubview(radioButton1);
                    firstLine.AddSubview(everyLabel);
                    firstLine.AddSubview(daysLabel);
                    firstLine.AddSubview(daysTextField);

                    firstLine.AddConstraints(new[]
                    {
                        radioButton1.LeadingAnchor.ConstraintEqualTo(firstLine.LeadingAnchor),
                        radioButton1.CenterYAnchor.ConstraintEqualTo(firstLine.CenterYAnchor),
                        radioButton1.TopAnchor.ConstraintEqualTo(firstLine.TopAnchor, 5f),

                        everyLabel.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        everyLabel.LeadingAnchor.ConstraintEqualTo(radioButton1.TrailingAnchor, 10f),

                        daysTextField.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        daysTextField.LeadingAnchor.ConstraintEqualTo(everyLabel.TrailingAnchor, 5f),

                        daysLabel.CenterYAnchor.ConstraintEqualTo(radioButton1.CenterYAnchor),
                        daysLabel.LeadingAnchor.ConstraintEqualTo(daysTextField.TrailingAnchor, 5f),
                    });

                    AddArrangedSubview(firstLine);
                }

                public void UpdateModel(int i)
                {
                }

                public void Refresh()
                {
                }

                public void SetViewModel(EditAppointmentViewModel ca)
                {
                    viewModel = ca;
                }
            }

        }

        class RangeView : UIStackView, IEditable
        {
            public void Refresh()
            {
                throw new NotImplementedException();
            }

            public void SetViewModel(EditAppointmentViewModel ca)
            {
                throw new NotImplementedException();
            }
        }

        interface IEditable
        {
            void Refresh();
            void SetViewModel(EditAppointmentViewModel vm);
        }

        class PickerTextField : UITextField
        {
            const float insetVal = 5f;

            public PickerTextField(bool editable = false)
            {
                Font = Theme.DefaultFont;
                TranslatesAutoresizingMaskIntoConstraints = false;
                Layer.BorderColor = Theme.Blue.CGColor;
                Layer.BorderWidth = 2f;
                Layer.CornerRadius = 8f;
                TextColor = Theme.Blue;
                if (!editable)
                    TintColor = Theme.Clear;
            }

            public override CGRect TextRect(CGRect forBounds)
            {
                return forBounds.Inset(insetVal, insetVal);
            }

            public override CGRect EditingRect(CGRect forBounds)
            {
                return forBounds.Inset(insetVal, insetVal);
            }
        }

        class PickerViewModel : UIPickerViewModel
        {
            List<string> elements;
            Action<int> selectedDelegate;

            public PickerViewModel(List<string> elements, Action<int> selectedDelegate)
            {
                this.elements = elements;
                this.selectedDelegate = selectedDelegate;
            }

            public override nint GetComponentCount(UIPickerView pickerView) => 1;

            public override nint GetRowsInComponent(UIPickerView pickerView, nint component) => elements.Count;

            public override string GetTitle(UIPickerView pickerView, nint row, nint component)
            {
                return elements[(int)row];
            }

            public override void Selected(UIPickerView pickerView, nint row, nint component)
            {
                selectedDelegate?.Invoke((int)row);
            }
        }

        public class SeparatorSubView : UIView
        {
            static readonly UIColor backgroundColor = Theme.Blue;

            public SeparatorSubView()
            {
                BackgroundColor = backgroundColor;
                HeightAnchor.ConstraintEqualTo(0.5f).Active = true;
                TranslatesAutoresizingMaskIntoConstraints = false;
            }
        }

        class RadioButton : UIView
        {
            UIView innerCircle;

            public RadioButton()
            {
                TranslatesAutoresizingMaskIntoConstraints = false;
                Layer.CornerRadius = 5f;
                Layer.BorderColor = Theme.Blue.CGColor;
                Layer.BorderWidth = 2f;

                innerCircle = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                };

                innerCircle.Layer.CornerRadius = 3f;
                innerCircle.BackgroundColor = Theme.Blue;

                AddSubview(innerCircle);

                AddConstraints(new[]
                    {
                    innerCircle.CenterXAnchor.ConstraintEqualTo(CenterXAnchor),
                    innerCircle.CenterYAnchor.ConstraintEqualTo(CenterYAnchor),
                    innerCircle.WidthAnchor.ConstraintEqualTo(innerCircle.HeightAnchor),
                    innerCircle.WidthAnchor.ConstraintEqualTo(6),
                    WidthAnchor.ConstraintEqualTo(HeightAnchor),
                    WidthAnchor.ConstraintEqualTo(15),
                    });

                innerCircle.Alpha = 0;
            }

            public void SetEnabled()
            {
                innerCircle.Alpha = 1;
            }

            public void SetDisabled()
            {
                innerCircle.Alpha = 0;
            }
        }


    }
}
