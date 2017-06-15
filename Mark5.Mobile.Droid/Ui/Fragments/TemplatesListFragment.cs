
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class TemplatesListFragment : RetainableStateFragment
    {
        #region Retainable State

        public override string GenerateTag() => $"{nameof(TemplatesListFragment)}";

        #endregion

        class TemplatesListAdapter : RecyclerView.Adapter
        {
            public event EventHandler<TemplatePreview> ItemClicked = delegate { };

            public static class ViewType
            {
                public const int TemplateView = 0;
                public const int SectionView = 1;
            }

            public static class Section
            {
                public const int Private = 0;
                public const int Public = 1;
            }

            List<List<TemplatePreview>> templatesInView = new List<List<TemplatePreview>>(2)
            {
                new List<TemplatePreview>(),
                new List<TemplatePreview>(),
            };

            public override int ItemCount => templatesInView.Sum(f => f.Count) + 2;

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                if (holder is SectionViewHolder sectionViewHolder)
                {
                    var section = GetSectionAtPosition(position);
                    var title = section == Section.Private ? holder.ItemView.Context.GetString(Resource.String.private_header)
                                                  : holder.ItemView.Context.GetString(Resource.String.public_header);

                    sectionViewHolder.SectionTitle = title;
                }

                if (holder is TemplateViewHolder templateViewHolder)
                {
                    var preview = GetItemAtPosition(position);
                    templateViewHolder.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, preview)));
                    templateViewHolder.Name = preview.Name;
                }
            }

            TemplatePreview GetItemAtPosition(int position)
            {
                var privateCount = templatesInView[Section.Private].Count;
                var publicCount = templatesInView[Section.Public].Count;

                if (position < privateCount)
                {
                    return templatesInView[Section.Private][position - 1];
                }

                return templatesInView[Section.Public][position - 2];
            }

            int GetSectionAtPosition(int position)
            {
                var privateCount = templatesInView[Section.Private].Count;
                var publicCount = templatesInView[Section.Public].Count;
                if (privateCount == 0 || position < privateCount) //We consider that private come before public
                    return Section.Public;
                else
                    return Section.Private;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                if (viewType == ViewType.SectionView)
                {
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_section, parent, false);
                    return new SectionViewHolder(itemView);
                }
                else
                {
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_templates, parent, false);
                    return new TemplateViewHolder(itemView);
                }
            }

            public void RefreshData(List<TemplatePreview> previews)
            {
                NotifyItemRangeRemoved(0, ItemCount);

                templatesInView.Clear();
                templatesInView[Section.Private].AddRange(previews.Where(p => p.Private));
                templatesInView[Section.Public].AddRange(previews.Where(p => !p.Private));

                NotifyItemRangeInserted(0, ItemCount);
            }

            #region RecyclerView ViewHolders

            class TemplateViewHolder : RecyclerView.ViewHolder
            {
                public string Name { set => nameTextView.Text = value; }

                readonly AppCompatTextView nameTextView;

                public TemplateViewHolder(View itemView)
                    : base(itemView)
                {
                    // Locate and cache view references
                    nameTextView = itemView as AppCompatTextView;
                }
            }

            class SectionViewHolder : RecyclerView.ViewHolder
            {
                public string SectionTitle { set => sectionTitleTextView.Text = value; }

                public AppCompatTextView sectionTitleTextView;

                public SectionViewHolder(View itemView)
                    : base(itemView)
                {
                    // Locate and cache view references
                    sectionTitleTextView = itemView as AppCompatTextView;
                }
            }

            #endregion
        }

    }

}

