//
// Project: Mark5.Mobile.Common
// File: IBusinessEntity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;

namespace Mark5.Mobile.Common.Model
{
    public interface IBusinessEntity
    {

        int Id { get; set; }

        Guid Guid { get; set; }

        ObjectType ObjectType { get; }

        ModuleType ModuleType { get; }
    }
}

