//
// File: ISectionedAdapter.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

namespace FastScrollRecycler
{
    public interface ISectionedAdapter
    {
        string GetSectionName(int position);
    }
}