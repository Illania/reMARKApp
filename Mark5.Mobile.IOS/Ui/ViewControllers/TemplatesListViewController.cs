using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class TemplatesListViewController : AbstractViewController
    {
        UIBarButtonItem dismissButtonItem;

        UITableView tableView;
        DataSource dataSource;

        CancellationTokenSource cts; //TODO used?

        bool refreshing;

        public TemplatesListViewController()
        {
            Title = Localization.GetString("templates");
        }

        #region Lifecycle methods

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeCategoriesListView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
        }

        public async override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(TemplatesListViewController)} appeared");

            if (dataSource.Empty)
                await RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeInitializeHandlers();

            cts?.Cancel();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(TemplatesListViewController)} received memory warning!");

            dataSource?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }
        #endregion

        #region Init methods

        void InitializeNavigationBar()
        {
            dismissButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            NavigationItem.SetRightBarButtonItem(dismissButtonItem, false);
        }

        void InitializeCategoriesListView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            tableView = new UITableView();
            tableView.Source = dataSource = new DataSource(this, tableView, Localization.GetString("no_templates"));
            tableView.EstimatedRowHeight = 40f;
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.ClipsToBounds = false;
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(tableView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });
        }

        void InitializeHandlers()
        {
            if (dismissButtonItem != null)
                dismissButtonItem.Clicked += DismissButtonItem_Clicked;
        }

        void DeInitializeHandlers()
        {
            if (dismissButtonItem != null)
                dismissButtonItem.Clicked -= DismissButtonItem_Clicked;
        }

        void DismissButtonItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        #region Refreshing

        async Task RefreshData()
        {
            if (refreshing)
                return;

            CommonConfig.Logger.Info("Refreshing templates list");

            cts?.Cancel();
            cts = new CancellationTokenSource();

            try
            {
                var previews = await Managers.DocumentsManager.GetTemplatePreviewsAsync();

                dataSource.SetItems(previews);
                refreshing = false;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while retrieving template previews", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
                refreshing = false;
            }
        }

        #endregion

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {
            public bool Empty { get { return !templatesInView.SelectMany(v => v).Any(); } }

            public IEnumerable<TemplatePreview> Items { get { return templatesInView.SelectMany(i => i); } }

            List<List<TemplatePreview>> templatesInView = new List<List<TemplatePreview>>(2);

            UITableView tableView;
            string emptyText;
            TemplatesListViewController viewController;

            bool loading = true;

            public DataSource(TemplatesListViewController viewController, UITableView tableView, string emptyText)
            {
                this.tableView = tableView;
                this.emptyText = emptyText;
                this.viewController = viewController;
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

                var tp = templatesInView[indexPath.Section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(TemplatesTableViewCell.Key) as TemplatesTableViewCell ?? TemplatesTableViewCell.Create();
                cell.Initialize(tp);

                return cell;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (loading || Empty)
                    return 1;

                return templatesInView.Count;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return templatesInView[(int) section].Count;
            }

            public override nfloat GetHeightForHeader(UITableView tableView, nint section)
            {
                if (loading || Empty || !templatesInView[(int) section].Any())
                    return 0;

                return UITableView.AutomaticDimension;
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                return section == 0 ? Localization.GetString("private") : Localization.GetString("Public");
            }

            public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
            {
                var v = headerView as UITableViewHeaderFooterView;
                if (v == null)
                    return;

                v.TextLabel.TextColor = Theme.DarkerBlue;
            }

            public void SetItems(List<TemplatePreview> templatePreviews)
            {
                loading = false;

                templatesInView.Clear();

                templatesInView.Add(templatePreviews.Where(t => t.Private).ToList());
                templatesInView.Add(templatePreviews.Where(t => !t.Private).ToList());

                tableView.BeginUpdates();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableView.InsertSections(NSIndexSet.FromIndex(1), UITableViewRowAnimation.Fade);
                tableView.EndUpdates();

            }

            public void Reset()
            {
                loading = true;

                templatesInView.Clear();

                tableView.BeginUpdates();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableView.DeleteSections(NSIndexSet.FromNSRange(new NSRange(1, 1)), UITableViewRowAnimation.Fade);
                tableView.EndUpdates();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                templatesInView = null;
            }
        }

    }
}
