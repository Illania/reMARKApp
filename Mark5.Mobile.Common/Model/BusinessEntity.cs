//
// Project: Mark5.Mobile.Common
// File: BusinessEntity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using SQLite;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    public abstract class BusinessEntity : IBusinessEntity
    {

        [Column("Id"), PrimaryKey]
        public int Id { get; set; } = -1;

        [Column("Guid"), NotNull]
        public Guid Guid { get; set; }

        [Ignore]
        public abstract ObjectType ObjectType { get; }

        [Ignore]
        public abstract ModuleType ModuleType { get; }
    }
}

