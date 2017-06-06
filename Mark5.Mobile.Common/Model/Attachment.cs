//
// File: Attachment.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System.IO;

namespace Mark5.Mobile.Common
{
    public class Attachment
    {
        public string Filename { get; set; }
        public string Extension { get; set; }
        public int Size { get; set; }
        public string Md5 { get; set; }
        public Stream Stream { get; set; }
    }
}