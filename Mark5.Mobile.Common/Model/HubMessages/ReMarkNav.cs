using System;
using TinyMessenger;
using UIKit;

namespace Mark5.Mobile.Common.Model.HubMessages
{
    public class ReMarkNav : TinyMessageBase
    {
        public NavigationModule Module;

        public ReMarkNav(object sender, NavigationModule module )
            : base(sender)
        {
            Module = module;
        }
    }
}
