//
// File: OnFastScrollStateChangeListener.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

namespace FastScrollRecycler
{
    public interface IOnFastScrollStateChangeListener
    {
        void OnFastScrollStart();

        void OnFastScrollStop();
    }
}