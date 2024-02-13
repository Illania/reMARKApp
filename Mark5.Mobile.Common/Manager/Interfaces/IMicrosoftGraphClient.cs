using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Classes.Azure;
using Microsoft.Graph.Models;

namespace Mark5.Mobile.Classes.AuthService
{
    public interface IMicrosoftGraphClient
    {
        public Task<AzureApplicationProxyInfo> GetAzureApplicationProxyInfo();

        public Task<string> Authenticate(object parentWindow, bool forceInteractive = true);

        public Task<AzureUser> GetAzureUser();

        public Task<List<AzureEndpointInfo>> GetAzureEndpointInfoList();

        public Task<Event> ImportFromICal((string Id, List<Common.Model.Attendee> Attendees) iEvent,
            List<string> participantAddressesToUpdate);

    }
}

