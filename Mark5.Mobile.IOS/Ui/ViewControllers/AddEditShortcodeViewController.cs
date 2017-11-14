using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Analytics;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCell;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class AddEditShortcodeViewController : AbstractTableViewController
    {
        public Shortcode Shortcode { get; set; }
        public ShortcodePreview ShortcodePreview { get; set; }
        public ShortcodeCreationModeFlag CreationModeFlag { get; set; }

        UIBarButtonItem saveButtonItem;
        UIBarButtonItem cancelButtonItem;

        UIView activeField;

        bool refreshed;

        DataSource TableViewDataSource => (DataSource)TableView.Source;

        #region UIViewControllerOverrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();

            if (CreationModeFlag == ShortcodeCreationModeFlag.New)
                AnalyticsManager.LogEvent(new AddShortcodeEvent());
            if (CreationModeFlag == ShortcodeCreationModeFlag.Edit)
                AnalyticsManager.LogEvent(new EditShortcodeEvent());
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
            }

            InitializeHandlers();
        }

        public override void ViewDidAppear(bool animated)
        {
            CommonConfig.Logger.Info($"{nameof(AddEditShortcodeViewController)} appeared");
            base.ViewWillAppear(animated);
            RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeInitializeHandlers();

            CommonConfig.Logger.Info($"{nameof(AddEditShortcodeViewController)} will disappear");
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            ((DataSource)TableView.Source)?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            saveButtonItem = null;
            cancelButtonItem = null;
            activeField = null;

            ((DataSource)TableView.Source)?.Reset();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        #endregion

        #region Init methods

        void InitializeNavigationBar()
        {
            cancelButtonItem = new UIBarButtonItem
            {
                Title = Localization.GetString("cancel")
            };
            NavigationItem.SetLeftBarButtonItem(cancelButtonItem, false);

            saveButtonItem = new UIBarButtonItem
            {
                Title = Localization.GetString("save"),
                Enabled = true
            };
            NavigationItem.SetRightBarButtonItem(saveButtonItem, false);

            SetTitle();
        }

        void SetTitle()
        {
            string title = string.Empty;
            if (CreationModeFlag == ShortcodeCreationModeFlag.New)
            {
                title = Localization.GetString("edit_shortcode_create");
            }
            else if (CreationModeFlag == ShortcodeCreationModeFlag.Edit)
            {
                title = Localization.GetString("edit_shortcode_edit");
            }

            NavigationItem.Title = title;
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView);
            TableView.TableFooterView = new UIView();
            TableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.OnDrag;
            TableView.Editing = true;
            TableView.AllowsSelectionDuringEditing = true;
            TableView.CellLayoutMarginsFollowReadableWidth = true;
        }

        void InitializeHandlers()
        {
            if (cancelButtonItem != null)
                cancelButtonItem.Clicked += CancelButton_Clicked;

            if (saveButtonItem != null)
                saveButtonItem.Clicked += SaveButton_Clicked;

            if (TableViewDataSource != null)
                TableViewDataSource.ViewIsActivated += DataSource_ViewIsActivated;
        }

        void DeInitializeHandlers()
        {
            if (cancelButtonItem != null)
                cancelButtonItem.Clicked -= CancelButton_Clicked;

            if (saveButtonItem != null)
                saveButtonItem.Clicked -= SaveButton_Clicked;

            if (TableViewDataSource != null)
                TableViewDataSource.ViewIsActivated -= DataSource_ViewIsActivated;
        }

        #endregion

        void RefreshData()
        {
            if (refreshed)
                return;

            var ds = (DataSource)TableView.Source;

            if (CreationModeFlag == ShortcodeCreationModeFlag.New)
            {
                Shortcode = new Shortcode();
                ShortcodePreview = new ShortcodePreview();
            }

            ds.Refresh(Shortcode, ShortcodePreview, CreationModeFlag);
            refreshed = true;
        }

        #region Handlers

        void DataSource_ViewIsActivated(object sender, EventArgs e)
        {
            activeField = (UIView)sender;
        }

        void CancelButton_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        async void SaveButton_Clicked(object sender, EventArgs e)
        {
            if (!TableViewDataSource.IsFormCorrect())
                return;

            var contentString = CreationModeFlag == ShortcodeCreationModeFlag.New ? Localization.GetString("adding_shortcode___") : Localization.GetString("editing_shortcode___");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(contentString);

            try
            {
                TableView.EndEditing(true);
                await Managers.ShortcodesManager.CreateOrUpdateShortcodeAsync(Shortcode, ShortcodePreview);

                dismissAction();
                DismissViewController(true, null);
            }
            catch (Exception ex)
            {
                dismissAction();
                await Dialogs.ShowErrorAlertAsync(this, ex);
                CommonConfig.Logger.Error($"Error while sending/editing shortcode [creationMode = {CreationModeFlag}, shortcodeId = {Shortcode?.Id}]", ex);
            }
        }

        async Task<DocumentAddress> ChooseAddress(UITableViewCell cell)
        {
            var strings = new[] { Localization.GetString("add_from_contact"), Localization.GetString("add_empty") };

            var choiceIndex = await Dialogs.ShowListActionSheetAsync(this, strings, TableView, cell);

            if (choiceIndex == 0)
            {
                var vc = new PickerContactsFoldersListViewController();
                PresentViewController(new NavigationController(vc), true, null);

                var pa = await vc.Result;
                DismissViewController(true, null);

                if (pa != null)
                    return new DocumentAddress { Address = pa.Address };

            }
            else if (choiceIndex == 1)
                return new DocumentAddress();

            return null;
        }

        #endregion

        #region Keyboard

        void OnKeyboardDidShowNotification(object sender, UIKeyboardEventArgs e)
        {
            AdjustViewToKeyboard(UI.KeyboardHeightFromNotification(e.Notification), e.Notification, true, true);
        }

        void OnKeyboardWillChangeFrameNotification(object sender, UIKeyboardEventArgs e)
        {
            AdjustViewToKeyboard(UI.KeyboardHeightFromNotification(e.Notification), e.Notification, false, false);
        }

        void OnKeyboardWillHideNotification(object sender, UIKeyboardEventArgs e)
        {
            AdjustViewToKeyboard(0f, e.Notification, false, true);
        }

        void AdjustViewToKeyboard(float keyboardHeight, NSNotification notification, bool adjustContentOffset, bool adjustInsets)
        {
            if (notification == null)
            {
                View.LayoutIfNeeded();
                return;
            }

            if (adjustContentOffset && activeField != null)
            {
                var difference = activeField.Frame.Bottom - TableView.ContentOffset.Y - (View.Frame.Height - keyboardHeight) + 10;

                if (difference > 0)
                {
                    var co = TableView.ContentOffset;
                    co.Y += difference;
                    TableView.SetContentOffset(co, true);
                }
            }

            if (adjustInsets)
            {
                var ci = TableView.ContentInset;
                ci.Bottom = keyboardHeight;
                TableView.ContentInset = ci;

                ci = TableView.ScrollIndicatorInsets;
                ci.Bottom = keyboardHeight;
                TableView.ScrollIndicatorInsets = ci;
            }
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable, IUIGestureRecognizerDelegate
        {
            WeakReference<AddEditShortcodeViewController> viewControllerWeakReference;
            WeakReference<UITableView> tableViewWeakReference;

            public event EventHandler ViewIsActivated = delegate { };

            SectionCollection sections = new SectionCollection();

            Dictionary<NSIndexPath, nfloat> cellHeights = new Dictionary<NSIndexPath, nfloat>();

            public DataSource(AddEditShortcodeViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
                InitializeSections();
            }

            void InitializeSections()
            {
                var sectionsToInsert = new List<AbstractSection> {
                    new GeneralSection(this),
                    new AddressSection(this, DocumentAddressType.To),
                    new AddressSection(this, DocumentAddressType.Cc),
                    new AddressSection(this, DocumentAddressType.Bcc),
                };

                foreach (var section in sectionsToInsert)
                {
                    sections.Add(section);
                }
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var row = RowAtIndexPath(indexPath);
                var cell = tableView.DequeueReusableCell(row.Key) as AddEditTableViewCell;
                if (cell == null)
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

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return true;
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return RowAtIndexPath(indexPath).EditingStyle;
            }

            public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
            {
                var row = RowAtIndexPath(indexPath);
                row.OnCommit(indexPath);
            }

            public void Refresh(Shortcode shortcode, ShortcodePreview shortcodePreview,
                                ShortcodeCreationModeFlag creationMode)
            {
                foreach (var section in sections)
                {
                    section.Shortcode = shortcode;
                    section.ShortcodePreview = shortcodePreview;
                    section.CreationModeFlag = creationMode;

                    section.InitializeRows();
                }

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromNSRange(new NSRange(0, sections.Count)), UITableViewRowAnimation.Automatic);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                tableViewWeakReference = null;

                sections = null;
            }

            public bool IsFormCorrect()
            {
                var valid = true;
                foreach (var section in sections)
                {
                    if (!section.IsSectionValid())
                        return false;
                }
                return valid;
            }

            async Task<DocumentAddress> HeaderCellClicked(UITableViewCell cell)
            {
                var vc = viewControllerWeakReference.Unwrap();
                return vc == null ? null : await vc.ChooseAddress(cell);
            }

            public int IndexForSection(AbstractSection section)
            {
                return sections.FindIndex(s => s == section);
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

            public void Reset()
            {
                sections.Clear();

                tableViewWeakReference.Unwrap()?.ReloadData();
            }

            #region Support classes

            class SectionCollection : List<AbstractSection> { }

            public abstract class AbstractSection
            {
                public DataSource DataSource;

                public UITableView TableView { get => DataSource.tableViewWeakReference.Unwrap(); }
                public Shortcode Shortcode { get; set; }
                public ShortcodePreview ShortcodePreview { get; set; }
                public ShortcodeCreationModeFlag CreationModeFlag { get; set; }

                public RowCollection Rows { get; } = new RowCollection();

                abstract public void InitializeRows();

                public bool IsSectionValid()
                {
                    var valid = true;

                    for (int i = 0; i < Rows.Count; i++)
                    {
                        var row = Rows[i];
                        var isRowValid = row.IsRowValid();
                        row.SetErrorState(!isRowValid);

                        if (valid && !isRowValid)
                        {
                            var sectionIndex = DataSource.IndexForSection(this);
                            var indexPath = NSIndexPath.FromRowSection(i, sectionIndex);
                            TableView?.ScrollToRow(indexPath, UITableViewScrollPosition.Middle, true);
                        }

                        valid &= isRowValid;
                    }
                    return valid;
                }

                protected AbstractSection(DataSource dataSource)
                {
                    DataSource = dataSource;
                }
            }

            abstract class MultiSection : AbstractSection
            {
                protected MultiSection(DataSource dataSource)
                    : base(dataSource)
                {
                }

                public abstract void DeleteRow(NSIndexPath indexPath, AbstractRow row);
            }

            class GeneralSection : AbstractSection
            {
                public GeneralSection(DataSource dataSource)
                    : base(dataSource)
                {
                }

                public override void InitializeRows()
                {
                    Rows.Add(new NameRow(this));
                    Rows.Add(new DescriptionRow(this));
                }
            }

            class AddressSection : MultiSection
            {
                DocumentAddressType addressType;

                public AddressSection(DataSource dataSource, DocumentAddressType type)
                    : base(dataSource)
                {
                    addressType = type;
                }

                public override void InitializeRows()
                {
                    var addresses = Shortcode.Addresses.Where(c => c.AddressType == addressType
                                                              && c.Type == CommunicationAddressType.Email);

                    foreach (var address in addresses)
                        Rows.Add(new AddressRow(this, address));

                    Rows.Add(new AddressHeaderRow(this, addressType));
                }

                public void AddNewRow(NSIndexPath indexPath, DocumentAddress da)
                {
                    da.Type = CommunicationAddressType.Email;
                    da.AddressType = addressType;

                    Shortcode.Addresses.Add(da);
                    Rows.Insert(Rows.Count - 1, new AddressRow(this, da));
                    TableView?.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                public override void DeleteRow(NSIndexPath indexPath, AbstractRow row)
                {
                    var pnRow = row as AddressRow;
                    var ca = pnRow.Content;
                    Shortcode.Addresses.Remove(ca);
                    Rows.Remove(row);
                    TableView?.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                    TableView?.EndEditing(true);
                }

                public async void HeaderCellClicked(UITableViewCell cell, NSIndexPath indexPath)
                {
                    var result = await DataSource.HeaderCellClicked(cell);
                    if (result != null)
                        AddNewRow(indexPath, result);
                }
            }

            public class RowCollection : List<AbstractRow> { }

            #region Abstract rows

            public abstract class AbstractRow
            {
                protected AddEditTableViewCell Cell;
                protected AbstractSection Section;

                public DataSource DataSource { get => Section.DataSource; }
                public UITableView TableView { get => Section.DataSource.tableViewWeakReference.Unwrap(); }
                public Shortcode Shortcode { get => Section.Shortcode; }
                public ShortcodePreview ShortcodePreview { get => Section.ShortcodePreview; }
                public ShortcodeCreationModeFlag CreationMode { get => Section.CreationModeFlag; }

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

                public override string Key => TextFieldTableViewCell.Key;

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

            abstract class TitledTextView : AbstractRow
            {
                readonly string title;
                readonly bool isMultiline;
                readonly UITextAutocapitalizationType autocapitalizationType;
                readonly UITextAutocorrectionType autocorrectionType;

                protected TitledTextView(AbstractSection section, string title, bool isMultiline,
                                       UITextAutocapitalizationType autocapitalizationType,
                                       UITextAutocorrectionType autocorrectionType)
                    : base(section)
                {
                    this.title = title;
                    this.isMultiline = isMultiline;
                    this.autocorrectionType = autocorrectionType;
                    this.autocapitalizationType = autocapitalizationType;
                }

                public override string Key => TitledTextViewTableViewCell.Key;

                public override AddEditTableViewCell CreateCell() => new TitledTextViewTableViewCell();

                protected override void Initialize()
                {
                    var tfc = (TitledTextViewTableViewCell)Cell;
                    tfc.Reset();
                    tfc.SetAutocorrectionType(autocorrectionType);
                    tfc.SetAutocapitalizationType(autocapitalizationType);
                    tfc.SetMultiline(isMultiline);
                    tfc.SetTitle(title);
                    tfc.ContentEditedAction = ContentEdited;
                    tfc.NumbersOfLineChangedAction = NumberOfLinesChanged;
                }

                protected abstract void ContentEdited(string e);

                void NumberOfLinesChanged()
                {
                    if (isMultiline)
                    {
                        //Used to make the cell grow with the content
                        UIView.AnimationsEnabled = false;
                        TableView.BeginUpdates();
                        TableView.EndUpdates();
                        UIView.AnimationsEnabled = true;
                    }
                }
            }

            abstract class MultiHeaderRow : AbstractRow
            {
                protected MultiHeaderRow(MultiSection section)
                    : base(section)
                {
                }

                public override UITableViewCellEditingStyle EditingStyle => UITableViewCellEditingStyle.Insert;

                public override string Key => MultiRowHeaderTableViewCell.Key;

                public override AddEditTableViewCell CreateCell() => new MultiRowHeaderTableViewCell();

                public override void RefreshRow() { }

            }

            abstract class MultiContentRow<T> : AbstractRow where T : class
            {
                public T Content { get; set; }

                protected MultiContentRow(MultiSection section, T content)
                    : base(section)
                {
                    Content = content;
                }

                public override UITableViewCellEditingStyle EditingStyle => UITableViewCellEditingStyle.Delete;

                public override void OnCommit(NSIndexPath indexPath)
                {
                    ((MultiSection)Section).DeleteRow(indexPath, this);
                }
            }

            #endregion

            class NameRow : TextFieldRow
            {
                public NameRow(AbstractSection section)
                    : base(section, Localization.GetString("name"), UITextAutocapitalizationType.Words, UITextAutocorrectionType.No)
                {
                }

                public override bool IsRowValid()
                {
                    return !string.IsNullOrWhiteSpace(ShortcodePreview.Name);
                }

                protected override void ContentEdited(string e)
                {
                    ShortcodePreview.Name = e;
                    SetErrorState(false);
                }

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(ShortcodePreview.Name);
            }

            class DescriptionRow : TitledTextView
            {
                public DescriptionRow(AbstractSection section)
                    : base(section, Localization.GetString("description"), true, UITextAutocapitalizationType.Sentences, UITextAutocorrectionType.Default)
                {
                }

                public override void RefreshRow()
                {
                    ((TitledTextViewTableViewCell)Cell).SetContent(ShortcodePreview.Description);
                }

                protected override void ContentEdited(string e)
                {
                    ShortcodePreview.Description = e;
                }
            }

            class AddressHeaderRow : MultiHeaderRow
            {
                readonly DocumentAddressType addressType;

                public AddressHeaderRow(AddressSection section, DocumentAddressType addressType)
                            : base(section)
                {
                    this.addressType = addressType;
                }

                protected override void Initialize()
                {
                    var cell = (MultiRowHeaderTableViewCell)Cell;
                    string titleString;
                    switch (addressType)
                    {
                        case DocumentAddressType.To:
                            titleString = Localization.GetString("add_to");
                            break;
                        case DocumentAddressType.Cc:
                            titleString = Localization.GetString("add_cc");
                            break;
                        case DocumentAddressType.Bcc:
                            titleString = Localization.GetString("add_bcc");
                            break;
                        default:
                            throw new ArgumentException("The type is not yet supported!");
                    }

                    cell.SetTitle(titleString);
                }

                public override void OnClicked(NSIndexPath indexPath)
                {
                    ((AddressSection)Section).HeaderCellClicked(Cell, indexPath);
                }

                public override void OnCommit(NSIndexPath indexPath)
                {
                    ((AddressSection)Section).HeaderCellClicked(Cell, indexPath);
                }
            }

            class AddressRow : MultiContentRow<DocumentAddress>
            {
                public AddressRow(AddressSection section, DocumentAddress address)
                    : base(section, address)
                {
                }

                public override bool IsRowValid()
                {
                    return Validator.IsEmailValid(Content.Address);
                }

                public override string Key => AddressTableViewCell.Key;

                public override AddEditTableViewCell CreateCell() => new AddressTableViewCell();

                protected override void Initialize()
                {
                    var cell = (AddressTableViewCell)Cell;
                    cell.Reset();
                    cell.AddressChangedAction = Cell_AddressChanged;
                }

                public override void RefreshRow()
                {
                    var cell = (AddressTableViewCell)Cell;
                    cell.BindContent(Content, ErrorState);
                }

                void Cell_AddressChanged()
                {
                    if (Validator.IsEmailValid(Content.Address))
                        SetErrorState(false);
                }
            }
            #endregion

        }

    }
}
