//
// Project: Mark5.Mobile.Droid
// File: handledSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentsSearchViews
{

    public class HandledSearchView : DocumentsSearchView
    {

        readonly AppCompatTextView handledTitle;
        readonly AppCompatTextView handledSubtitle;

        bool? SelectedHandled;

        public HandledSearchView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
            SetBackgroundResource(typedArray.GetResourceId(0, 0));
            typedArray.Recycle();

            Clickable = true;
            Click += async (sender, e) =>
            {
                SelectedHandled = await Dialogs.ShowSingleSelectDialogAsync(context, Resource.String.search_handled, new List<bool?> { null, true, false }, SelectedHandled, displayText: TextForValue);
                handledSubtitle.Text = TextForValue(SelectedHandled);
            };

            handledTitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            handledTitle.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            handledTitle.SetText(Resource.String.search_handled);
            AddView(handledTitle);

            handledSubtitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            handledSubtitle.Text = TextForValue(SelectedHandled);
            handledSubtitle.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            AddView(handledSubtitle);
        }

        string TextForValue(bool? value)
        {
            if (value == null)
            {
                return Context.GetString(Resource.String.search_handled_none_selected);
            }
            if (value.Value)
            {
                return Context.GetString(Resource.String.search_handled_true);
            }

            return Context.GetString(Resource.String.search_handled_false);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            SelectedHandled = criteria.Handled;
            handledSubtitle.Text = TextForValue(SelectedHandled);
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.Handled = SelectedHandled;
        }
    }
}
