//
// Project: Mark5.Mobile.Droid
// File: CustomArrayAdapter.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Views;
using Android.Widget;
using Android.Content;
using System.Collections;

namespace Mark5.Mobile.Droid.Ui.Common
{

    public class CustomArrayAdapter : ArrayAdapter
    {


        public static CustomArrayAdapter Create(Context context, int textArrayResId, int textViewResId, int dropDownViewResId)
        {
            var strings = context.Resources.GetStringArray(textArrayResId);
            var adapter = new CustomArrayAdapter(context, textViewResId, strings);
            adapter.SetDropDownViewResource(dropDownViewResId);
            return adapter;
        }

        public CustomArrayAdapter(Context context, int textViewResId, IList objects)
            : base(context, textViewResId, objects)
        {
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = base.GetView(position, convertView, parent);
            view.SetPadding(0, view.PaddingTop, view.PaddingRight, view.PaddingBottom);
            return view;
        }
    }
}
