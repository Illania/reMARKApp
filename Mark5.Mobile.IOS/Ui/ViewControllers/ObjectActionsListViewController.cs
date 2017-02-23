//
// Project: Mark5.Mobile.IOS
// File: ObjectActionsListViewController.cs
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
    
    public class ObjectActionsListViewController : AbstractViewController
    {

        readonly IBusinessEntity businessEntity;

        UIBarButtonItem doneItem;
        UITableView tableView;

        public ObjectActionsListViewController(IBusinessEntity businessEntity)
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
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public override async void ViewDidAppear(bool animated)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(ObjectActionsListViewController)} appeared");

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
            CommonConfig.Logger.Warning($"{nameof(ObjectActionsListViewController)} received memory warning!");

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
            tableView.Source = new DataSource(tableView, Localization.GetString("no_object_actions"));
            tableView.AllowsSelection = false;
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
            NavigationItem.Title = Localization.GetString("actions");
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

        async Task RefreshData()
        {
            CommonConfig.Logger.Info($"Refreshing list of actions");

            try
            {
                var objectActions = await Managers.CommonActionsManager.GetObjectActionsAsync(businessEntity);
                var grouppedObjectActions = objectActions.OrderBy(oa => oa.ActionType).ThenBy(oa => oa.ActionTimeTimestamp).GroupBy(oa => oa.ActionType).ToDictionary(v => v.Key, v=>v.ToArray());
                var ds = (DataSource)tableView.Source;
                ds.SetItems(grouppedObjectActions);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh list of actions", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);

                NavigationController.DismissViewController(true, null);
            }
        }

        class DataSource : UITableViewSource, IDisposable
        {

            public bool Empty
            {
                get
                {
                    return objectActionsInView.Count < 1;
                }
            }

            UITableView tableView;
            string emptyText;

            bool loading = true;
            string[] objectActionsSections = new string[0];
            Dictionary<string, ObjectAction[]> objectActionsInView = new Dictionary<string, ObjectAction[]>();

            public DataSource(UITableView tableView, string emptyText)
            {
                this.tableView = tableView;
                this.emptyText = emptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                {
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();
                }

                if (objectActionsInView.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var section = objectActionsSections[indexPath.Section];
                var oa = objectActionsInView[section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(ObjectActionsTableViewCell.Key) as ObjectActionsTableViewCell ?? ObjectActionsTableViewCell.Create();
                cell.Initialize(oa);

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (objectActionsInView.Count < 1)
                    return 1;

                var sectionName = objectActionsSections[section];
                return objectActionsInView[sectionName].Length;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (loading)
                    return 1;

                if (objectActionsInView.Count < 1)
                    return 1;

                return objectActionsSections.Length;
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (loading)
                    return string.Empty;

                if (objectActionsInView.Count < 1)
                    return string.Empty;

                return objectActionsSections[section];
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 72f;
            }

            public void SetItems(Dictionary<string, ObjectAction[]> objectActions)
            {
                loading = false;

                objectActionsSections = objectActions.Keys.ToArray();
                objectActionsInView = new Dictionary<string, ObjectAction[]>(objectActions);

                tableView.BeginUpdates();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
                tableView.InsertSections(NSIndexSet.FromNSRange(new NSRange(1, objectActionsSections.Length - 1)), UITableViewRowAnimation.Automatic);
                tableView.EndUpdates();
            }

            public void Reset()
            {
                loading = true;

                var sectionsCount = objectActionsSections.Length;

                objectActionsSections = new string[0];
                objectActionsInView.Clear();

                tableView.BeginUpdates();
                tableView.DeleteSections(NSIndexSet.FromNSRange(new NSRange(1, sectionsCount - 1)), UITableViewRowAnimation.Automatic);
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
                tableView.EndUpdates();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                tableView = null;
                objectActionsSections = new string[0];
                objectActionsInView = null;
            }
        }
    }
}
