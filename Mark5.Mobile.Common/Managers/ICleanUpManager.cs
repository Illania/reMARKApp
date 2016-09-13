//
// Project: Mark5.Mobile.Common
// File: ICleanUpManager.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common
{

    public interface ICleanUpManager
    {

        Task<bool> IsCleanUpNecessary(int intervalDays);

        Task CleanUp(IEnumerable<ModuleType> modules = null);
    }
}

