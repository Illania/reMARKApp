using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class PhonebookContactsListViewController : AbstractViewController
    {
        UIBarButtonItem exitEditItem;
        UITableView tableView;

        CancellationTokenSource cts;

        Action<string, string> phonebookContactSelectedAction;

        public PhonebookContactsListViewController(Action<string, string> recentAddressClickedAction)
        {
            this.phonebookContactSelectedAction = recentAddressClickedAction;
        }

        #region UIViewControllerOverrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeNavigationBarTitle();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(PhonebookContactsListViewController)} appeared");

            var ds = (DataSource) tableView.Source;
            if (ds.Empty)
                await RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();

            cts?.Cancel();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(PhonebookContactsListViewController)} received memory warning!");

            var ds = tableView?.Source as DataSource;
            ds?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        #endregion

        #region Initialization

        void InitializeNavigationBar()
        {
            exitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            NavigationItem.SetRightBarButtonItem(exitEditItem, true);
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            tableView = new UITableView();
            tableView.ClipsToBounds = false;
            tableView.Source = new DataSource(this, tableView, Localization.GetString("phonebook_contacts_empty"));
            tableView.AllowsSelectionDuringEditing = false;
            tableView.AllowsMultipleSelectionDuringEditing = true;
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            tableView.EstimatedRowHeight = 44f;
            tableView.RowHeight = UITableView.AutomaticDimension;
            View.AddSubview(tableView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });
        }

        void InitializeNavigationBarTitle()
        {
            UIView.AnimationsEnabled = false;
            NavigationItem.Title = Localization.GetString("phonebook_contacts_title");
            NavigationItem.Prompt = null;
            UIView.AnimationsEnabled = true;
        }

        void InitializeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked += ExitEditItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked -= ExitEditItem_Clicked;
        }

        #endregion

        #region Refreshing

        async Task RefreshData()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            try
            {
                var contacts = CommonConfig.PhonebookUtilities.GetPhonebookContacts();

                if (contacts == null)
                {
                    await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("phonebook_contacts_no_access_title"),
                                                         Localization.GetString("phonebook_contacts_no_access_content"));
                    DismissViewController(true, null);
                }

                var ds = (DataSource) tableView.Source;
                ds.SetItems(contacts.OrderBy(c => c.Name).ToList());
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while retrieving phonebook contacts", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
                NavigationController?.PopViewController(true);
            }
            finally
            {
                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        #endregion

        #region Actions

        public void PhonebookAddressSelected(PrintableSuggestion pb, UITableViewCell cell)
        {
            phonebookContactSelectedAction(pb.Name, pb.Address);
            DismissViewController(true, null);
        }

        #endregion

        #region Event Handlers

        void ExitEditItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {
            public bool Empty { get { return !phonebookContactsInView.SelectMany(v => v).Any(); } }

            PhonebookContactsListViewController viewController;
            UITableView tableView;
            readonly string emptyText;

            bool loading = true;
            List<List<PrintableSuggestion>> phonebookContactsInView = new List<List<PrintableSuggestion>>(25);

            public DataSource(PhonebookContactsListViewController viewController, UITableView tableView, string emptyText)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.emptyText = emptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var ra = phonebookContactsInView[indexPath.Section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(SuggestionsTableViewCell.Key) as SuggestionsTableViewCell ?? SuggestionsTableViewCell.Create();
                cell.Initialize(ra);

                return cell;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (loading || Empty)
                    return 1;

                return phonebookContactsInView.Count;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return phonebookContactsInView[(int) section].Count;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return false;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var ra = phonebookContactsInView[indexPath.Section][indexPath.Row];
                viewController.PhonebookAddressSelected(ra, tableView.CellAt(indexPath));
            }

            public override string[] SectionIndexTitles(UITableView tableView)
            {
                return phonebookContactsInView.Select(i => i.First()?.Name.SafeSubstring(0, 1).ToUpper()).ToArray();
            }

            public void SetItems(List<PrintableSuggestion> phonebookContacts)
            {
                loading = false;

                phonebookContactsInView = phonebookContacts.GroupBy(cp => cp.Name.SafeSubstring(0, 1)).Select(s => s.ToList()).ToList();
                tableView.ReloadData();
            }

            public void Reset()
            {
                loading = true;

                var count = phonebookContactsInView.Count;

                phonebookContactsInView.Clear();

                tableView.BeginUpdates();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);

                tableView.EndUpdates();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                phonebookContactsInView = null;
            }
        }
    }
}
