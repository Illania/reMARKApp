//
// Project: Mark5.Mobile.Common
// File: DownloadPolicy.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common
{

    public abstract class DownloadPolicy
    {
    }

    public class DownloadAllPolicy : DownloadPolicy
    {
    }

    public class DownloadFoldersPolicy : DownloadPolicy
    {

        public List<int> FolderIds { get; } = new List<int>();
    }
}

