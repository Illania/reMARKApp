//
// Project: Mark5.Mobile.Droid
// File: FastScrollRecyclerView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Support.V7.Widget;

namespace FastScrollRecycler
{
    public class FastScrollRecyclerView : RecyclerView
    {

        public FastScrollRecyclerView(IntPtr javaReference, Android.Runtime.JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public FastScrollRecyclerView(Android.Content.Context context)
            : base(context)
        {
        }

        public FastScrollRecyclerView(Android.Content.Context context, Android.Util.IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public FastScrollRecyclerView(Android.Content.Context context, Android.Util.IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
        }

        public int GetScrollBarWidth()
        {
            throw new NotImplementedException();
        }

        internal int GetScrollBarThumbHeight()
        {
            throw new NotImplementedException();
        }

        internal string ScrollToPositionAtProgress(float v)
        {
            throw new NotImplementedException();
        }
   }
}
