using UIKit;
using Foundation;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Mark5.Mobile.IOS.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using static Mark5.Mobile.IOS.Model.DateTimeChangeEvent;
using Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells;
using Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells.AddEditAppointmentTableViewCell;
using Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews.RecurrenceView;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class EditAppointmentViewController : AbstractAddEditAppointmentViewController
    {
        public EditAppointmentViewController(int appointmentId, int calendarId)
            : base(appointmentId, calendarId)
        {
            Title = Localization.GetString("edit_appointment");
            CreationModeFlag = ContactCreationModeFlag.Edit;
        }
    }

    public class AddAppointmentViewController : AbstractAddEditAppointmentViewController
    {
        public AddAppointmentViewController()
        {
            Title = Localization.GetString("create_appointment");
            CreationModeFlag = ContactCreationModeFlag.New;
        }
    }

    public abstract class AbstractAddEditAppointmentViewController : AbstractTableViewController, IAddEditAppointmentView
    {
        readonly int appointmentId;
        readonly int calendarId;
        readonly AddEditAppointmentPresenter presenter;
        readonly List<CalendarViewModel> calendars;

        UIBarButtonItem saveButtonItem;
        AddEditAppointmentViewModel viewModel;
        Action progressDialogDismissal;

        public ContactCreationModeFlag CreationModeFlag;

        public AbstractAddEditAppointmentViewController(int appointmentId = -1, int calendarId = -1)
            : base(UITableViewStyle.Grouped)
        {
            this.appointmentId = appointmentId;
            this.calendarId = calendarId;
            presenter = new AddEditAppointmentPresenter();
            presenter.AttachView(this);
            calendars = new List<CalendarViewModel>();
        }

        public override void LoadView()
        {
            base.LoadView();
            InitView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            InitNavigationBar();

            presenter.LoadCalendarsList();
        }

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            InitializeHandlers();
            await RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            DeinitializeHandlers();
        }

        protected override void Recycle()
        {
            saveButtonItem = null;
        }

        private void InitView()
        {
            TableView.Source = new DataSource(this, TableView);
            TableView.TableFooterView = new UIView();
            TableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.OnDrag;
            TableView.Editing = false;
            TableView.AllowsSelectionDuringEditing = true;
            TableView.CellLayoutMarginsFollowReadableWidth = true;
            TableView.Alpha = 0;
        }

        async Task RefreshData()
        {
            if (viewModel == null)
            {
                if (CreationModeFlag == ContactCreationModeFlag.New)
                    await presenter.LoadEmptyAppointment();

                if (CreationModeFlag == ContactCreationModeFlag.Edit)
                    await presenter.LoadAppointment(calendarId, appointmentId);
            }
            else
                RefreshTable();
        }

        private void InitNavigationBar()
        {
            saveButtonItem = new UIBarButtonItem
            {
                Title = Localization.GetString("save")
            };

            NavigationItem.SetRightBarButtonItems(new[] { saveButtonItem }, false);
        }

        private void InitializeHandlers()
        {
            if (saveButtonItem != null)
                saveButtonItem.Clicked += SaveButtonItem_Clicked;
        }

        private void DeinitializeHandlers()
        {
            if (saveButtonItem != null)
                saveButtonItem.Clicked -= SaveButtonItem_Clicked;
        }

        private async void SaveButtonItem_Clicked(object sender, EventArgs e)
        {
            var isValid = ((DataSource)TableView.Source).IsFormCorrect();

            if (isValid)
                await presenter.AddOrEditAppointment(viewModel);
            else
                await Dialogs.ShowConfirmAlertAsync(this, "Cannot Save Event", "The start date must be before the end date");
        }

        void RefreshTable()
        {
            ((DataSource)TableView.Source).Refresh(viewModel, calendars);
        }

        #region IAddEditAppointmentView implementation

        public void ShowAppointment(AddEditAppointmentViewModel viewModel)
        {
            this.viewModel = viewModel;
            RefreshTable();
            UIView.Animate(0.1, () => { TableView.Alpha = 1; });
        }

        public async Task ShowLoadError(Exception ex)
        {
            await Dialogs.ShowErrorAlertAsync(this, ex);
        }

        public void ShowLoading()
        {
            progressDialogDismissal = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_appointments___"));
        }

        public void StopLoading()
        {
            progressDialogDismissal?.Invoke();
        }

        public void UpdateCalendarsList(List<CalendarViewModel> calendars)
        {
            this.calendars.AddRange(calendars);
        }

        public void CloseView()
        {
            NavigationController.PopViewController(true);
        }

        public async Task ShowEditingError(Exception ex)
        {
            await Dialogs.ShowErrorAlertAsync(this, ex);
        }

        public void ShowEditingLoading()
        {
            var dialogText = Localization.GetString(CreationModeFlag == ContactCreationModeFlag.New ? "creating_appointment___" : "editing_appointment___");
            progressDialogDismissal = Dialogs.ShowInfiniteProgressDialog(dialogText);
        }

        public void StopEditingLoading()
        {
            progressDialogDismissal?.Invoke();
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable, IUIGestureRecognizerDelegate
        {
            public WeakReference<AbstractAddEditAppointmentViewController> viewControllerWeakReference;
            public WeakReference<UITableView> tableViewWeakReference;

            public event EventHandler ViewIsActivated = delegate { };
            public event EventHandler ResponsibleUserRowClicked = delegate { };
            public event EventHandler ParentRowClicked = delegate { };
            public event EventHandler ParentRemoved = delegate { };

            SectionCollection sections = new SectionCollection();

            Dictionary<NSIndexPath, nfloat> cellHeights = new Dictionary<NSIndexPath, nfloat>();

            public DataSource(AbstractAddEditAppointmentViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
                InitializeSections();
            }

            void InitializeSections()
            {
                var sectionsToInsert = new List<AbstractSection> {
                    new GeneralSection(this),
                    new DateTimeSection(this),
                    new DetailSection(this),
                    new MessageSection(this)
                };

                foreach (var section in sectionsToInsert)
                    sections.Add(section);
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var row = RowAtIndexPath(indexPath);
                if (!(tableView.DequeueReusableCell(row.Key) is AddEditTableViewCell cell))
                {
                    cell = row.CreateCell();
                    var gr = new UITapGestureRecognizer
                    {
                        CancelsTouchesInView = false,
                        WeakDelegate = this,
                    };

                    cell.AddGestureRecognizer(gr);
                }
                row.BindCell(cell);
                return cell;
            }

            public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
            {
                cellHeights[indexPath] = cell.Frame.Size.Height;
            }

            public override void CellDisplayingEnded(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
            {
                if (indexPath != null && !tableView.IndexPathsForVisibleRows.Contains(indexPath))
                {
                    var row = RowAtIndexPath(indexPath);
                    row?.UnbindCell();
                }
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return sections[(int)section].Rows.Count;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return sections.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                if (cell.SelectionStyle == UITableViewCellSelectionStyle.None)
                    return;

                var row = RowAtIndexPath(indexPath);
                row.OnClicked(indexPath);

                if (tableView?.IndexPathForSelectedRow != null)
                    tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);
            }

            public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
            {
                if (cellHeights.ContainsKey(indexPath))
                {
                    return cellHeights[indexPath];
                }

                return 60.0f;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath) => true;

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return RowAtIndexPath(indexPath).EditingStyle;
            }

            public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
            {
                var row = RowAtIndexPath(indexPath);
                row.OnCommit(indexPath);
            }

            public void Refresh(AddEditAppointmentViewModel viewModel, List<CalendarViewModel> calendars)
            {
                foreach (var section in sections)
                {
                    section.ViewModel = viewModel;
                    section.Calendars = calendars;
                    section.InitializeRows();
                }

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromNSRange(new NSRange(0, sections.Count)), UITableViewRowAnimation.Automatic);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                tableViewWeakReference = null;
                viewControllerWeakReference = null;

                sections = null;
            }

            public bool IsFormCorrect()
            {
                var valid = true;
                foreach (var section in sections)
                {
                    valid &= section.IsSectionValid();
                }
                return valid;
            }

            AbstractRow RowAtIndexPath(NSIndexPath indexPath)
            {
                return sections[indexPath.Section].Rows.ElementAtOrDefault(indexPath.Row);
            }

            [Export("gestureRecognizer:shouldReceiveTouch:")]
            public bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
            {
                return true;
            }

            [Export("gestureRecognizerShouldBegin:")]
            public bool ShouldBegin(UIGestureRecognizer recognizer)
            {
                ViewIsActivated(recognizer.View, EventArgs.Empty);
                return true;
            }

            public void RemoveParentContact()
            {
                ParentRemoved(this, EventArgs.Empty);
            }

            public void Reset()
            {
                sections.Clear();

                tableViewWeakReference.Unwrap()?.ReloadData();
            }

            #region Custom section definitions
            class SectionCollection : List<AbstractSection> { }

            abstract class AbstractSection
            {
                public DataSource DataSource;
                public UIViewController viewController { get => DataSource.viewControllerWeakReference.Unwrap(); }
                public UITableView TableView { get => DataSource.tableViewWeakReference.Unwrap(); }
                public List<CalendarViewModel> Calendars;
                public AddEditAppointmentViewModel ViewModel;
                public RowCollection Rows = new RowCollection();

                abstract public void InitializeRows();

                public bool IsSectionValid()
                {
                    var valid = true;
                    foreach (var row in Rows)
                    {
                        var isRowValid = row.IsRowValid();

                        valid &= isRowValid;
                    }
                    return valid;
                }

                protected AbstractSection(DataSource dataSource)
                {
                    DataSource = dataSource;
                }
            }

            class GeneralSection : AbstractSection
            {
                public GeneralSection(DataSource dataSource)
                    : base(dataSource)
                {
                }

                public override void InitializeRows()
                {
                    Rows = new RowCollection
                    {
                        new NameRow(this),
                        new LocationRow(this)
                    };
                }
            }

            class DateTimeSection : AbstractSection
            {
                WeakReference<StartDateRow> startDateRow;
                WeakReference<EndDateRow> endDateRow;

                public DateTimeSection(DataSource dataSource)
                    : base(dataSource)
                {
                }

                public override void InitializeRows()
                {
                    startDateRow = new StartDateRow(this, DateChanged).Wrap();
                    endDateRow = new EndDateRow(this, DateChanged).Wrap();

                    Rows = new RowCollection
                    {
                        new AllDayToggleRow(this, DateChanged),
                        startDateRow.Unwrap(),
                        endDateRow.Unwrap(),
                        new ReoccurrenceRow(this),
                        new ReminderRow(this)
                    };
                }

                public void DateChanged(DateTimeChangeEvent args)
                {
                    switch (args.rowType)
                    {
                        case DateRowType.AllDay:
                            ViewModel.AllDay = args.allDay;
                            break;
                        case DateRowType.Ends:
                            ViewModel.End = args.selectedDate;
                            break;
                        case DateRowType.Starts:
                            var previousStartDate = ViewModel.Start;
                            ViewModel.Start = args.selectedDate;
                            if (ViewModel.End < ViewModel.Start)
                            {
                                var difference = (ViewModel.End - previousStartDate).TotalMinutes;
                                ViewModel.End = ViewModel.Start.AddMinutes(difference);
                            }
                            break;
                    }

                    startDateRow.Unwrap()?.RefreshRow();
                    endDateRow.Unwrap()?.RefreshRow();
                }
            }

            class DetailSection : AbstractSection
            {
                public DetailSection(DataSource dataSource)
                   : base(dataSource)
                {
                }

                public override void InitializeRows()
                {
                    Rows = new RowCollection
                    {
                        new CalendarRow(this),
                        new ParticipantsRow(this)
                    };
                }
            }

            class MessageSection : AbstractSection
            {
                public MessageSection(DataSource dataSource)
                    : base(dataSource)
                {
                }

                public override void InitializeRows()
                {
                    Rows = new RowCollection
                    {
                        new DescriptionRow(this),
                    };
                }
            }

            class RowCollection : List<AbstractRow> { }
            #endregion

            #region Custome row definitions
            abstract class AbstractRow
            {
                protected AddEditTableViewCell Cell;
                protected AbstractSection Section;

                public DataSource DataSource { get => Section.DataSource; }
                public UITableView TableView { get => Section.DataSource.tableViewWeakReference.Unwrap(); }
                public AbstractAddEditAppointmentViewController ViewController { get => Section.DataSource.viewControllerWeakReference.Unwrap(); }
                public AddEditAppointmentViewModel ViewModel { get => Section.ViewModel; }

                public virtual UITableViewCellEditingStyle EditingStyle => UITableViewCellEditingStyle.None;

                public abstract string Key { get; }

                public virtual bool IsRowValid() { return true; }

                bool error;

                protected bool ErrorState { get => error; }

                protected AbstractRow(AbstractSection section)
                {
                    Section = section;
                }

                public abstract AddEditTableViewCell CreateCell();

                public void BindCell(AddEditTableViewCell cell)
                {
                    Cell = cell;
                    Initialize();
                    RefreshRow();
                    SetErrorState(error, false);
                }

                public void UnbindCell()
                {
                    Cell = null;
                }

                public void ReloadRow()
                {
                    if (Cell == null)
                        return;

                    var indexPath = TableView?.IndexPathForCell(Cell);
                    if (indexPath != null)
                        TableView?.ReloadRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                public void SetErrorState(bool errorState, bool animate = true)
                {
                    error = errorState;
                    Cell?.SetErrorState(errorState, animate);
                }

                protected abstract void Initialize();
                public abstract void RefreshRow();

                public virtual void OnClicked(NSIndexPath indexPath) { }
                public virtual void OnCommit(NSIndexPath indexPath) { }
            }

            abstract class TextFieldRow : AbstractRow
            {
                readonly string placeholder;

                readonly UITextAutocapitalizationType autocapitalizationType;
                readonly UITextAutocorrectionType autocorrectionType;

                protected TextFieldRow(AbstractSection section, string placeholder,
                                        UITextAutocapitalizationType autocapitalizationType,
                                        UITextAutocorrectionType autocorrectionType)
                    : base(section)
                {
                    this.placeholder = placeholder;
                    this.autocorrectionType = autocorrectionType;
                    this.autocapitalizationType = autocapitalizationType;
                }

                public override string Key => "TextFieldRow";

                public override AddEditTableViewCell CreateCell() => new TextFieldTableViewCell();

                protected override void Initialize()
                {
                    var tfc = (TextFieldTableViewCell)Cell;
                    tfc.Reset();
                    tfc.SetAutocorrectionType(autocorrectionType);
                    tfc.SetAutocapitalizationType(autocapitalizationType);
                    tfc.SetPlaceholder(placeholder);
                    tfc.ContentEdited = ContentEdited;
                }

                protected abstract void ContentEdited(string e);
            }

            class DescriptionRow : AbstractRow
            {
                public override string Key => "TitledTextView";

                public DescriptionRow(AbstractSection section) : base(section)
                {
                }

                public override AddEditTableViewCell CreateCell() => new TitledTextViewTableViewCell();

                protected override void Initialize()
                {
                    var tfc = (TitledTextViewTableViewCell)Cell;
                    tfc.Reset();
                    tfc.SetAutocorrectionType(UITextAutocorrectionType.Default);
                    tfc.SetAutocapitalizationType(UITextAutocapitalizationType.None);
                    tfc.SetMultiline(true);
                    tfc.ContentEditedAction = ContentEdited;
                    tfc.NumbersOfLineChangedAction = NumberOfLinesChanged;
                    tfc.SetTitle(Localization.GetString("message"));
                }

                void ContentEdited(string e)
                {
                    ViewModel.Description = e;
                }

                void NumberOfLinesChanged()
                {
                    UIView.AnimationsEnabled = false;
                    TableView?.BeginUpdates();
                    TableView?.EndUpdates();
                    UIView.AnimationsEnabled = true;
                }

                public override void RefreshRow()
                {
                    if (ViewModel != null && ViewModel.Description != null && !ViewModel.Description.Equals(string.Empty))
                        ((TitledTextViewTableViewCell)Cell).SetContent(ViewModel.Description);
                }
            }

            class CalendarRow : AbstractRow
            {
                public override string Key => "CalendarRow";

                public CalendarRow(AbstractSection section)
                   : base(section)
                {
                }

                public override AddEditTableViewCell CreateCell() => new CalendarDisclosureTableViewCell();

                public override void RefreshRow()
                {
                    if (ViewModel != null && ViewModel.Calendar != null)
                    {
                        ((CalendarDisclosureTableViewCell)Cell).SetLabel(ViewModel.Calendar.Name);
                        ((CalendarDisclosureTableViewCell)Cell).SetCalendarColor(ViewModel.Calendar.HexColor);
                    }
                    else
                        ((CalendarDisclosureTableViewCell)Cell).SetLabel(Localization.GetString("none"));
                }

                protected override void Initialize()
                {
                    ((CalendarDisclosureTableViewCell)Cell).SetTitle(Localization.GetString("calendar"));
                }

                public async override void OnClicked(NSIndexPath indexPath)
                {
                    var vc = AddEditAppointmentCalendarListViewController.Create(Section.Calendars, ViewModel.Calendar);
                    ViewController?.NavigationController?.PushViewController(vc, true);

                    CalendarViewModel selectedCalendar = await vc.Result;
                    if (selectedCalendar != null)
                        ViewModel.Calendar = selectedCalendar;
                }
            }

            class NameRow : TextFieldRow
            {
                public NameRow(AbstractSection section)
                    : base(section, Localization.GetString("name"), UITextAutocapitalizationType.None, UITextAutocorrectionType.No)
                {
                }

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(ViewModel?.Subject);

                protected override void ContentEdited(string e)
                {
                    ViewModel.Subject = e;
                    SetErrorState(false);
                }
            }

            class LocationRow : TextFieldRow
            {
                public override string Key => "LocationRow";

                public LocationRow(AbstractSection section)
                    : base(section, Localization.GetString("location"), UITextAutocapitalizationType.None, UITextAutocorrectionType.No)
                {
                }

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(ViewModel?.Location);

                protected override void ContentEdited(string e)
                {
                    ViewModel.Location = e;
                    SetErrorState(false);
                }
            }

            class AllDayToggleRow : AbstractRow
            {
                Action<DateTimeChangeEvent> dateChangedHandler;
                public override string Key => "AllDayToggleRow";

                public AllDayToggleRow(AbstractSection section, Action<DateTimeChangeEvent> dateChangedHandler)
                    : base(section)
                {
                    this.dateChangedHandler = dateChangedHandler;
                }

                public override AddEditTableViewCell CreateCell() => new AllDayToggleTableViewCell(dateChangedHandler);

                public override void RefreshRow()
                {
                    if (ViewModel != null)
                        ((AllDayToggleTableViewCell)Cell).SetToggleState(ViewModel.AllDay);
                }

                protected override void Initialize()
                {
                }
            }

            class StartDateRow : DateSelectionRow
            {
                public override string Key => "StartDateRow";

                public StartDateRow(AbstractSection section, Action<DateTimeChangeEvent> dateChangedHandler)
                    : base(section, dateChangedHandler, DateRowType.Starts)
                {
                }

                protected override void Initialize()
                {
                    SetLabel(Localization.GetString("starts"));
                }
            }

            class EndDateRow : DateSelectionRow
            {
                public override string Key => "EndDateRow";

                public EndDateRow(AbstractSection section, Action<DateTimeChangeEvent> dateChangedHandler)
                    : base(section, dateChangedHandler, DateRowType.Ends)
                {
                }

                protected override void Initialize()
                {
                    SetLabel(Localization.GetString("ends"));
                }
            }

            class DateSelectionRow : AbstractRow
            {
                readonly Action<DateTimeChangeEvent> dateChangedHandler = delegate { };
                readonly DateRowType rowType;

                public override string Key => "DateSelectionRow";

                public DateSelectionRow(AbstractSection section, Action<DateTimeChangeEvent> dateChangedHandler, DateRowType rowType) : base(section)
                {
                    this.dateChangedHandler = dateChangedHandler;
                    this.rowType = rowType;
                }

                protected override void Initialize()
                {
                }

                public override AddEditTableViewCell CreateCell() => new DateSelectioTableViewCell(dateChangedHandler, rowType);

                public override void RefreshRow()
                {
                    if (ViewModel != null)
                    {
                        switch (rowType)
                        {
                            case DateRowType.Ends:
                                if (ViewModel.End < ViewModel.Start)
                                {
                                    if (ViewModel.AllDay)
                                        ((DateSelectioTableViewCell)Cell).SetInvalidDate(ViewModel.End);
                                    else
                                        ((DateSelectioTableViewCell)Cell).SetInvalidDateAndTime(ViewModel.End);

                                    break;
                                }
                                if (ViewModel.AllDay)
                                    ((DateSelectioTableViewCell)Cell).SetDateOnly(ViewModel.End);
                                else
                                    ((DateSelectioTableViewCell)Cell).SetDateAndTime(ViewModel.End);
                                break;
                            case DateRowType.Starts:
                                if (ViewModel.AllDay)
                                    ((DateSelectioTableViewCell)Cell).SetDateOnly(ViewModel.Start);
                                else
                                    ((DateSelectioTableViewCell)Cell).SetDateAndTime(ViewModel.Start);
                                break;
                        }
                    }
                }

                public override void OnClicked(NSIndexPath indexPath)
                {
                    DateSelectioTableViewCell cell = (DateSelectioTableViewCell)Cell;
                    if (cell != null)
                        cell.DateTextField.BecomeFirstResponder();
                }

                public void SetLabel(string title)
                {
                    ((DateSelectioTableViewCell)Cell).Label.Text = title;
                }

                public override bool IsRowValid()
                {
                    if (rowType == DateRowType.Ends && ViewModel.End < ViewModel.Start)
                        return false;
                    return true;
                }
            }

            class ReoccurrenceRow : AbstractRow
            {
                public override string Key => "ReoccurrenceRow";

                public ReoccurrenceRow(AbstractSection section) : base(section)
                {
                }

                public override AddEditTableViewCell CreateCell() => new AppointmentDisclosureTableViewCell();

                public override void RefreshRow()
                {
                    if (ViewModel != null && ViewModel.RecurrenceInfo != null)
                        ((AppointmentDisclosureTableViewCell)Cell).SetLabel(ViewModel.RecurrenceInfo.ToFriendlyString());
                    else
                        ((AppointmentDisclosureTableViewCell)Cell).SetLabel(Localization.GetString("never"));
                }

                protected override void Initialize()
                {
                    ((AppointmentDisclosureTableViewCell)Cell).SetTitle(Localization.GetString("repeats"));
                }

                public async override void OnClicked(NSIndexPath indexPath)
                {
                    var choices = new List<string> { "Never", "Custom" }.ToArray();
                    var title = "Repeats";
                    var result = await Dialogs.ShowListActionSheetWithTitleAsync(ViewController, choices, Cell, title);

                    if (result < 0)
                        return;
                    if (result == 0)
                        ViewModel.RecurrenceInfo = null;
                    else if (result == 1)
                    {
                        var recInfo = ViewModel.RecurrenceInfo ?? AddEditAppointmentViewModel.GetEmptyRecurrenceInfo();
                        var vc = RecurrenceViewController.Create(recInfo);
                        ViewController?.NavigationController?.PushViewController(vc, true);
                        var newRecInfo = await vc.Result;

                        if (newRecInfo != null)
                            ViewModel.RecurrenceInfo = newRecInfo;
                    }

                    RefreshRow();
                }
            }

            class ParticipantsRow : AbstractRow
            {
                public override string Key => "ParticipantsRow";

                public ParticipantsRow(AbstractSection section) : base(section)
                {
                }

                public override AddEditTableViewCell CreateCell() => new AppointmentDisclosureTableViewCell();

                public override void RefreshRow()
                {
                    if (ViewModel != null && ViewModel.Participants != null)
                    {
                        if (ViewModel.Participants.Count == 0)
                            ((AppointmentDisclosureTableViewCell)Cell).SetLabel("None");
                        else
                            ((AppointmentDisclosureTableViewCell)Cell).SetLabel($"{ViewModel.Participants.Count}");
                    }
                    else
                        ((AppointmentDisclosureTableViewCell)Cell).SetLabel("None");
                }

                protected override void Initialize()
                {
                    ((AppointmentDisclosureTableViewCell)Cell).SetTitle("Participants");
                }

                public override void OnClicked(NSIndexPath indexPath)
                {
                    ViewController?.NavigationController?.PushViewController(new AddEditAppointmentParticipantsViewController(ViewModel), true);
                }
            }

            class ReminderRow : AbstractRow
            {
                public override string Key => new NSString("ReminderRow");

                public ReminderRow(AbstractSection section) : base(section)
                {
                }

                public override AddEditTableViewCell CreateCell() => new ReminderDisclosureTableViewCell();

                public override void RefreshRow()
                {
                    if (ViewModel != null && ViewModel.ReminderTimeBeforeStart > -1)
                    {
                        ReminderInfo reminder = ReminderInfo.ConvertFromSeconds((int)ViewModel.ReminderTimeBeforeStart);
                        ((ReminderDisclosureTableViewCell)Cell).SetLabel(reminder.Title);
                        ((ReminderDisclosureTableViewCell)Cell).SetPickerSelection(reminder);
                    }
                    else
                    {
                        ((ReminderDisclosureTableViewCell)Cell).SetLabel(Localization.GetString("none"));
                    }
                }

                protected override void Initialize()
                {
                    ReminderDisclosureTableViewCell cell = (ReminderDisclosureTableViewCell)Cell;
                    cell.ReminderSelected = ReminderSelected;
                    cell.SetTitle(Localization.GetString("reminder"));
                }

                void ReminderSelected(ReminderInfo reminder)
                {
                    ViewModel.ReminderTimeBeforeStart = reminder.Seconds;
                }

                public override void OnClicked(NSIndexPath indexPath)
                {
                    ((ReminderDisclosureTableViewCell)Cell).ShowPicker();
                }
            }

            #endregion
        }
    }
}