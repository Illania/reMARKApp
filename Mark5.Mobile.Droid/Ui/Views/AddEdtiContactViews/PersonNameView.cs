using System;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class PersonNameView : AddEditContactView
    {
        readonly AppCompatEditText compositeNameEditText;
        readonly AppCompatEditText firstNameEditText;
        readonly AppCompatEditText middleNameEditText;
        readonly AppCompatEditText lastNameEditText;

        readonly LinearLayoutCompat expandedLayout;
        readonly LinearLayoutCompat namesLayout;

        readonly AppCompatImageButton expandButton;
        readonly Action<string> onPersonNameChanged;

        public PersonNameView(Context context, Action<string> onPersonNameChanged)
            : base(context)
        {
            this.onPersonNameChanged = onPersonNameChanged;

            Orientation = Horizontal;

            namesLayout = new LinearLayoutCompat(context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f),
            };
            AddView(namesLayout);

            expandButton = new AppCompatImageButton(context);

            expandButton.SetImageResource(Resource.Drawable.expand_down);

            var addButtonLp = new LayoutParams(ConversionUtils.ConvertDpToPixels(24), ConversionUtils.ConvertDpToPixels(24))
            {
                TopMargin = DistanceSmall,
                LeftMargin = DistanceSmall,
                Gravity = (int)GravityFlags.Top,
            };
            expandButton.Click += ExpandButton_Click;
            expandButton.LayoutParameters = addButtonLp;
            AddView(expandButton);

            compositeNameEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                InputType = Android.Text.InputTypes.TextFlagNoSuggestions,
            };
            compositeNameEditText.SetHint(Resource.String.edit_contact_name);
            compositeNameEditText.TextChanged += CompositeNameEditText_TextChanged;
            compositeNameEditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
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
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                InputType = Android.Text.InputTypes.TextFlagNoSuggestions,
            };
            firstNameEditText.SetHint(Resource.String.edit_contact_first_name);
            firstNameEditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            firstNameEditText.TextChanged += FirstNameEditText_TextChanged;
            expandedLayout.AddView(firstNameEditText);

            middleNameEditText = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                InputType = Android.Text.InputTypes.TextFlagNoSuggestions,
            };
            middleNameEditText.SetHint(Resource.String.edit_contact_middle_name);
            middleNameEditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            middleNameEditText.TextChanged += MiddleNameEditText_TextChanged;
            expandedLayout.AddView(middleNameEditText);

            lastNameEditText = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                InputType = Android.Text.InputTypes.TextFlagNoSuggestions,
            };
            lastNameEditText.SetHint(Resource.String.edit_contact_last_name);
            lastNameEditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            lastNameEditText.TextChanged += LastNameEditText_TextChanged;
            expandedLayout.AddView(lastNameEditText);
        }

        void ExpandButton_Click(object sender, EventArgs e)
        {
            if (expandedLayout.Visibility == ViewStates.Gone)
            {
                expandedLayout.Visibility = ViewStates.Visible;
                compositeNameEditText.Visibility = ViewStates.Gone;
                expandButton.Animate().Rotation(180).SetInterpolator(new AccelerateDecelerateInterpolator());
            }
            else
            {
                expandedLayout.Visibility = ViewStates.Gone;
                compositeNameEditText.Visibility = ViewStates.Visible;
                expandButton.Animate().Rotation(0).SetInterpolator(new AccelerateDecelerateInterpolator());
                UpdateCompositeName();
            }
        }

        void CompositeNameEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            var parts = compositeNameEditText.Text.Split(' ');
            if (parts.Any())
            {
                Contact.FirstName = parts.First();
                Contact.Patronymic = string.Empty;
                Contact.LastName = string.Join(" ", parts.Skip(1));
            }

            UpdateSingleNames();
            onPersonNameChanged?.Invoke(compositeNameEditText.Text);
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
            UpdateSingleNames();
            UpdateCompositeName();
            onPersonNameChanged?.Invoke(compositeNameEditText.Text);
        }

        void UpdateSingleNames()
        {
            firstNameEditText.Text = Contact.FirstName;
            middleNameEditText.Text = Contact.Patronymic;
            lastNameEditText.Text = Contact.LastName;
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
            onPersonNameChanged?.Invoke(compositeNameEditText.Text);
        }
    }
}
