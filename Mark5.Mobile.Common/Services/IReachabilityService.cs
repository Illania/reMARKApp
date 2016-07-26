//
// Project: Mark5.Mobile.Common
// File: IReachabilityService.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Threading.Tasks;

namespace Mark5.Mobile.Common.Services
{
    public interface IReachabilityService
    {

        Task<bool> IsServiceReachable();

    }
}

