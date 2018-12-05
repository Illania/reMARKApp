using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class ModuleNavigationController : UIViewController
    {
        UIButton closeButton;
        UIView searchButtonContainer;
        NavigationModule.NavigationModuleType currentModule;

        WeakReference<AbstractMainViewController.IAbstractMainViewControllerDelegate> weakReferenceAbstractMainViewControllerDelegate;

        public AbstractMainViewController.IAbstractMainViewControllerDelegate AbstractMainViewControllerDelegate
        {
            get
            {
                return weakReferenceAbstractMainViewControllerDelegate.TryGetTarget(out AbstractMainViewController.IAbstractMainViewControllerDelegate documentPageViewControllerDelegate) ? documentPageViewControllerDelegate : null;
            }

            set
            {
                weakReferenceAbstractMainViewControllerDelegate = new WeakReference<AbstractMainViewController.IAbstractMainViewControllerDelegate>(value);
            }
        }

        public ModuleNavigationController(NavigationModule.NavigationModuleType currentModule)
        {
            this.currentModule = currentModule;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            searchButtonContainer = new TouchTransparentView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            var del = UIApplication.SharedApplication?.Delegate as AppDelegate;
            var root = del?.Window?.RootViewController;

            UILabel title = new UILabel
            {
                Font = Theme.DefaultBoldFont,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "Choose",
                TextAlignment = UITextAlignment.Center
            };

            View.AddSubview(title);
            View.AddConstraints(new[]
            {
                title.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                title.TopAnchor.ConstraintEqualTo(View.TopAnchor, 40f)
            });

            View.AddSubview(searchButtonContainer);
            View.AddConstraints(new[]
            {
                searchButtonContainer.HeightAnchor.ConstraintEqualTo(65f),
                searchButtonContainer.WidthAnchor.ConstraintEqualTo(65f),
                searchButtonContainer.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                searchButtonContainer.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.BottomAnchor : BottomLayoutGuide.GetTopAnchor(), 2),
            });

            closeButton = new UIButton
            {
                TintColor = Theme.White,
                BackgroundColor = Theme.DarkBlue,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                ContentEdgeInsets = new UIEdgeInsets(14f, 14f, 14f, 14f)
            };

            closeButton.SetImage(UIImage.FromBundle("Failed").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            closeButton.Layer.BorderColor = Theme.DarkGray.CGColor;
            closeButton.Layer.BorderWidth = .7f;
            closeButton.Layer.CornerRadius = 27.5f;
            searchButtonContainer.AddSubview(closeButton);
            searchButtonContainer.AddConstraints(new[]
            {
                closeButton.HeightAnchor.ConstraintEqualTo(55f),
                closeButton.WidthAnchor.ConstraintEqualTo(55f),
                closeButton.CenterXAnchor.ConstraintEqualTo(searchButtonContainer.CenterXAnchor),
                closeButton.BottomAnchor.ConstraintEqualTo(searchButtonContainer.BottomAnchor, -10f),
            });

            closeButton.TouchUpInside += (object sender, EventArgs e) => { DismissViewController(false, null); };

            var seperator = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.DarkBlue,
            };

            seperator.Layer.CornerRadius = 1.5f;

            View.AddSubview(seperator);
            View.AddConstraints(new[]
            {
                seperator.HeightAnchor.ConstraintEqualTo(3f),
                seperator.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                seperator.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
                seperator.LeftAnchor.ConstraintEqualTo(View.LeftAnchor, 20f),
                seperator.RightAnchor.ConstraintEqualTo(View.RightAnchor, -20f)
            });

            List<NavigationModule> topModules = new List<NavigationModule>{
                new NavigationModule(NavigationModule.NavigationModuleType.Mail),
                new NavigationModule(NavigationModule.NavigationModuleType.Shortcodes),
                new NavigationModule(NavigationModule.NavigationModuleType.Contacts),
                new NavigationModule(NavigationModule.NavigationModuleType.Search)
            };

            List<NavigationModule> bottomModules = new List<NavigationModule>{
                new NavigationModule(NavigationModule.NavigationModuleType.Settings),
            };

            BuildStacks(topModules);
            BuildStacks(bottomModules, true);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            View.BackgroundColor = Theme.White;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
        }

        void BuildStacks(List<NavigationModule> modules, bool bottom = false)
        {

            List<UIStackView> stackViews = BuildStackViews(modules, bottom);

            var dummyCount = (int)Math.Ceiling((double)modules.Count / 3) * 3 - modules.Count;
            modules.AddRange(Enumerable.Repeat(0, dummyCount).Select(i => new NavigationModule(NavigationModule.NavigationModuleType.Dummy)).ToList());

            var row = 0;

            for (var i = 0; i < modules.Count; i++)
            {
                if (i > 0 && i % 3 == 0)
                {
                    row++;
                }

                bool isCurrent = modules[i].Type == currentModule;

                var btn = new ReMarkNavigationButton(modules[i])
                {
                    Selected = isCurrent,
                    OnClicked = BtnClicked
                };

                stackViews[row].AddArrangedSubview(btn);
            }

            View.AddSubviews(stackViews.ToArray());
        }

        void BtnClicked()
        {
            DismissViewController(false, null);
        }

        List<UIStackView> BuildStackViews(List<NavigationModule> modules, bool bottomNavigation = false)
        {
            var stackviewCount = Math.Ceiling((double)modules.Count / 3);
            List<UIStackView> stackViews = new List<UIStackView>();

            for (var i = 0; i < stackviewCount; i++)
            {
                stackViews.Add(new UIStackView
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    BackgroundColor = Theme.LightBlue,
                    Axis = UILayoutConstraintAxis.Horizontal,
                    Alignment = UIStackViewAlignment.Center,
                    Distribution = UIStackViewDistribution.FillEqually
                });
            }

            View.AddSubviews(stackViews.ToArray());

            List<NSLayoutConstraint> stackViewsConstraints = new List<NSLayoutConstraint>();

            for (var i = 0; i < stackViews.Count; i++)
            {
                stackViewsConstraints.Add(stackViews[i].CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor));
                stackViewsConstraints.Add(stackViews[i].LeftAnchor.ConstraintEqualTo(View.LeftAnchor, 20f));
                stackViewsConstraints.Add(stackViews[i].RightAnchor.ConstraintEqualTo(View.RightAnchor, -20f));

                //Attach last stackview to center Y of view
                if (i == stackViews.Count - 1)
                {
                    if (bottomNavigation)
                    {
                        stackViewsConstraints.Add(stackViews[i].TopAnchor.ConstraintEqualTo(View.CenterYAnchor, 20f));
                    }
                    else
                    {
                        stackViewsConstraints.Add(stackViews[i].BottomAnchor.ConstraintEqualTo(View.CenterYAnchor, -10f));
                    }
                }

                //Attach bottom to next stackviews top
                if (i != stackViews.Count - 1 && stackViews.Count > 1 && stackViews[i + 1] != null)
                {
                    stackViewsConstraints.Add(stackViews[i].BottomAnchor.ConstraintEqualTo(stackViews[i + 1].TopAnchor, -10f));
                }
            }

            View.AddConstraints(stackViewsConstraints.ToArray());

            return stackViews;
        }

        class ReMarkNavigationButton : UIView
        {
            public Action OnClicked;

            public bool Selected
            {
                set
                {
                    if (value)
                    {
                        Button.BackgroundColor = Theme.DarkBlue;
                        TintColor = Theme.White;
                    }
                    else
                    {
                        Button.BackgroundColor = Theme.White;
                        Button.TintColor = Theme.DarkBlue;
                    }
                }
            }

            readonly UIButton Button = new UIButton
            {
                TintColor = Theme.White,
                BackgroundColor = Theme.White,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                ContentEdgeInsets = new UIEdgeInsets(14f, 14f, 14f, 14f)
            };

            readonly UILabel Title = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                TextColor = Theme.DarkBlue,
                TextAlignment = UITextAlignment.Center,
                Font = Theme.DefaultLightFont
            };

            public ReMarkNavigationButton(NavigationModule module)
            {

                TranslatesAutoresizingMaskIntoConstraints = false;

                AddConstraints(new[]
                {
                    HeightAnchor.ConstraintEqualTo(95f),
                    WidthAnchor.ConstraintEqualTo(55f)
                });

                Button.SetImage(UIImage.FromBundle(module.Image).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                Button.Layer.BorderColor = Theme.DarkGray.CGColor;
                Button.Layer.BorderWidth = .7f;
                Button.Layer.CornerRadius = 32.5f;

                AddSubview(Button);
                AddConstraints(new[]
                {
                    Button.HeightAnchor.ConstraintEqualTo(65f),
                    Button.WidthAnchor.ConstraintEqualTo(65f),
                    Button.CenterXAnchor.ConstraintEqualTo(CenterXAnchor),
                    Button.TopAnchor.ConstraintEqualTo(TopAnchor),
                });

                Title.Text = module.Title;

                AddSubview(Title);
                AddConstraints(new[]
                {
                    Title.TopAnchor.ConstraintEqualTo(Button.BottomAnchor, 5f),
                    Title.CenterXAnchor.ConstraintEqualTo(CenterXAnchor),
                    Title.WidthAnchor.ConstraintEqualTo(WidthAnchor)
                });

                if (module.Type == NavigationModule.NavigationModuleType.Dummy)
                {
                    Button.Alpha = 0;
                    Title.Alpha = 0;
                }

                Button.TouchUpInside += (object sender, EventArgs e) =>
                {
                    OnClicked?.Invoke();
                    CommonConfig.MessengerHub.Publish(new ReMarkNav(this, module));
                };
            }
        }
    }
}
