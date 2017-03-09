// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
	[Register ("ContactsTableViewCell")]
	partial class ContactsTableViewCell
	{
		[Outlet]
		UIKit.UIView CategoriesView { get; set; }

		[Outlet]
		UIKit.UILabel DescriptionLabel { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint LeadingConstraint { get; set; }

		[Outlet]
		UIKit.UILabel NameLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LeadingConstraint != null) {
				LeadingConstraint.Dispose ();
				LeadingConstraint = null;
			}

			if (CategoriesView != null) {
				CategoriesView.Dispose ();
				CategoriesView = null;
			}

			if (DescriptionLabel != null) {
				DescriptionLabel.Dispose ();
				DescriptionLabel = null;
			}

			if (NameLabel != null) {
				NameLabel.Dispose ();
				NameLabel = null;
			}
		}
	}
}
