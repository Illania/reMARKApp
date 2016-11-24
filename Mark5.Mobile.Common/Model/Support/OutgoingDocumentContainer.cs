//
// Project: Mark5.Mobile.Common
// File: OutgoingDocumentContainer.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    public class OutgoingDocumentContainer
    {

        public Document Document { get; set; }
        public DocumentPreview DocumentPreview { get; set; }
        public OutgoingDocumentInfo Info { get; set; }
    }
}

