using System;
using UIKit;
namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class CreateAppointmentViewController : UIViewController
    {
        public CreateAppointmentViewController()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "Create Appointment";

            InitializeNavigationBar();

            View.BackgroundColor = UIColor.White;
        }

        void InitializeNavigationBar()
        {

        }
    }
}
