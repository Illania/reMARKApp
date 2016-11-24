//
// Project: Mark5.Mobile.Common
// File: IOutgoingDocumentsManager.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Threading.Tasks;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common
{

    public interface IOutgoingDocumentsManager
    {

        void Notify(Guid identifier);

        Task<bool> IsRunning();

        Task Start();

        Task Stop();
    }
}

