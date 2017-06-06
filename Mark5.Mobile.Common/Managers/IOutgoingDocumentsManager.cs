//
// File: IOutgoingDocumentsManager.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common
{
    public interface IOutgoingDocumentsManager
    {
        event EventHandler<Guid> DocumentAddedToQueue;

        event EventHandler<OutgoingDocumentContainer> DocumentBeingSent;

        event EventHandler<OutgoingDocumentContainer> DocumentSendingSuccessful;

        event EventHandler<OutgoingDocumentContainer> DocumentSendingFailed;

        void Notify(Guid identifier);

        Task<bool> IsRunning();

        Task Start();

        Task Stop();
    }
}