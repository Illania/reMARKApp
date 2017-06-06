//
// Project: Mark5.Mobile.Droid
// File: PickCountryFragment.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickCountryFragment : RetainableStateFragment
    {
        RecyclerView recyclerView;
        CountriesListViewAdapter adapter;

        public Action<int> CloseRequest { get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(PickCountryFragment)}]");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));

            adapter = new CountriesListViewAdapter(this);
            recyclerView.SetAdapter(adapter);

            HasOptionsMenu = false;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity) Activity).SupportActionBar.Subtitle = GetString(Resource.String.search_country);

            CommonConfig.Logger.Info($"Created {nameof(PickCountryFragment)}");
        }

        public override void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(PickCountryFragment)}");
                RefreshData();
            }
        }

        public void RefreshData()
        {
            adapter.SetItems(ServerConfig.SystemSettings.ContactsModuleInfo.Countries);
        }

        public void CountrySelected(CountryInfo c)
        {
            if (CloseRequest != null)
                CloseRequest(c.FaxPrefix);
            ((AppCompatActivity) Activity).OnBackPressed();
        }

        #region Retained State

        public override string GenerateTag()
        {
            return $"{nameof(PickLinesListFragment)}";
        }

        #endregion

        class CountriesListViewAdapter : RecyclerView.Adapter
        {
            readonly List<CountryInfo> countriesInView = new List<CountryInfo>(500);

            public override int ItemCount
            {
                get { return countriesInView.Count; }
            }

            public List<CountryInfo> Items
            {
                get { return countriesInView; }
            }

            readonly PickCountryFragment parent;

            public CountriesListViewAdapter(PickCountryFragment parent)
            {
                this.parent = parent;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var c = countriesInView[position];
                var lvh = holder as CountryViewHolder;

                lvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => HandleClick(c)));

                lvh.Name = c.Name;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.search_list_item_country, parent, false);
                return new CountryViewHolder(itemView);
            }

            public void SetItems(List<CountryInfo> countries)
            {
                var count = countriesInView.Count;
                countriesInView.AddRange(countries);
                NotifyItemRangeInserted(count, countries.Count);
            }

            void HandleClick(CountryInfo c)
            {
                parent.CountrySelected(c);
            }
        }

        class CountryViewHolder : RecyclerView.ViewHolder
        {
            public string Name
            {
                set { nameTextView.Text = value; }
            }

            readonly AppCompatTextView nameTextView;

            public CountryViewHolder(View itemView)
                : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.search_list_item_country_name);
            }
        }
    }
}