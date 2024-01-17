using TinyMessenger;

namespace reMark.Mobile.IOS.Model.HubMessages
{
    public class NavigationModuleChangedMessage : TinyMessageBase
    {
        public NavigationModule Module;

        public NavigationModuleChangedMessage(object sender, NavigationModule module)
            : base(sender)
        {
            Module = module;
        }
    }
}