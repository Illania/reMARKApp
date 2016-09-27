//
// Project: Mark5.Mobile.Droid
// File: IDocumentView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{

    public interface IDocumentView
    {

        DocumentPreview DocumentPreview { get; set; }

        Document Document { get; set; }

        void RefreshView();
    }
}

