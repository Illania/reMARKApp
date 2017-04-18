//
// Project: Mark5.Mobile.Droid
// File: SubjectView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.MailViewerViews
{

    public class SubjectView : MailViewerView
	{

		AppCompatTextView subjectTextView;

		public SubjectView(Context context)
			: base(context)
		{
			InitializeView();
		}

		void InitializeView()
		{
			SetPadding(DistanceLarge, DistanceLarge, DistanceLarge, DistanceNormal);

            subjectTextView = new AppCompatTextView(Context)
			{
				LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
			};
			subjectTextView.SetTextAppearanceCompat(Context, Resource.Style.fontTitle);

			AddView(subjectTextView);
		}

		public override void RefreshView()
		{
            if (MailMessage != null)
			{
				Visibility = ViewStates.Visible;
                subjectTextView.Text = string.IsNullOrWhiteSpace(MailMessage.Subject) ? Context.GetString(Resource.String.no_subject) : MailMessage.Subject;
			}
			else
			{
				Visibility = ViewStates.Gone;
				subjectTextView.Text = string.Empty;
			}
		}
	}
}
