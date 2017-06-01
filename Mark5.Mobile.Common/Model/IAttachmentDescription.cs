//
// Project: Mark5.Mobile.IOS
// File: IAttachmentDescription.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

namespace Mark5.Mobile.Common.Model
{
    public interface IAttachmentDescription
    {
        string Name { get; set; }

        long SizeInBytes { get; set; }
    }
}
