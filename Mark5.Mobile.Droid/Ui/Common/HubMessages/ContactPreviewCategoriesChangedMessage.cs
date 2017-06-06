//
// Project: 
// File: ContactPreviewCategoriesChanged.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Common.HubMessages
{
    public class ContactPreviewCategoriesChangedMessage : TinyMessageBase
    {
        public int ContactPreviewId { get; private set; }

        public List<Category> Categories { get; private set; }

        public ContactPreviewCategoriesChangedMessage(object sender, int contactPreviewId, List<Category> categories)
            : base(sender)
        {
            ContactPreviewId = contactPreviewId;
            Categories = categories;
        }
    }
}