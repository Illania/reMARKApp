using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public abstract class AbstractCategoriesSearchView<T> : AbstractDropdownSearchView<T>
    {
        protected readonly List<int> selectedCategoryIds = new List<int>();
        readonly ObjectType objectType;

        public AbstractCategoriesSearchView(Context context, ISearchCriteriaFragment fragment, ObjectType type)
            : base(context, Resource.String.search_categories, Resource.String.search_categories_none, fragment)
        {
            objectType = type;
        }

        protected override void ClickAction()
        {
            var pclf = new PickCategoriesListFragment(objectType, selectedCategoryIds.ToArray(), new CategoriesCloseRequest(UpdateCategories));

            ParentFragment.ReplaceFragment(pclf, pclf.GenerateTag());
        }

        protected void UpdateCategories(List<Category> categories)
        {
            UpdateCategories(categories.Select(c => c.Id).ToList());
        }

        protected void UpdateCategories(List<int> categoriesId)
        {
            selectedCategoryIds.Clear();
            selectedCategoryIds.AddRange(categoriesId);
            UpdateBottomTextView(categoriesId.Count);
            UpdateCriteria();
        }
    }
}