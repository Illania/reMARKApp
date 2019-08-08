using System;
namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public interface IAnimating
    {
        void ExpandCompressView();
        event EventHandler BeginAnimating;
        event EventHandler Animating;
        event EventHandler EndAnimating;
    }
}
