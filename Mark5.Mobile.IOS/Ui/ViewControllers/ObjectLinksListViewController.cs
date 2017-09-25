using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class ObjectLinksListViewController : AbstractTableViewController
    {
        readonly IBusinessEntity businessEntity;

        UIBarButtonItem doneItem;

        public ObjectLinksListViewController(IBusinessEntity businessEntity)
            : base(UITableViewStyle.Grouped)
        {
            this.businessEntity = businessEntity;
        }

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();

            if (TableView?.IndexPathForSelectedRow != null)
                TableView.DeselectRow(TableView.IndexPathForSelectedRow, true);

            if (TableView?.IndexPathsForSelectedRows?.Length > 0)
                foreach (var selectedIndexPath in TableView?.IndexPathsForSelectedRows)
                    TableView.DeselectRow(selectedIndexPath, true);

            ReachabilityBar.Attach(View, TableView, (float)NavigationController.BottomLayoutGuide.Length);
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(ObjectLinksListViewController)} appeared");

            if (((DataSource)TableView.Source).Empty)
                await RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(ObjectLinksListViewController)} received memory warning!");

            ((DataSource)TableView.Source)?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        public override void Recycle()
        {
            base.Recycle();

            ((DataSource)TableView.Source)?.Reset();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        void InitializeNavigationBar()
        {
            NavigationItem.Title = Localization.GetString("links");

            doneItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            NavigationItem.SetRightBarButtonItem(doneItem, false);
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView);
        }

        void InitializeHandlers()
        {
            if (doneItem != null)
                doneItem.Clicked += DoneItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (doneItem != null)
                doneItem.Clicked -= DoneItem_Clicked;
        }

        void DoneItem_Clicked(object sender, EventArgs e) => NavigationController.DismissViewController(true, null);

        public void ObjectLinkSelected(ObjectLink link)
        {
            ObjectType switchObjectType;
            int objectId;

            if (businessEntity.ObjectType == link.FromObjectType && businessEntity.Id == link.FromObjectId)
            {
                switchObjectType = link.ToObjectType;
                objectId = link.ToObjectId;
            }
            else
            {
                switchObjectType = link.FromObjectType;
                objectId = link.FromObjectId;
            }

            switch (switchObjectType)
            {
                case ObjectType.Document:
                    PresentDocumentViewController(objectId);
                    break;
                case ObjectType.Contact:
                    PresentContactViewController(objectId);
                    break;
                case ObjectType.Shortcode:
                    PresentShortcodeViewController(objectId);
                    break;
            }
        }

        public void PresentDocumentViewController(int documentId)
        {
            var vc = new DocumentViewController
            {
                Modal = true
            };
            vc.SetRefreshDataOnAppear();
            vc.SetData(documentId);

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        public void PresentContactViewController(int contactId)
        {
            var vc = new ContactViewController
            {
                Modal = true
            };
            vc.SetRefreshDataOnAppear();
            vc.SetData(contactId);

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        public void PresentShortcodeViewController(int shortcodeId)
        {
            var vc = new ShortcodeViewController
            {
                Modal = true
            };
            vc.SetRefreshDataOnAppear();
            vc.SetData(shortcodeId);

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        async Task RefreshData()
        {
            CommonConfig.Logger.Info($"Refreshing list of links");

            try
            {
                var objectLinks = await Managers.CommonActionsManager.GetObjectLinksAsync(businessEntity);

                ProcessObjectLinks(objectLinks);

                var grouppedObjectLinks = objectLinks.OrderBy(ol => ol.TypeInfo.DescriptionSimple)
                                                     .GroupBy(ol => ol.IsReverse ? ol.TypeInfo.DescriptionComplexReverse : ol.TypeInfo.DescriptionComplex)
                                                     .ToDictionary(kv => kv.Key, kv => kv.ToArray());
                ((DataSource)TableView.Source).SetItems(grouppedObjectLinks);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh list of links", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);

                NavigationController.DismissViewController(true, null);
            }
        }

        void ProcessObjectLinks(List<ObjectLink> ols)
        {
            foreach (var ol in ols)
            {
                ol.TypeInfo.DescriptionAction = ProcessString(ol.TypeInfo.DescriptionAction, ol.FromObjectType, ol.ToObjectType);
                ol.TypeInfo.DescriptionActionReverse = ProcessString(ol.TypeInfo.DescriptionActionReverse, ol.FromObjectType, ol.ToObjectType);
                ol.TypeInfo.DescriptionComplex = ProcessString(ol.TypeInfo.DescriptionComplex, ol.FromObjectType, ol.ToObjectType);
                ol.TypeInfo.DescriptionComplexReverse = ProcessString(ol.TypeInfo.DescriptionComplexReverse, ol.FromObjectType, ol.ToObjectType);
                ol.TypeInfo.DescriptionSimple = ProcessString(ol.TypeInfo.DescriptionSimple, ol.FromObjectType, ol.ToObjectType);
            }
        }

        string ProcessString(string str, ObjectType from, ObjectType to)
        {
            if (str.Contains("%"))
            {
                str = str.Replace("%ObjFromName%", from.ToString());
                str = str.Replace("%ObjToName%", to.ToString());
            }

            return str;
        }

        class DataSource : UITableViewSource, IDisposable
        {
            public bool Empty => objectLinksInView.Count < 1;

            readonly WeakReference<ObjectLinksListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            string[] objectLinksSections = new string[0];
            Dictionary<string, ObjectLink[]> objectLinksInView = new Dictionary<string, ObjectLink[]>();

            public DataSource(ObjectLinksListViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (objectLinksInView.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(Localization.GetString("no_object_links"));
                    return emptyCell;
                }

                var section = objectLinksSections[indexPath.Section];
                var ol = objectLinksInView[section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(ObjectLinksTableViewCell.Key) as ObjectLinksTableViewCell ?? ObjectLinksTableViewCell.Create();
                cell.Initialize(ol);

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (objectLinksInView.Count < 1)
                    return 1;

                var sectionName = objectLinksSections[section];
                return objectLinksInView[sectionName].Length;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (loading)
                    return 1;

                if (objectLinksInView.Count < 1)
                    return 1;

                return objectLinksSections.Length;
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (loading)
                    return string.Empty;

                if (objectLinksInView.Count < 1)
                    return string.Empty;

                return objectLinksSections[section];
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => ObjectLinksTableViewCell.Height;

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.CellAt(indexPath).SelectionStyle == UITableViewCellSelectionStyle.None)
                    return;

                var section = objectLinksSections[indexPath.Section];
                var ol = objectLinksInView[section][indexPath.Row];

                viewControllerWeakReference.Unwrap()?.ObjectLinkSelected(ol);
            }

            public void SetItems(Dictionary<string, ObjectLink[]> objectLinks)
            {
                loading = false;

                objectLinksSections = objectLinks.Keys.ToArray();
                objectLinksInView = new Dictionary<string, ObjectLink[]>(objectLinks);

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                if (objectLinksSections.Length > 1)
                    tableViewWeakReference.Unwrap()?.InsertSections(NSIndexSet.FromNSRange(new NSRange(1, objectLinksSections.Length - 1)), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public void Reset()
            {
                loading = true;

                objectLinksSections = new string[0];
                objectLinksInView.Clear();

                var sectionsCount = tableViewWeakReference.Unwrap()?.NumberOfSections() ?? 0;

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                if (sectionsCount > 1)
                    tableViewWeakReference.Unwrap()?.DeleteSections(NSIndexSet.FromNSRange(new NSRange(1, sectionsCount - 1)), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }
        }
    }
}