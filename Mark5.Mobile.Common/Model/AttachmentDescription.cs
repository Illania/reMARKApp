using System;
using Newtonsoft.Json;

namespace Mark5.Mobile.Common.Model
{
    public class AttachmentDescription : IAttachmentDescription
    {
        [JsonIgnore] static readonly char[] invalidChars =
        {
            '\\',
            '/',
            ':',
            '?',
            '|',
            '*',
            '"',
            '<',
            '>',
            '\0',
            '\b',
            '\t'
        };

        public int Id { get; set; } = -1;

        public string Name { get; set; }
        [JsonIgnore] string safeName;

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

                    for (var i = 0; i < chars.Length; i++)
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
}