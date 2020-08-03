using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V7.Preferences;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class OnBoardingUtilities
    {
        const string appVersionKey = "latestAppVersionKey";
        static AlertDialog dialog;
        static ViewPager viewPager;

        public static void ShowOnBoardingIfNecessary(Context context)
        {
            try
            {
                if (ApplicationHasBeenUpdated(context))
                {
                    SaveAppVersionName(context);

                    var view = CreateView(context);

                    dialog = new AlertDialog.Builder(context)
                       .SetView(view).Create();

                    dialog.Show();
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error trying to show onboarding.", ex);
                return;
            }
        }

        static bool ApplicationHasBeenUpdated(Context context)
        {
            var currentVersionCode = float.Parse(context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName);
            var storedVersionCode = PreferenceManager.GetDefaultSharedPreferences(context).GetFloat(appVersionKey, 0);

            return currentVersionCode > storedVersionCode;
        }

        static void SaveAppVersionName(Context context)
        {
            var currentVersionCode = float.Parse(context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName);
            var prefManager = PreferenceManager.GetDefaultSharedPreferences(context);
            var editor = prefManager.Edit();
            editor.PutFloat(appVersionKey, currentVersionCode);
            editor.Commit();
        }

        static View CreateView(Context context)
        {
            var linearLayoout = new LinearLayoutCompat(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };

            viewPager = new ViewPager(context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };

            var content = GetPageModels();

            viewPager.Adapter = new OnBoardingPageAdapter(context, content);

            viewPager.Id = View.GenerateViewId();
            viewPager.SetCurrentItem(0, false);

            linearLayoout.AddView(viewPager);

            return linearLayoout;
        }

        static List<OnBoardingPageModel> GetPageModels()
        {
            return new List<OnBoardingPageModel>
            {
                new OnBoardingPageModel("Welcome to reMARK", "We have made a few changes in the reMARK app. Press next to see what has happened.",
                     Resource.Drawable.onboarding_1),
                new OnBoardingPageModel("Select multiple files as an attachment", "It is now possible to select multiple files as an attachment when composing an email.",
                     Resource.Drawable.onboarding_2),
                new OnBoardingPageModel("Basic formatting", "Basic formatting (Bold/Italic/Underline) now available in email composing.",
                     Resource.Drawable.onboarding_3),
                new OnBoardingPageModel("Offline sync of read action", "Offline? Your read action will still be updated and then synced when you go back online.",
                     Resource.Drawable.onboarding_4),
                new OnBoardingPageModel("Autocomplete with mailing lists", "Now autocomplete will also suggests mailing lists when composing emails.",
                     Resource.Drawable.onboarding_5),
                new OnBoardingPageModel("Calendar invitations", "Now you can answer to calendar invitations in mails.",
                     Resource.Drawable.onboarding_6),
            };
        }

        static void GoToNextPage() => viewPager.SetCurrentItem(viewPager.CurrentItem + 1, true);

        static void Close() => dialog.Dismiss();

        class OnBoardingPageModel
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public int ImageResourceId { get; set; }

            public OnBoardingPageModel(string title, string content, int imageResourceId)
            {
                Title = title;
                Content = content;
                ImageResourceId = imageResourceId;
            }
        }

        class OnBoardingPageAdapter : PagerAdapter
        {
            public override int Count => pageModels.Count;

            readonly Context context;
            List<OnBoardingPageModel> pageModels;

            public OnBoardingPageAdapter(Context context, List<OnBoardingPageModel> pageModels)
            {
                this.context = context;
                this.pageModels = pageModels;
            }

            public override bool IsViewFromObject(View view, Java.Lang.Object @object)
            {
                return view == @object;
            }

            public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
            {
                var pageModel = pageModels[position];

                var linearLayout = new LinearLayoutCompat(context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent),
                    Orientation = LinearLayoutCompat.Vertical,
                };

                linearLayout.SetBackgroundColor(new Color(ContextCompat.GetColor(context, Resource.Color.lightgray)));

                View topView = null;

                var paddingValue = Conversion.ConvertDpToPixels(15);

                var displayMetrics = new DisplayMetrics();
                ((Activity)context).WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
                int maxHeight = context.Resources.Configuration.Orientation == Android.Content.Res.Orientation.Portrait
                    ? (int)(displayMetrics.HeightPixels * 0.50)
                    : (int)(displayMetrics.HeightPixels * 0.20);

                if (position == 0)
                {
                    var animationView = new LottieAnimationView(context);
                    animationView.SetAnimation("splash.json");

                    var ip = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                    {
                        Gravity = (int)GravityFlags.Center
                    };
                    animationView.LayoutParameters = ip;
                    animationView.SetAdjustViewBounds(true);
                    animationView.SetScaleType(ImageView.ScaleType.FitStart);
                    animationView.SetMaxHeight(maxHeight);
                    animationView.PlayAnimation();

                    topView = animationView;
                }
                else
                {
                    var imageView = new AppCompatImageView(context);
                    var ip = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                    {
                        Gravity = (int)GravityFlags.Center
                    };
                    imageView.LayoutParameters = ip;
                    imageView.SetAdjustViewBounds(true);
                    imageView.SetScaleType(ImageView.ScaleType.FitStart);
                    imageView.SetImageResource(pageModel.ImageResourceId);

                    imageView.SetMaxHeight(maxHeight);

                    topView = imageView;
                }

                topView.SetPadding(0, paddingValue * 3, 0, 0);

                var titleTextView = new AppCompatTextView(context)
                {
                    Text = pageModel.Title,
                    LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                    TextAlignment = TextAlignment.Center,
                    Gravity = GravityFlags.Center,
                };
                titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontLargeBold);
                titleTextView.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.darkblue)));
                titleTextView.SetPadding(paddingValue, paddingValue, paddingValue, 0);

                var contentTextView = new AppCompatTextView(context)
                {
                    Text = pageModel.Content,
                    LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0, 1),
                    TextAlignment = TextAlignment.Center,
                    Gravity = GravityFlags.Top,
                    MovementMethod = Android.Text.Method.ScrollingMovementMethod.Instance,
                };
                contentTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
                contentTextView.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.darkblue)));
                contentTextView.SetPadding(paddingValue, paddingValue, paddingValue, paddingValue);

                var bottomRowLayout = new RelativeLayout(context);
                var lp = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                lp.SetMargins(0, 0, 0, paddingValue);
                bottomRowLayout.LayoutParameters = lp;

                var nextDoneButton = new AppCompatButton(context)
                {
                    Text = (position != Count - 1) ? "NEXT" : "DONE",
                    Id = View.GenerateViewId(),
                };
                nextDoneButton.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.white)));
                nextDoneButton.SetBackgroundColor(new Color(ContextCompat.GetColor(context, Resource.Color.darkblue)));
                var np = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                np.AddRule(LayoutRules.CenterInParent);
                nextDoneButton.Click += (object sender, EventArgs e) =>
                {
                    if (position != Count - 1)
                        GoToNextPage();
                    else
                        Close();
                };
                nextDoneButton.LayoutParameters = np;

                bottomRowLayout.AddView(nextDoneButton);

                linearLayout.AddView(topView);
                linearLayout.AddView(titleTextView);
                linearLayout.AddView(contentTextView);
                linearLayout.AddView(bottomRowLayout);

                container.AddView(linearLayout);

                return linearLayout;
            }

            public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object @object)
            {
                container.RemoveView((View)@object);
            }
        }
    }
}