//
// Project: Mark5.Mobile.Common
// File: AttachmentDescription.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using Newtonsoft.Json;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    public class AttachmentDescription : IAttachmentDescription
    {

        [JsonIgnore]
        static readonly char[] invalidChars = { '\\', '/', ':', '?', '|', '*', '"', '<', '>', '\0', '\b', '\t' };

        public int Id { get; set; } = -1;

        public string Name { get; set; }

        [JsonIgnore]
        string safeName;

        [JsonIgnore]
        public string SafeName
        {
            get
            {
                if (Name == null)
                    return null;

                if (string.IsNullOrWhiteSpace(safeName))
                {
                    var chars = Name.ToCharArray();

                    for (int i = 0; i < chars.Length; i++)
                    {
                        var c = chars[i];
                        if (char.IsControl(c) || Array.IndexOf(invalidChars, c) >= 0)
                            chars[i] = '_';
                    }

                    safeName = new string(chars);
                }

                return safeName;
            }
        }

        public long SizeInBytes { get; set; }
    }

    public interface IAttachmentDescription
    {
        string Name { get; set; }

        long SizeInBytes { get; set; }
    }
}

