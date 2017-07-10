using System;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Text;
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
                LeftMargin = DistanceNormal,
                Gravity = (int)GravityFlags.Top,
            };
            expandButton.Click += ExpandButton_Click;
            expandButton.LayoutParameters = addButtonLp;
            AddView(expandButton);

            compositeNameEditText = new AppCompatEditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                InputType = InputTypes.TextVariationPersonName | InputTypes.TextFlagCapSentences,
            };
            compositeNameEditText.SetHint(Resource.String.edit_contact_name);
            compositeNameEditText.AfterTextChanged += CompositeNameEditText_TextChanged;
            compositeNameEditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            namesLayout.AddView(compositeNameEditText);

            expandedLayout = new LinearLayoutCompat(context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            expandedLayout.Visibility = ViewStates.Gone;
            namesLayout.AddView(expandedLayout);

            //TODO doesn't get capitalied

            firstNameEditText = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                InputType = InputTypes.TextVariationPersonName | InputTypes.TextFlagCapSentences,
            };
            firstNameEditText.SetHint(Resource.String.edit_contact_first_name);
            firstNameEditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            firstNameEditText.AfterTextChanged += FirstNameEditText_TextChanged;
            expandedLayout.AddView(firstNameEditText);

            middleNameEditText = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                InputType = InputTypes.TextVariationPersonName | InputTypes.TextFlagCapSentences,
            };
            middleNameEditText.SetHint(Resource.String.edit_contact_middle_name);
            middleNameEditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            middleNameEditText.AfterTextChanged += MiddleNameEditText_TextChanged;
            expandedLayout.AddView(middleNameEditText);

            lastNameEditText = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                InputType = InputTypes.TextVariationPersonName | InputTypes.TextFlagCapSentences,
            };
            lastNameEditText.SetHint(Resource.String.edit_contact_last_name);
            lastNameEditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            lastNameEditText.AfterTextChanged += LastNameEditText_TextChanged;
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

        void CompositeNameEditText_TextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            if (!compositeNameEditText.HasFocus)
                return;

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

        void FirstNameEditText_TextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            if (!firstNameEditText.HasFocus)
                return;

            Contact.FirstName = firstNameEditText.Text;
            UpdateCompositeName();

            firstNameEditText.Error = string.IsNullOrEmpty(Contact.FirstName)
                ? Context.GetString(Resource.String.edit_contact_first_name_error) : null;
        }

        void MiddleNameEditText_TextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            if (!middleNameEditText.HasFocus)
                return;

            Contact.Patronymic = middleNameEditText.Text;
            UpdateCompositeName();
        }

        void LastNameEditText_TextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            if (!lastNameEditText.HasFocus)
                return;

            Contact.LastName = lastNameEditText.Text;
            UpdateCompositeName();

            lastNameEditText.Error = string.IsNullOrEmpty(Contact.LastName)
                ? Context.GetString(Resource.String.edit_contact_last_name_error) : null;
        }

        public override void RefreshView()
        {
            UpdateSingleNames();
            UpdateCompositeName();
            UpdateErrors();
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

            compositeNameEditText.Error = ContainsValidContent() ? null
                : Context.GetString(Resource.String.edit_contact_composite_name_error);
        }

        void UpdateErrors()
        {
            firstNameEditText.Error = string.IsNullOrEmpty(Contact.FirstName)
                ? Context.GetString(Resource.String.edit_contact_first_name_error) : null;

            lastNameEditText.Error = string.IsNullOrEmpty(Contact.LastName)
                ? Context.GetString(Resource.String.edit_contact_last_name_error) : null;

            compositeNameEditText.Error = ContainsValidContent() ? null
                : Context.GetString(Resource.String.edit_contact_composite_name_error);
        }

        public override bool ContainsValidContent()
        {
            return !string.IsNullOrEmpty(Contact.FirstName) && !string.IsNullOrEmpty(Contact.LastName);
        }
    }
}
