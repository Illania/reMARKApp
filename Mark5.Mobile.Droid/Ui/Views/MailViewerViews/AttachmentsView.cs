using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using MailBee.Mime;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.MailViewerViews
{
    public class AttachmentsView : MailViewerView
    {
        LinearLayoutCompat container;

        public event EventHandler<Attachment> AttachmentClicked = delegate { };
        public event EventHandler<Attachment> AttachmentLongClicked = delegate { };

        public AttachmentsView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            Orientation = Vertical;

            var title = new AppCompatTextView(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = DistanceLarge,
                    TopMargin = DistanceNormal,
                    RightMargin = DistanceLarge,
                    BottomMargin = DistanceNormal
                },
                Text = Context.GetString(Resource.String.attachments)
            };
            title.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);
            AddView(title);

            container = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Orientation = Vertical
            };
            AddView(container);
        }

        public override void RefreshView()
        {
            if (MailMessage != null && MailMessage.Attachments.Count > 0)
            {
                Visibility = ViewStates.Visible;

                container.RemoveViews(0, container.ChildCount);

                foreach (var att in MailMessage.Attachments.OfType<Attachment>())
                {
                    var av = new AttachmentView(Context, att, DistanceLarge, DistanceNormal);
                    av.Click += (sender, e) => AttachmentClicked(this, att);
                    av.LongClick += (sender, e) => AttachmentLongClicked(this, att);
                    container.AddView(av);
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
            public AttachmentView(Context context, Attachment att, int distanceLarge, int distanceNormal)
                : base(context)
            {
                var maximumWidth = ConversionUtils.ConvertDpToPixels(250f);
                var innerMargin = ConversionUtils.ConvertDpToPixels(4f);

                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = distanceLarge,
                    TopMargin = 0,
                    RightMargin = distanceLarge,
                    BottomMargin = distanceNormal
                };
                Elevation = ConversionUtils.ConvertDpToPixels(2f);

                SetBackgroundResource(Resource.Drawable.rounded_background);

                Clickable = true;

                var imageSize = ConversionUtils.ConvertDpToPixels(16f);
                var image = new AppCompatImageView(Context)
                {
                    LayoutParameters = new LayoutParams(imageSize, imageSize)
                    {
                        RightMargin = innerMargin,
                        Gravity = (int) GravityFlags.Center
                    }
                };
                image.SetImageResource(Resource.Drawable.attachment);
                image.SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                AddView(image);

                var title = new AppCompatTextView(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        Gravity = (int) GravityFlags.Center
                    },
                    Text = att.Name,
                    Ellipsize = TextUtils.TruncateAt.End,
                };
                title.SetMaxWidth(maximumWidth);
                title.SetSingleLine(true);
                title.SetTextAppearanceCompat(Context, Resource.Style.fontSmallLight);
                AddView(title);

                var size = new AppCompatTextView(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        Gravity = (int) GravityFlags.Center
                    },
                    Text = " (" + Formatters.FormatFileSize(att.Size) + ")",
                };
                size.SetSingleLine(true);
                size.SetTextAppearanceCompat(Context, Resource.Style.fontSmallLight);
                AddView(size);
            }
        }
    }
}