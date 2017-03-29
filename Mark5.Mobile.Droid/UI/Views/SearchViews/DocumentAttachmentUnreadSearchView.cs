//
// Project: Mark5.Mobile.Droid
// File: DocumentAttachmentUnreadSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentAttachmentUnreadSearchView : AbstractButtonsSearchView<SearchDocumentsCriteria>
    {
        CustomButton withAttachmentsButton;
        CustomButton unreadEmailsButton;

        public DocumentAttachmentUnreadSearchView(Android.Content.Context context) : base(context)
        {
            withAttachmentsButton = new CustomButton(context, Resource.String.search_document_with_attachments);
            unreadEmailsButton = new CustomButton(context, Resource.String.search_document_unread);

            AddButtons(withAttachmentsButton, unreadEmailsButton);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            //TODO       
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            //TODO       
        }
    }
}
