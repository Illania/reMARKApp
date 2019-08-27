using UIKit;
using Foundation;
using System;
using System.Linq;
using System.Globalization;
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

    public class AbstractAddEditAppointmentViewController : AbstractTableViewController, IAddEditAppointmentView
    {
        readonly int appointmentId;
        readonly int calendarId;
        readonly AddEditAppointmentPresenter presenter;

        UIBarButtonItem saveButtonItem;
        AddEditAppointmentViewModel viewModel;
        Action progressDialogDismissal;

        public ContactCreationModeFlag CreationModeFlag;

        public AbstractAddEditAppointmentViewController()
        {
            presenter = new AddEditAppointmentPresenter();
            presenter.AttachView(this);
        }

        public AbstractAddEditAppointmentViewController(int appointmentId, int calendarId)
        {
            this.appointmentId = appointmentId;
            this.calendarId = calendarId;
            presenter = new AddEditAppointmentPresenter();
            presenter.AttachView(this);
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
            RefreshData();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            InitializeHandlers();

            ((DataSource)TableView.Source).Refresh(viewModel, CreationModeFlag, false);
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
            TableView.Editing = true;
            TableView.AllowsSelectionDuringEditing = true;
            TableView.CellLayoutMarginsFollowReadableWidth = true;
            TableView.Alpha = 0;
        }

        async void RefreshData()
        {
            if (CreationModeFlag == ContactCreationModeFlag.New)
                await presenter.LoadEmptyAppointment();

            if (CreationModeFlag == ContactCreationModeFlag.Edit)
                await presenter.LoadAppointment(calendarId, appointmentId);

            ((DataSource)TableView.Source).Refresh(viewModel, CreationModeFlag, false);
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

        private void SaveButtonItem_Clicked(object sender, EventArgs e)
        {
            //TODO: add model validation before:
            //await presenter.AddOrEditAppointment(viewModel);
        }

        private void InitNavigationBar()
        {
            saveButtonItem = new UIBarButtonItem
            {
                Title = Localization.GetString("save")
            };

            NavigationItem.SetRightBarButtonItems(new[] { saveButtonItem }, false);
        }

        public void CloseView()
        {
            throw new NotImplementedException();
        }

        public Task ShowAddingEditingError(Exception ex)
        {
            throw new NotImplementedException();
        }

        public void ShowAppointment(AddEditAppointmentViewModel viewModel)
        {
            if (this.viewModel == null)
            {
                this.viewModel = viewModel;
                UIView.Animate(0.05, () => { TableView.Alpha = 1; });
                RefreshData();
            }
        }

        public async Task ShowLoadError()
        {
            await Dialogs.ShowErrorAlertAsync(this, new Exception("Unresolved error occured while loading appointment"));
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
            throw new NotImplementedException();
        }

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
                {
                    sections.Add(section);
                }
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

            public override UIView GetViewForHeader(UITableView tableView, nint section)
            {
                UIView headerView = new UIView
                {
                    BackgroundColor = UIColor.GroupTableViewBackgroundColor
                };

                headerView.AddConstraint(headerView.HeightAnchor.ConstraintEqualTo(20f));

                return headerView;
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

            public void Refresh(AddEditAppointmentViewModel viewModel, ContactCreationModeFlag creationMode, bool parentPreselected)
            {
                foreach (var section in sections)
                {
                    section.ViewModel = viewModel;
                    section.CreationMode = creationMode;
                    section.ParentPreselected = parentPreselected;
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
                public AddEditAppointmentViewModel ViewModel;
                public bool ParentPreselected;
                public RowCollection Rows = new RowCollection();
                public ContactCreationModeFlag CreationMode;

                abstract public void InitializeRows();

                public bool IsSectionValid()
                {
                    var valid = true;
                    foreach (var row in Rows)
                    {
                        var isRowValid = row.IsRowValid();
                        row.SetErrorState(!isRowValid);

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
                        new ReocurrenceRow(this),
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
                        new MessageRow(this),
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
                public UIViewController ViewController { get => Section.DataSource.viewControllerWeakReference.Unwrap(); }
                public AddEditAppointmentViewModel ViewModel { get => Section.ViewModel; }
                public ContactCreationModeFlag CreationMode { get => Section.CreationMode; }
                public bool ParentPreselected { get => Section.ParentPreselected; }

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
                    tfc.AdjustLeadingConstraint();
                    tfc.ContentEdited = ContentEdited;
                }

                protected abstract void ContentEdited(string e);
            }

            class MessageRow : AbstractRow
            {
                public override string Key => "TitledTextView";

                public MessageRow(AbstractSection section) : base(section)
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
                    tfc.AdjustLeadingConstraint();
                    tfc.SetPlaceholder(Localization.GetString("description"));
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
                    else
                        ((TitledTextViewTableViewCell)Cell).SetPlaceholder(Localization.GetString("description"));
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
                    Task<CalendarViewModel> result = null;

                    if (ViewModel.Calendar != null)
                    {
                        var vc = AddEditAppointmentCalendarListViewController.Factory(ViewModel.Calendar);
                        ViewController?.NavigationController?.PushViewController(vc, true);
                        result = vc.Result;
                    }
                    else
                    {
                        var vc = AddEditAppointmentCalendarListViewController.Factory();
                        ViewController?.NavigationController?.PushViewController(vc, true);
                        result = vc.Result;
                    }

                    CalendarViewModel selectedCalendar = await result;
                    if (selectedCalendar != null)
                        ViewModel.Calendar = selectedCalendar;
                    else
                        ViewModel.Calendar = null;
                }
            }

            class NameRow : TextFieldRow
            {
                public NameRow(AbstractSection section)
                    : base(section, Localization.GetString("name"), UITextAutocapitalizationType.Words, UITextAutocorrectionType.No)
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
                    : base(section, Localization.GetString("location"), UITextAutocapitalizationType.Words, UITextAutocorrectionType.No)
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
                //EventHandler<DateTimeChangeEvent> dateChangedHandler = delegate { };
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
                    InitializeDate(DateTime.Now);
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
                    if (ViewModel != null)
                        InitializeDate(DateTime.Now.AddHours(1));
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
                    base.OnClicked(indexPath);

                    DateSelectioTableViewCell cell = (DateSelectioTableViewCell)Cell;
                    if (cell != null)
                        cell.DateTextField.BecomeFirstResponder();
                }

                public void SetLabel(string title)
                {
                    ((DateSelectioTableViewCell)Cell).Label.Text = title;
                }

                public void InitializeDate(DateTime dateTime)
                {
                    ((DateSelectioTableViewCell)Cell).DateTextField.Text = $"{ dateTime.Date.ToString("d MMM yyyy", CultureInfo.CurrentCulture) }   { dateTime.Date.ToString("t", CultureInfo.CurrentCulture) }";
                }
            }

            class ReocurrenceRow : AbstractRow
            {
                public override string Key => "ReocurrenceRow";

                public ReocurrenceRow(AbstractSection section) : base(section)
                {
                }

                public override AddEditTableViewCell CreateCell() => new AppointmentDisclosureTableViewCell();

                public override void RefreshRow()
                {
                    if (ViewModel != null && ViewModel.RecurrenceInfo != null)
                        ((AppointmentDisclosureTableViewCell)Cell).SetLabel(Localization.GetString("custom"));
                    else
                        ((AppointmentDisclosureTableViewCell)Cell).SetLabel(Localization.GetString("never"));
                }

                protected override void Initialize()
                {
                    ((AppointmentDisclosureTableViewCell)Cell).SetTitle(Localization.GetString("repeats"));
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
                    ViewController?.NavigationController?.PushViewController(new AddEditParticipantsViewController(ViewModel), true);
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
                    base.OnClicked(indexPath);
                    ((ReminderDisclosureTableViewCell)Cell).Label.BecomeFirstResponder();
                }
            }

            #endregion
        }
    }
}