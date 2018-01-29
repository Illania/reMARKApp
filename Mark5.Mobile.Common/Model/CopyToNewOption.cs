using System;

namespace Mark5.Mobile.Common.Model
{
    [Flags]
    public enum CopyToNewOption
    {
        None = 0,
        Addresses = 1,
        Content = 2,
        Attachments = 4
    }
}