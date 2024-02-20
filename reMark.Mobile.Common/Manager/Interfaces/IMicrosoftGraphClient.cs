using reMark.Mobile.Classes.Azure;
using Microsoft.Graph.Models;

namespace reMark.Mobile.Common.Manager
{
    public interface IMicrosoftGraphClient
    {
        public Task<AzureApplicationProxyInfo> GetAzureApplicationProxyInfo();

        public Task<string> Authenticate(object parentWindow, bool forceInteractive = true);

        public Task<AzureUser> GetAzureUser();

        public Task<List<AzureEndpointInfo>> GetAzureEndpointInfoList();

        public Task<Event> ImportFromICal((string Id, List<Common.Model.Attendee> Attendees) iEvent,
            List<string> participantAddressesToUpdate);
        
        public bool IsAuthenticated();

    }
}

