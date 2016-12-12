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

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    public class CommentsValue
    {

        [Column("CommentsString")]
        public string CommentsString { get; set; }

        [Ignore]
        public List<Comment> Comments
        {
            get
            {
                return SerializationUtils.Deserialize<List<Comment>>(CommentsString);
            }
            set
            {
                CommentsString = SerializationUtils.Serialize(value);
            }
        }
    }
}

