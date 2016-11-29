//
// Project: Mark5.Mobile.Common
// File: ITester.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Tester
{

    public interface ITester
    {

        Task<bool> CanTest(CancellationToken ct = default(CancellationToken));

        Task<bool> Test(CancellationToken ct = default(CancellationToken));
    }
}

