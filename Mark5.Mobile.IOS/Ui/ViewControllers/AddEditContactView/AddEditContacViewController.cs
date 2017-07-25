using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.AddEditContactView
{
    public class AddEditContacViewController : AbstractViewController
    {
        public Contact Contact { get; set; }
        public ContactPreview ContactPreview { get; set; }
        public ContactType ContactType { get; set; }
        public ContactCreationModeFlag CreationModeFlag { get; set; }
        public ContactPreview ParentContactPreview { get; set; }
        public bool ParentPreselected { get; set; }

        UIBarButtonItem editButton;
        UIBarButtonItem cancelButton;

        UITableView tableView;

        #region UIViewControllerOverrides

        public override void LoadView()
        {
            base.LoadView();

            InitNavigationBar();
            InitView();
        }

        //TODO eventually put logging

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }


        #endregion

        #region Init methods

        void InitNavigationBar()
        {
            cancelButton = new UIBarButtonItem();
            cancelButton.Title = Localization.GetString("cancel");
            NavigationItem.SetLeftBarButtonItem(cancelButton, false);

            editButton = new UIBarButtonItem();
            editButton.Title = "Edit"; //TODO put right one
            //editButton.Image = UIImage.FromBundle(Path.Combine("icons", "compose.png"));
            editButton.Enabled = false;
            NavigationItem.SetRightBarButtonItem(editButton, false);
        }

        void InitView()
        {
            tableView = new UITableView(CGRect.Empty, UITableViewStyle.Plain);
            tableView.Source = new DataSource(this, tableView);
            tableView.TableFooterView = new UIView();
            tableView.EstimatedRowHeight = 60f;
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            tableView.ClipsToBounds = false;
            tableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.OnDrag;
            View.AddSubview(tableView);

            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f),
            });
        }

        void InitializeHandlers()
        {
            if (cancelButton != null)
                cancelButton.Clicked += CancelButton_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (cancelButton != null)
                cancelButton.Clicked -= CancelButton_Clicked;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            RefreshData();
        }

        void RefreshData()
        {
            var ds = (DataSource)tableView.Source;

            if (CreationModeFlag == ContactCreationModeFlag.New)
            {
                Contact = new Contact();
                ContactPreview = new ContactPreview();
                ContactPreview.Type = ContactType;
            }

            ds.Refresh(Contact, ContactPreview, ParentContactPreview, CreationModeFlag, ParentPreselected);
        }

        #endregion

        #region Handlers

        void CancelButton_Clicked(object sender, EventArgs e) //TODO move
        {
            DismissViewController(true, null);
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {
            AddEditContacViewController viewController;
            UITableView tableView;

            SectionCollection sections = new SectionCollection();

            public DataSource(AddEditContacViewController viewController, UITableView tableView)
            {
                this.viewController = viewController;
                this.tableView = tableView;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var row = sections[indexPath.Section].Rows[indexPath.Row];
                var cell = tableView.DequeueReusableCell(row.Key) ?? row.CreateCell();
                row.Bind(cell);
                return cell;
            }

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                return sections[(int)section].Rows.Count;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return sections.Count;
            }

            public void Refresh(Contact contact, ContactPreview contactPreview, ContactPreview parentContactPreview,
                                ContactCreationModeFlag creationMode, bool parentPreselected)
            {
                var sectionsToInsert = new List<AbstractSection> { new GeneralSection() };

                foreach (var section in sectionsToInsert)
                {
                    section.Contact = contact;
                    section.ContactPreview = contactPreview;
                    section.ParentContactPreview = parentContactPreview;
                    section.CreationMode = creationMode;
                    section.ParentPreselected = parentPreselected;

                    section.InitializeRows();
                    sections.Add(section);
                }

                tableView.BeginUpdates();
                tableView.InsertSections(NSIndexSet.FromNSRange(new NSRange(0, sections.Count)), UITableViewRowAnimation.Fade);
                tableView.EndUpdates();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                tableView = null;
                viewController = null;

                sections = null;
            }

            #region Support classes

            class SectionCollection : List<AbstractSection> { }

            abstract class AbstractSection
            {
                public Contact Contact { get; set; }
                public ContactPreview ContactPreview { get; set; }
                public ContactPreview ParentContactPreview { get; set; }
                public ContactCreationModeFlag CreationMode { get; set; }
                public bool ParentPreselected { get; set; }

                public RowCollection Rows { get; } = new RowCollection();

                abstract public void InitializeRows();
            }

            class GeneralSection : AbstractSection
            {
                public override void InitializeRows()
                {
                    Rows.Add(new NameRow());
                    Rows.Add(new NameRow());
                    Rows.Add(new NameRow());

                    Rows.ForEach(r =>
                    {
                        r.Contact = Contact;
                        r.ContactPreview = ContactPreview;
                        r.ParentPreselected = ParentPreselected;
                        r.ParentContactPreview = ParentContactPreview;
                        r.CreationMode = CreationMode;
                    });
                }
            }

            class RowCollection : List<AbstractRow> { }

            abstract class AbstractRow
            {
                public Contact Contact { get; set; }
                public ContactPreview ContactPreview { get; set; }
                public ContactPreview ParentContactPreview { get; set; }
                public ContactCreationModeFlag CreationMode { get; set; }
                public bool ParentPreselected { get; set; }

                public abstract string Key { get; }

                public abstract UITableViewCell CreateCell();

                public abstract void Bind(UITableViewCell cell);
            }

            class NameRow : AbstractRow
            {
                public override string Key => TextFieldTableViewCell.Key;

                public override void Bind(UITableViewCell cell)
                {
                    var tfc = (TextFieldTableViewCell)cell;
                    tfc.Initialize("Name"); //TODO test
                }

                public override UITableViewCell CreateCell()
                {
                    return TextFieldTableViewCell.Create();
                }
            }

            #endregion

        }

    }
}
