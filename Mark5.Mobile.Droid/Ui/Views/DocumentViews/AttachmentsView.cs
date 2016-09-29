//
// Project: Mark5.Mobile.Droid
// File: AttachmentsView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{

    public class AttachmentsView : DocumentView
    {

        LinearLayoutCompat container;

        public AttachmentsView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            var scrollView = new HorizontalScrollView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                VerticalScrollBarEnabled = false,
                HorizontalScrollBarEnabled = false
            };
            AddView(scrollView);

            container = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            scrollView.AddView(container);
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null && Document != null && Document.Attachments.Count > 0)
            {
                Visibility = ViewStates.Visible;

                foreach (var ad in Document.Attachments)
                {
                    container.AddView(new AttachmentView(Context, ad));
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
                container.RemoveViews(0, container.ChildCount);
            }
        }

        class AttachmentView : LinearLayoutCompat
        {

            public AttachmentView(Context context, AttachmentDescription attachmentDescription)
                : base(context)
            {
                var minimumWidth = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 75.0f, Resources.DisplayMetrics) + 0.5f);
                var maximumWidth = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 150.0f, Resources.DisplayMetrics) + 0.5f);
                var margin = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 8.0f, Resources.DisplayMetrics) + 0.5f);
                var innerMargin = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 4.0f, Resources.DisplayMetrics) + 0.5f);

                Orientation = Vertical;

                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = margin,
                    TopMargin = margin,
                    RightMargin = margin,
                    BottomMargin = margin
                };
                Elevation = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 2.0f, Resources.DisplayMetrics) + 0.5f);
                SetBackgroundResource(Resource.Drawable.rounded_background);

                var title = new AppCompatTextView(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        BottomMargin = innerMargin
                    },
                    Text = attachmentDescription.Name.Split('.')[0],
                    Ellipsize = TextUtils.TruncateAt.End,
                    Gravity = GravityFlags.Top,
                };
                title.SetMinWidth(minimumWidth);
                title.SetMaxWidth(maximumWidth);
                title.SetSingleLine(true);
                if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    title.SetTextAppearance(Context, Resource.Style.fontPrimary);
#pragma warning restore CS0618 // Type or member is obsolete
                }
                else
                {
#pragma warning disable XA0001 // Find issues with Android API usage
                    title.SetTextAppearance(Resource.Style.fontPrimary);
#pragma warning restore XA0001 // Find issues with Android API usage
                }
                AddView(title);

                var innerLayout = new LinearLayoutCompat(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                };
                AddView(innerLayout);

                var extension = new AppCompatTextView(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        RightMargin = innerMargin,
                        Weight = 1
                    },
                    Text = attachmentDescription.Name.Split('.')[1].ToUpper(),
                    Gravity = GravityFlags.Start
                };
                extension.SetSingleLine(true);
                if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    extension.SetTextAppearance(Context, Resource.Style.fontButtonLink);
#pragma warning restore CS0618 // Type or member is obsolete
                }
                else
                {
#pragma warning disable XA0001 // Find issues with Android API usage
                    extension.SetTextAppearance(Resource.Style.fontButtonLink);
#pragma warning restore XA0001 // Find issues with Android API usage
                }
                innerLayout.AddView(extension);

                var size = new AppCompatTextView(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        Weight = 1
                    },
                    Text = Formatters.FormatFileSize(attachmentDescription.SizeInBytes),
                    Gravity = GravityFlags.End
                };
                size.SetSingleLine(true);
                if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    size.SetTextAppearance(Context, Resource.Style.fontSecondary);
#pragma warning restore CS0618 // Type or member is obsolete
                }
                else
                {
#pragma warning disable XA0001 // Find issues with Android API usage
                    size.SetTextAppearance(Resource.Style.fontSecondary);
#pragma warning restore XA0001 // Find issues with Android API usage
                }
                innerLayout.AddView(size);
            }
        }
    }
}
