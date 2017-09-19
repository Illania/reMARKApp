using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ShortcodeViews
{
    public class AddressesView : ShortcodeView
    {
        readonly DocumentAddressType type;
        LinearLayoutCompat AddressesLayout;

        public event EventHandler<DocumentAddress> DocumentAddressClicked = delegate { };

        public AddressesView(Context context, DocumentAddressType type)
            : base(context)
        {
            this.type = type;

            InitializeView();
        }

        void InitializeView()
        {
            var titleView = new AppCompatTextView(Context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    BottomMargin = DistanceSmall
                },
                Text = type.ToString()
            };

            titleView.SetTextAppearanceCompat(Context, Resource.Style.fontLarge);
            titleView.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            titleView.SetPadding(DistanceVeryLarge, 0, DistanceNormal, 0);
            InnerLayout.AddView(titleView);

            AddressesLayout = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Orientation = LinearLayoutCompat.Vertical
            };

            InnerLayout.AddView(AddressesLayout);
        }

        public override void RefreshView()
        {
            if (ShortcodePreview != null && Shortcode != null && Shortcode.Addresses.Any(da => da.Type == CommunicationAddressType.Email && da.AddressType == type))
            {
                Visibility = ViewStates.Visible;

                AddressesLayout.RemoveAllViews();
                var addresses = Shortcode.Addresses.Where(da => da.Type == CommunicationAddressType.Email && da.AddressType == type).ToArray();
                foreach (var address in addresses)
                {
                    var av = new AddressView(Context, address, DistanceVeryLarge, DistanceNormal);
                    av.Click += (sender, e) => DocumentAddressClicked(this, address);
                    AddressesLayout.AddView(av);
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        class AddressView : LinearLayoutCompat
        {
            public AddressView(Context context, DocumentAddress address, int distanceVeryLarge, int distanceNormal)
                : base(context)
            {
                var typedArray = Context.ObtainStyledAttributes(new int[]
                {
                    Resource.Attribute.selectableItemBackground
                });
                SetBackgroundResource(typedArray.GetResourceId(0, 0));
                typedArray.Recycle();

                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                Orientation = Vertical;
                SetPadding(distanceVeryLarge, distanceNormal, distanceVeryLarge, distanceNormal);

                Clickable = true;

                var addressView = new AppCompatTextView(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                    Text = string.IsNullOrEmpty(address.Name) ? address.Address : $"{address.Name} <{address.Address}>"
                };

                addressView.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
                AddView(addressView);

                if (!string.IsNullOrWhiteSpace(address.FullAttention))
                {
                    var nameView = new AppCompatTextView(Context)
                    {
                        LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                        Text = address.FullAttention
                    };

                    nameView.SetTextAppearanceCompat(Context, Resource.Style.fontSmallLight);
                    AddView(nameView);
                }

                LongClickable = true;
                LongClick += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(addressView.Text))
                        Integration.CopyToClipboard(context, addressView.Text);
                };
            }
        }
    }
}