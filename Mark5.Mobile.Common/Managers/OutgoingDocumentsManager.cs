using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common
{
    public class OutgoingDocumentsManager
    {
        static readonly OutgoingDocumentsManager sharedInstance = new OutgoingDocumentsManager();

        bool initialized;
        CancellationTokenSource cts;
        Task sendTask;

        public static OutgoingDocumentsManager SharedInstance
        {
            get
            {
                return sharedInstance;
            }
        }

        ICrossPlatformConcurrentQueue<OutgoingDocumentContainer> queue;
        public ICrossPlatformConcurrentQueue<OutgoingDocumentContainer> Queue
        {
            set
            {
                if (queue != null)
                {
                    throw new InvalidOperationException("Queue has already been set!");
                }

                queue = value;
            }
        }

        static OutgoingDocumentsManager()
        {
        }

        OutgoingDocumentsManager()
        {
        }

        public void DocumentsArrived()
        {
            throw new NotImplementedException();
        }

        //TODO I'm also considering that the unlocking of a document will be notified to the queue, so we can just skip a file if needed

        void AddToQueue(OutgoingDocumentContainer container)
        {
            queue.TryAdd(container);
        }

        void AddToQueue(IEnumerable<OutgoingDocumentContainer> containers)
        {
            foreach (var container in containers)
            {
                AddToQueue(container);
            }
        }

        public void Initialize()
        {
            //Need to recover things from filesystem, and put in the queue
            //Need to hook up on reachability change events

            initialized = true;
        }

        public void Start()
        {
            if (!initialized)
            {
                Initialize();
            }

            if (sendTask != null)
            {
                return;
            }

            //Need to check reachability here

            sendTask = Task.Run(async () => await SendAction()).ContinueWith((t) =>
            {
                if (t.IsFaulted)
                {
                    //TODO need to log the exception
                }

                sendTask = null;
            });
        }

        public void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts = null;
            }

            sendTask = null;
        }

        async Task SendAction()
        {
            while (cts.IsCancellationRequested)
            {
                OutgoingDocumentContainer container;
                queue.TryTake(out container, -1, cts.Token);

                //DO something with files

                //If successful, need to delete document folder
                //If not, need to save the "failed" status (probably we need to save the exception or something)

                //Send notification that a certain document was sent


            }
        }

    }

}

