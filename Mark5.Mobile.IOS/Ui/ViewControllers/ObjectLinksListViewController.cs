//
// Project: Mark5.Mobile.IOS
// File: ObjectLinksListViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class ObjectLinksListViewController : AbstractViewController
    {

        readonly IBusinessEntity businessEntity;

        UIBarButtonItem doneItem;
        UITableView tableView;

        public ObjectLinksListViewController(IBusinessEntity businessEntity)
        {
            this.businessEntity = businessEntity;
        }

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeNavigationBarTitle();
            InitializeHandlers();

            if (tableView?.IndexPathForSelectedRow != null)
                tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);

            if (tableView?.IndexPathsForSelectedRows?.Length > 0)
                foreach (var selectedIndexPath in tableView?.IndexPathsForSelectedRows)
                    tableView.DeselectRow(selectedIndexPath, true);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public override async void ViewDidAppear(bool animated)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(ObjectLinksListViewController)} appeared");

            var ds = (DataSource)tableView.Source;
            if (ds.Empty)
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

            var ds = tableView?.Source as DataSource;
            ds?.Reset();

            base.DidReceiveMemoryWarning();
        }

        void InitializeNavigationBar()
        {
            doneItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            NavigationItem.SetRightBarButtonItem(doneItem, false);
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            tableView = new UITableView(CGRect.Empty, UITableViewStyle.Grouped);
            tableView.ClipsToBounds = false;
            tableView.Source = new DataSource(this, tableView, Localization.GetString("no_object_links"));
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

        void InitializeNavigationBarTitle()
        {
            NavigationItem.Title = Localization.GetString("links");
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

        void DoneItem_Clicked(object sender, EventArgs e)
        {
            NavigationController.DismissViewController(true, null);
        }

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
            var vc = new DocumentViewController();
            vc.Modal = true;
            vc.SetRefreshDataOnAppear();
            vc.SetData(documentId);

            var navigationController = new UINavigationController(vc);
            navigationController.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;

            PresentViewController(navigationController, true, null);
        }

        public void PresentContactViewController(int contactId)
        {
            var vc = new ContactViewController();
            vc.Modal = true;
            vc.SetRefreshDataOnAppear();
            vc.SetData(contactId);

            var navigationController = new UINavigationController(vc);
            navigationController.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;

            PresentViewController(navigationController, true, null);
        }

        public void PresentShortcodeViewController(int shortcodeId)
        {
            var vc = new ShortcodeViewController();
            vc.Modal = true;
            vc.SetRefreshDataOnAppear();
            vc.SetData(shortcodeId);

            var navigationController = new UINavigationController(vc);
            navigationController.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;

            PresentViewController(navigationController, true, null);
        }

        async Task RefreshData()
        {
            CommonConfig.Logger.Info($"Refreshing list of links");

            try
            {
                var objectLinks = await Managers.CommonActionsManager.GetObjectLinksAsync(businessEntity);

                ProcessObjectLinks(objectLinks);

                var grouppedObjectLinks = objectLinks.OrderBy(ol => ol.TypeInfo.DescriptionSimple).GroupBy(ol => ol.IsReverse ? ol.TypeInfo.DescriptionComplexReverse : ol.TypeInfo.DescriptionComplex).ToDictionary(kv => kv.Key, kv => kv.ToArray());
                var ds = (DataSource)tableView.Source;
                ds.SetItems(grouppedObjectLinks);
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

            public bool Empty
            {
                get
                {
                    return objectLInksInView.Count < 1;
                }
            }

            ObjectLinksListViewController viewController;
            UITableView tableView;
            string emptyText;

            bool loading = true;
            string[] objectLinksSections = new string[0];
            Dictionary<string, ObjectLink[]> objectLInksInView = new Dictionary<string, ObjectLink[]>();

            public DataSource(ObjectLinksListViewController viewController, UITableView tableView, string emptyText)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.emptyText = emptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                {
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();
                }

                if (objectLInksInView.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var section = objectLinksSections[indexPath.Section];
                var ol = objectLInksInView[section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(ObjectLinksTableViewCell.Key) as ObjectLinksTableViewCell ?? ObjectLinksTableViewCell.Create();
                cell.Initialize(ol);

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (objectLInksInView.Count < 1)
                    return 1;

                var sectionName = objectLinksSections[section];
                return objectLInksInView[sectionName].Length;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (loading)
                    return 1;

                if (objectLInksInView.Count < 1)
                    return 1;

                return objectLinksSections.Length;
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (loading)
                    return string.Empty;

                if (objectLInksInView.Count < 1)
                    return string.Empty;

                return objectLinksSections[section];
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 72f;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.CellAt(indexPath).SelectionStyle == UITableViewCellSelectionStyle.None) return;

                var section = objectLinksSections[indexPath.Section];
                var ol = objectLInksInView[section][indexPath.Row];

                viewController.ObjectLinkSelected(ol);
            }

            public void SetItems(Dictionary<string, ObjectLink[]> objectLinks)
            {
                loading = false;

                objectLinksSections = objectLinks.Keys.ToArray();
                objectLInksInView = new Dictionary<string, ObjectLink[]>(objectLinks);

                tableView.BeginUpdates();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                if (objectLinksSections.Length > 1)
                    tableView.InsertSections(NSIndexSet.FromNSRange(new NSRange(1, objectLinksSections.Length - 1)), UITableViewRowAnimation.Fade);
                tableView.EndUpdates();
            }

            public void Reset()
            {
                loading = true;

                var sectionsCount = objectLinksSections.Length;

                objectLinksSections = new string[0];
                objectLInksInView.Clear();

                tableView.BeginUpdates();
                tableView.DeleteSections(NSIndexSet.FromNSRange(new NSRange(1, sectionsCount - 1)), UITableViewRowAnimation.Fade);
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableView.EndUpdates();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                objectLinksSections = new string[0];
                objectLInksInView = null;
            }
        }
    }
}
