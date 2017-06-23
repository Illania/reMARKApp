﻿using System;
using System.Threading.Tasks;

namespace Mark5.Mobile.Common.Services
{
    public interface IDocumentsUploadService
    {
        void Notify(Guid identifier);
        Task<bool> IsRunning();
        Task Start();
        Task Stop();
    }
}