//
// Project: Mark5.Mobile.Common
// File: DownloadItemInfo.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common
{

    abstract class DownloadItemInfo
    {

        public int FolderId { get; set; }
        virtual public ObjectType Type { get; set; }
    }

    class ContactDownloadInfo : DownloadItemInfo
    {

        public override ObjectType Type
        {
            get
            {
                return ObjectType.Contact;
            }
        }
    }

    class ShortcodeDownloadInfo : DownloadItemInfo
    {

        public override ObjectType Type
        {
            get
            {
                return ObjectType.Shortcode;
            }
        }
    }

    class DocumentDownloadInfo : DownloadItemInfo
    {

        public override ObjectType Type
        {
            get
            {
                return ObjectType.Document;
            }
        }
    }
}

