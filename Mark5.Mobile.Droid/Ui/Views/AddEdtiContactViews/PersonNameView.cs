using System;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
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

        public PersonNameView(Context context)
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
            expandButton.SetImageResource(Resource.Drawable.expand_down);
            expandButton.SetColorFilter(new Color(ContextCompat.GetColor(context, Resource.Color.darkblue)));

            var typedArray = Context.ObtainStyledAttributes(new int[]
            {
                 Resource.Attribute.selectableItemBackgroundBorderless
            });
            expandButton.SetBackgroundResource(typedArray.GetResourceId(0, 0));

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
                InputType = InputTypes.TextVariationPersonName | InputTypes.TextFlagCapSentences | InputTypes.ClassText,
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

            firstNameEditText = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                InputType = InputTypes.TextVariationPersonName | InputTypes.TextFlagCapSentences | InputTypes.ClassText,
            };
            firstNameEditText.SetHint(Resource.String.edit_contact_first_name);
            firstNameEditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            firstNameEditText.AfterTextChanged += FirstNameEditText_TextChanged;
            expandedLayout.AddView(firstNameEditText);

            middleNameEditText = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                InputType = InputTypes.TextVariationPersonName | InputTypes.TextFlagCapSentences | InputTypes.ClassText,
            };
            middleNameEditText.SetHint(Resource.String.edit_contact_middle_name);
            middleNameEditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            middleNameEditText.AfterTextChanged += MiddleNameEditText_TextChanged;
            expandedLayout.AddView(middleNameEditText);

            lastNameEditText = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                InputType = InputTypes.TextVariationPersonName | InputTypes.TextFlagCapSentences | InputTypes.ClassText,
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

        void CompositeNameEditText_TextChanged(object sender, AfterTextChangedEventArgs e)
        {
            if (!compositeNameEditText.HasFocus) //To avoid loops while changing text programmatically
                return;

            ContactPreview.Name = compositeNameEditText.Text;

            var parts = compositeNameEditText.Text.Split(' ');
            if (parts.Any())
            {
                Contact.FirstName = parts.First();
                Contact.Patronymic = string.Empty;
                Contact.LastName = string.Join(" ", parts.Skip(1));
            }

            UpdateSingleNames();
        }

        void FirstNameEditText_TextChanged(object sender, AfterTextChangedEventArgs e)
        {
            if (!firstNameEditText.HasFocus)
                return;

            Contact.FirstName = firstNameEditText.Text;
            UpdateCompositeName();
        }

        void MiddleNameEditText_TextChanged(object sender, AfterTextChangedEventArgs e)
        {
            if (!middleNameEditText.HasFocus)
                return;

            Contact.Patronymic = middleNameEditText.Text;
            UpdateCompositeName();
        }

        void LastNameEditText_TextChanged(object sender, AfterTextChangedEventArgs e)
        {
            if (!lastNameEditText.HasFocus)
                return;

            Contact.LastName = lastNameEditText.Text;
            UpdateCompositeName();
        }

        public override void RefreshView()
        {
            UpdateSingleNames();
            UpdateCompositeName();
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
            ContactPreview.Name = compositeNameEditText.Text = sb.ToString();
        }

        public bool ContainsValidContent()
        {
            return !string.IsNullOrEmpty(Contact.FirstName) && !string.IsNullOrEmpty(Contact.LastName);
        }
    }
}
