using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.SearchView
{
    public class DocumentsSearchByReferenceResultsFilterController:UITableViewController
    {
        public DocumentsSearchByReferenceResultsViewController DocumentSearchResultsController { get; set; }
       
        public override void LoadView()
        {
            base.LoadView();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
        }


        void InitializeView()
        {
            var searchResultsDataSource = new DocumentsSearchByReferenceResultsDataSource(DocumentSearchResultsController, TableView, PlatformConfig.Preferences.CompactDocumentsList);
            TableView.Source = searchResultsDataSource;
            TableView.EstimatedRowHeight = 60f;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.AllowsSelection = true;
            TableView.UserInteractionEnabled = true;      
        }

        public async void SearchDocuments(string searchText, CancellationToken ct)
        {        
            var dataSource = TableView?.Source as DocumentsSearchByReferenceResultsDataSource;
            dataSource?.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested)
                return;

            var ds = (DocumentsSearchByReferenceResultsDataSource)DocumentSearchResultsController.TableView.Source;
            var filteredDocuments = ds.Items.Where(dp => MatchesQuery(dp, searchText)).ToList();

            if (ct.IsCancellationRequested)
                return;

            dataSource?.AppendItems(filteredDocuments);
        }

        static bool MatchesQuery(DocumentPreview dp, string query)
        {
#if DEBUG
            if (dp.Id.ToString() == query)
                return true;
#endif

            if (dp.ReferenceNumber?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (dp.Subject?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (dp.Preview?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (dp.Addresses.Any(da => da.Name?.ContainsCaseInsensitive(query) ?? false))
                return true;

            if (dp.Addresses.Any(da => da.Address?.ContainsCaseInsensitive(query) ?? false))
                return true;

            if (dp.Categories.Any(da => da.Name?.ContainsCaseInsensitive(query) ?? false))
                return true;

            if (dp.Creator?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }

    }
}
