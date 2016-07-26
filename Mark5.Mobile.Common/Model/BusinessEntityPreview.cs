//
// Project: Mark5.Mobile.Common
// File: BusinessEntityPreview.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using SQLite;

namespace Mark5.Mobile.Common.Model
{

    public abstract class BusinessEntityPreview : IBusinessEntity
    {

        [Column("Id"), PrimaryKey]
        public int Id { get; set; } = -1;

        [Column("Guid"), NotNull]
        public Guid Guid { get; set; }
    }
}
