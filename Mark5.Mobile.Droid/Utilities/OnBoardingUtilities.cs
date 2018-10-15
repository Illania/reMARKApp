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

            var content = new List<OnBoardingPageModel>
            {
                new OnBoardingPageModel("What's new", "We have made a few changes in the MARK5 app. Press next to see what has happened", Resource.Drawable.onboarding_1),
                new OnBoardingPageModel("Category flow", "We have made it easier to assign categories. " +
                                        "Tap the categories you wish to assign from “select categories” and they will appear in the top under “categories added”. " +
                                        "To remove them from “categories added” just tap them again and they will reappear under “select categories”. " +
                                        "Click save when you are done.", Resource.Drawable.onboarding_2),
                new OnBoardingPageModel("Login details are saved", "From now on MARK5 saves your login details when you log out of the app. " +
                                        "This means that you only need to type in your password when you want to login again.",Resource.Drawable.onboarding_3),
            };

            viewPager.Adapter = new OnBoardingPageAdapter(context, content);

            viewPager.Id = View.GenerateViewId();
            viewPager.SetCurrentItem(0, false);

            linearLayoout.AddView(viewPager);

            return linearLayoout;
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

                linearLayout.SetBackgroundColor(new Color(ContextCompat.GetColor(context, Resource.Color.lightblue)));

                var paddingValue = Conversion.ConvertDpToPixels(15);

                var imageView = new AppCompatImageView(context);
                var ip = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = (int)GravityFlags.Center
                };
                imageView.LayoutParameters = ip;
                imageView.SetAdjustViewBounds(true);
                imageView.SetScaleType(ImageView.ScaleType.FitStart);
                imageView.SetImageResource(pageModel.ImageResourceId);

                var displayMetrics = new DisplayMetrics();
                ((Activity)context).WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
                int height = context.Resources.Configuration.Orientation == Android.Content.Res.Orientation.Portrait
                    ? (int)(displayMetrics.HeightPixels * 0.50)
                    : (int)(displayMetrics.HeightPixels * 0.20);
                imageView.SetMaxHeight(height);

                imageView.SetPadding(0, paddingValue * 3, 0, 0);

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
                contentTextView.JustificationMode = Android.Text.JustificationMode.InterWord;
                contentTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
                contentTextView.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.darkblue)));
                contentTextView.SetPadding(paddingValue, paddingValue, paddingValue, paddingValue);

                var bottomRowLayout = new RelativeLayout(context);
                var lp = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                lp.SetMargins(0, 0, 0, paddingValue);
                bottomRowLayout.LayoutParameters = lp;

                if (position != Count - 1)
                {
                    var skipButton = new AppCompatButton(context)
                    {
                        Text = "Close",
                        Id = View.GenerateViewId(),
                    };
                    skipButton.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.darkblue)));
                    skipButton.SetBackgroundColor(Color.Transparent);
                    var sp = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                    sp.AddRule(LayoutRules.CenterVertical);
                    sp.AddRule(LayoutRules.AlignParentLeft);
                    skipButton.LayoutParameters = sp;
                    skipButton.Click += (object sender, EventArgs e) => Close();

                    bottomRowLayout.AddView(skipButton);
                }

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

                linearLayout.AddView(imageView);
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