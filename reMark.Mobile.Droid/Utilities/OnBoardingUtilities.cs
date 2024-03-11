using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.Preference;
using AndroidX.ViewPager.Widget;
//using Com.Airbnb.Lottie;
using reMark.Mobile.Common;
using reMark.Mobile.Droid.Ui.Common;
using Color = Android.Graphics.Color;
using View = Android.Views.View;

namespace reMark.Mobile.Droid.Utilities
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
            var currentVersionCode = context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName;
            var storedVersionCode = "0.0.0";

            try
            {
                storedVersionCode = PreferenceManager.GetDefaultSharedPreferences(context).GetString(appVersionKey, "0.0.0");
            }
            catch(Java.Lang.ClassCastException){}

            return new Version(currentVersionCode) > new Version(storedVersionCode);
        }

        static void SaveAppVersionName(Context context)
        {
            var currentVersionCode = context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName;
            var prefManager = PreferenceManager.GetDefaultSharedPreferences(context);
            var editor = prefManager.Edit();
            editor.PutString(appVersionKey, currentVersionCode);
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
                new OnBoardingPageModel("Welcome to reMARK",
                "We have made a few changes in the reMARK app. Press next to see what has changed.",
                     Resource.Drawable.onboarding_1),

                new OnBoardingPageModel("Landscape orientatioon",
                "Now reMARK app supports also landscape screen orientation.",
                     Resource.Drawable.onboarding_2),

                new OnBoardingPageModel("Out of Office feature",
                "Now you can set up automatic Out of Office reply in app.",
                     Resource.Drawable.onboarding_3),

                new OnBoardingPageModel("Extended offline features",
                "Now you can assign categories, remove, delete and file to folder emails in offline mode.",
                     Resource.Drawable.onboarding_4),

                new OnBoardingPageModel("User activities",
                "Now you can user automatic user action workflows in app.",
                     Resource.Drawable.onboarding_5),

                new OnBoardingPageModel("Delivery report",
                "Now you can get a detail email transmit status info.",
                     Resource.Drawable.onboarding_6)

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
                  
                    /*var animationView = new LottieAnimationView(context);
                    animationView.SetAnimation("splash.json");

                    var ip = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                    {
                        Gravity = (GravityFlags)(int)GravityFlags.Center
                    };
                    animationView.LayoutParameters = ip;
                    animationView.SetAdjustViewBounds(true);
                    animationView.SetScaleType(ImageView.ScaleType.FitStart);
                    animationView.SetMaxHeight(maxHeight);
                    animationView.PlayAnimation();
                    topView = animationView;*/
                    
                    var animationView = new ImageView(context);
                    var ip = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                    {
                        Gravity = (GravityFlags)(int)GravityFlags.Center
                    };
                    animationView.SetImageResource(Resource.Drawable.appicon_gray);
                    animationView.LayoutParameters = ip;
                    animationView.SetAdjustViewBounds(true);
                    animationView.SetScaleType(ImageView.ScaleType.FitStart);
                    animationView.SetMaxHeight(maxHeight);
                    topView = animationView;
                    
                }
                else
                {
                    var imageView = new AppCompatImageView(context);
                    var ip = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                    {
                        Gravity = (GravityFlags)(int)GravityFlags.Center
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
                    TextAlignment = Android.Views.TextAlignment.Center,
                    Gravity = GravityFlags.Center,
                };
                titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontLargeBold);
                titleTextView.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.darkblue)));
                titleTextView.SetPadding(paddingValue, paddingValue, paddingValue, 0);

                var contentTextView = new AppCompatTextView(context)
                {
                    Text = pageModel.Content,
                    LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0, 1),
                    TextAlignment = Android.Views.TextAlignment.Center,
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