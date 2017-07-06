using System;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class PersonName : AddEditContactView
    {
        AppCompatEditText compositeNameEditText;
        AppCompatEditText firstNameEditText;
        AppCompatEditText middleNameEditText;
        AppCompatEditText lastNameEditText;

        LinearLayoutCompat expandedLayout;
        LinearLayoutCompat namesLayout;

        AppCompatImageButton expandButton;

        public PersonName(Context context)
            : base(context)
        {
            Orientation = Horizontal;

            namesLayout = new LinearLayoutCompat(context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f),
            };
            AddView(namesLayout);

            expandButton = new AppCompatImageButton(context);

            expandButton.SetImageResource(Resource.Drawable.add);
            expandButton.SetColorFilter(Color.AliceBlue);

            var addButtonLp = new LayoutParams(ConversionUtils.ConvertDpToPixels(24), ConversionUtils.ConvertDpToPixels(24))
            {
                LeftMargin = DistanceSmall,
                RightMargin = DistanceLarge,
                Gravity = (int)GravityFlags.Top,
            };
            expandButton.Click += ExpandButton_Click;
            expandButton.LayoutParameters = addButtonLp;
            AddView(expandButton);

            compositeNameEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            compositeNameEditText.SetHint(Resource.String.edit_contact_name);
            compositeNameEditText.TextChanged += CompositeNameEditText_TextChanged;
            namesLayout.AddView(compositeNameEditText);

            expandedLayout = new LinearLayoutCompat(context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            expandedLayout.Visibility = ViewStates.Gone;
            namesLayout.AddView(expandedLayout);

            firstNameEditText = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            firstNameEditText.SetHint(Resource.String.edit_contact_first_name);
            firstNameEditText.TextChanged += FirstNameEditText_TextChanged;
            expandedLayout.AddView(firstNameEditText);

            middleNameEditText = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            middleNameEditText.SetHint(Resource.String.edit_contact_middle_name);
            middleNameEditText.TextChanged += MiddleNameEditText_TextChanged; ;
            expandedLayout.AddView(middleNameEditText);

            lastNameEditText = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            lastNameEditText.SetHint(Resource.String.edit_contact_last_name);
            lastNameEditText.TextChanged += LastNameEditText_TextChanged; ;
            expandedLayout.AddView(lastNameEditText);
        }

        void ExpandButton_Click(object sender, EventArgs e)
        {
            if (expandedLayout.Visibility == ViewStates.Gone)
            {
                expandedLayout.Visibility = ViewStates.Visible;
                compositeNameEditText.Visibility = ViewStates.Gone;
                expandButton.SetColorFilter(Color.Red);

            }
            else
            {
                expandedLayout.Visibility = ViewStates.Gone;
                compositeNameEditText.Visibility = ViewStates.Visible;
                expandButton.SetColorFilter(Color.AliceBlue);
            }
        }

        void CompositeNameEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            var parts = compositeNameEditText.Text.Split(' ');
            if (parts.Any())
            {
                Contact.FirstName = parts.First();
                Contact.LastName = string.Join(" ", parts.Skip(1));
            }
        }

        void FirstNameEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            Contact.FirstName = firstNameEditText.Text;
        }

        void MiddleNameEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            Contact.Patronymic = middleNameEditText.Text;
        }

        void LastNameEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            Contact.LastName = lastNameEditText.Text;
        }

        public override void RefreshView()
        {
            firstNameEditText.Text = Contact.FirstName;
            middleNameEditText.Text = Contact.Patronymic;
            lastNameEditText.Text = Contact.LastName;

            UpdateCompositeName();
        }

        void UpdateCompositeName()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(Contact.FirstName))
                sb.Append(Contact.FirstName);
            if (!string.IsNullOrWhiteSpace(Contact.Patronymic))
                sb.Append(" " + Contact.Patronymic);
            if (!string.IsNullOrWhiteSpace(Contact.LastName))
                sb.Append(" " + Contact.LastName);
            compositeNameEditText.Text = sb.ToString();
        }
    }
}
