using System.Collections.Generic;
using System.Linq;
using Android.Content;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Droid.Ui.Fragments;

namespace reMark.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentCategoriesSearchView : AbstractCategoriesSearchView<SearchDocumentsCriteria>
    {
        public DocumentCategoriesSearchView(Context context, ISearchCriteriaFragment fragment)
            : base(context, fragment, ObjectType.Document)
        {
        }

        public override void Refresh()
        {
            UpdateCategories(Criteria.CategoryIds);
        }

        public override void UpdateCriteria()
        {
            Criteria.CategoryIds.Clear();
            Criteria.CategoryIds.AddRange(selectedCategoryIds);
        }
    }
}