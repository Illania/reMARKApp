using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Lifecycle;
using Android.Animation;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views.InputMethods;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.Content;
using AndroidX.Core.Graphics.Drawable;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.AutoReplyViews
{
    public class IsActiveView: AutoReplySubView
    {
        SwitchCompat ToggleButton;
        Action toggleChanged = delegate { }; 
        public IsActiveView(Context context, Action toggleChanged):base(context)
        {
            this.toggleChanged = toggleChanged;

            var isActiveText = new BasicTextView(context);
            isActiveText.Text = "Active";
            isActiveText.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1)
            {
                Gravity = (GravityFlags)((int)GravityFlags.Left | (int)GravityFlags.CenterVertical),
            };

            AddView(isActiveText);

            ToggleButton = new SwitchCompat(context)
            {
                Gravity = GravityFlags.Right,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = (GravityFlags)((int)GravityFlags.Right | (int)GravityFlags.CenterVertical),
                    RightMargin = 0,
                },
                SwitchPadding = 0,
            };

            AddView(ToggleButton);
        }

        public override Task RefreshView()
        {
            ToggleButton.Checked = AutoReplyRule.Active;
            return Task.CompletedTask;
        }


        public override Task UpdateAutoReply()
        {
            AutoReplyRule.Active = ToggleButton.Checked;
            return Task.CompletedTask;
        }
    }
}





