//
// Project: Mark5.Mobile.Common
// File: CommentsValue.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;
using SQLite;

namespace Mark5.Mobile.Common.Model
{

    public class CommentsValue
    {

        [Column("CommentsBytes")]
        public byte[] CommentsBytes { get; set; }

        [Ignore]
        public List<Comment> Comments
        {
            get
            {
                return SerializationUtils.DeserializeFromByteArray<List<Comment>>(CommentsBytes);
            }
            set
            {
                CommentsBytes = SerializationUtils.SerializeToByteArray(value);
            }
        }
    }
}

