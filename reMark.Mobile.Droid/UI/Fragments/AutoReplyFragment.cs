using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.Widget;
using Google.Android.Material.FloatingActionButton;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Model.Exceptions;
using reMark.Mobile.Common.Storage.AppFileStorage.Interface;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Common.Utilities.Extensions;
using reMark.Mobile.Droid.Model;
using reMark.Mobile.Droid.Ui.Activities;
using reMark.Mobile.Droid.Ui.Common;
using reMark.Mobile.Droid.Ui.Views.AutoReplyViews;
using reMark.Mobile.Droid.Ui.Views.Common;
using reMark.Mobile.Droid.Ui.Views.ComposeDocumentViews;
using reMark.Mobile.Droid.Utilities;
using ContentView = reMark.Mobile.Droid.Ui.Views.AutoReplyViews.ContentView;
using FormattingView = reMark.Mobile.Droid.Ui.Views.AutoReplyViews.FormattingView;
using LineView = reMark.Mobile.Droid.Ui.Views.AutoReplyViews.LineView;
using ProgressBar = Android.Widget.ProgressBar;
using Rect = Android.Graphics.Rect;
using SubjectView = reMark.Mobile.Droid.Ui.Views.AutoReplyViews.SubjectView;
using Uri = Android.Net.Uri;
using View = Android.Views.View;

namespace reMark.Mobile.Droid.Ui.Fragments
{
    public class AutoReplyFragment : BaseFragment
    {
        readonly List<AutoReplySubView> subViews = new List<AutoReplySubView>(10);

        const string AutoReplyBundleKey = "AutoReply_a6c252fc-09b9-44a9-941f-ea3785c098978";

        AutoReplyRule autoReplyRule = new AutoReplyRule();

        bool documentLoaded;

        ProgressBar progress;
        NestedScrollView scrollView;
        LinearLayoutCompat linearLayout;

        View rootView;
        IsActiveView isActiveView;
        StartDateView startDateView;
        EndDateView endDateView;
        LineView lineView;
        SubjectView subjectView;
        ContentView contentView;
        FormattingView formattingView;

        FloatingActionButton fab;

        Rect visibleRect = new Rect();

        Action dismissAction;

        (int pickedHours, int pickedMinutes) LastPickedUserSendingDelay = (0,0);

        public static (AutoReplyFragment fragment, string tag) NewInstance(AutoReplyRule autoReplyRule)
        {
            var args = new Bundle();

            if (autoReplyRule != null)
                args.PutString(AutoReplyBundleKey, Serializer.Serialize(autoReplyRule));

            var fragment = new AutoReplyFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(AutoReplyFragment)} [autoReplyRule={autoReplyRule}]";
            return (fragment, tag);
        }

        #region Activity lifecycle

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(AutoReplyBundleKey))
                autoReplyRule = Serializer.Deserialize<AutoReplyRule>(Arguments.GetString(AutoReplyBundleKey));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);
            rootView.ViewTreeObserver.GlobalLayout += RootView_OnGlobalLayout;

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            progress.Visibility = ViewStates.Gone;
            scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            scrollView.Visibility = ViewStates.Visible;
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            var frame = rootView.FindViewById<FrameLayout>(Resource.Id.frame_layout);

            formattingView = new FormattingView(Context);
            frame.AddView(formattingView);

            isActiveView = new IsActiveView(Context);
            subViews.Add(isActiveView);

            startDateView = new StartDateView(Context);
            subViews.Add(startDateView);

            endDateView = new EndDateView(Context);
            subViews.Add(endDateView);

            lineView = new LineView(Context);
            lineView.Edited += Subview_Edited;
            subViews.Add(lineView);

            subjectView = new SubjectView(Context);
            subjectView.Edited += Subview_Edited;
            subViews.Add(subjectView);

            contentView = new ContentView(Context, formattingView, MoveViewToCaret, FormattingViewVisibilityChanged);
            subViews.Add(contentView);

            foreach (var subview in subViews)
            {
                linearLayout.AddView(subview);
                if (subview != contentView)
                    linearLayout.AddView(new Divider(Context));
            }


            fab = ((BaseAppCompatActivity)Activity).Fab;
            fab.SetImageResource(Resource.Drawable.action_save);
            fab.SetOnClickListener(new ActionOnClickListener(() => CheckDataIsValid(Save)));
            fab.Enabled = false;
            fab.Alpha = 0.6f;
            fab.Visibility = ViewStates.Visible;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnDestroyView()
        {
            fab.Visibility = ViewStates.Invisible;
            dismissAction?.Invoke();
            base.OnDestroyView();
            rootView.ViewTreeObserver.GlobalLayout -= RootView_OnGlobalLayout;
        }

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(AutoReplyFragment)}...");

            await LoadDocument();

            CommonConfig.Logger.Info($"Resumed {nameof(AutoReplyFragment)}...");
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Paused {nameof(ComposeDocumentFragment)}");
        }

        #endregion

        async Task LoadDocument()
        {
            if (documentLoaded)
                return;

            try
            {
                await ShowDocument();
                documentLoaded = true;

            }
            catch (Exception ex)
            {

                CommonConfig.Logger.Error("Failed to load autoreply rule", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                Activity?.OnBackPressed();
            }
        }

        async Task ShowDocument()
        {
            foreach (var subView in subViews)
            {
                subView.AutoReplyRule = autoReplyRule;
                await subView.RefreshView();
            }

            UpdateSaveButtonState();
        }

 

        #region Subviews event handlers
        void Subview_Edited(object sender, EventArgs e)
        {
            UpdateSaveButtonState();
        }


        #endregion

        #region Scrolling related

        void RootView_OnGlobalLayout(object sender, EventArgs e)
        {
            if (View == null)
                return;

            int[] windowCoordinates = new int[2];
            View.GetLocationOnScreen(windowCoordinates);

            visibleRect.Top = windowCoordinates[1];
            visibleRect.Bottom = windowCoordinates[1] + View.Height;
            visibleRect.Left = windowCoordinates[0];
            visibleRect.Right = windowCoordinates[0] + View.Width;

        }

        void MoveViewToCaret(View webView, int relativeCaretPositionDp)
        {
            if (relativeCaretPositionDp <= 0)
                return;

            int[] webViewWindowLocation = new int[2];

            webView.GetLocationOnScreen(webViewWindowLocation);
            var webViewYPosition = webViewWindowLocation[1];

            var relativeCaretPositionPx = Conversion.ConvertDpToPixels(relativeCaretPositionDp);

            var absoluteCaretPositionTop = relativeCaretPositionPx + webViewYPosition - 10; //Added a little bit of padding
            var caretSize = 150;
            var absoluteCaretPositionBottom = absoluteCaretPositionTop + caretSize;

            int delta = 0;

            if (absoluteCaretPositionBottom > visibleRect.Bottom)
                delta = absoluteCaretPositionBottom - visibleRect.Bottom;
            else if (absoluteCaretPositionTop < visibleRect.Top)
                delta = absoluteCaretPositionTop - visibleRect.Top;

            if (delta != 0)
                Activity.RunOnUiThread(() => scrollView.ScrollBy(0, delta));
        }

        #endregion

        #region Actions
        public void AskIfShouldSave()
        {
              Dialogs.ShowYesNoDialog(Context, Resource.String.saving_autoreply, Resource.String.confirm_save_autoreply,
                   Save, ()=> Activity?.Finish(),cancelable: true);
        }

        void CheckDataIsValid(Action positiveAction)
        {
            var incorrectDates = autoReplyRule.ActiveTo < autoReplyRule.ActiveFrom;
            if (incorrectDates)
                Dialogs.ShowYesNoDialog(Context, Resource.String.invalid_date, Resource.String.invalid_date_message, positiveAction, () => fab.Enabled = true);
            else if (subjectView.Empty)
                Dialogs.ShowYesNoDialog(Context, Resource.String.invalid_subject_title, Resource.String.invalid_subject_content, positiveAction, () => fab.Enabled = true);
            else
                positiveAction();
        }

        async void Save()
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.saving_autoreply, Resource.String.please_wait);

            foreach (var subView in subViews)
                await subView.UpdateAutoReply();

            await Managers.DocumentsManager.SetAutoReplyRule(autoReplyRule);

            dismissAction();

            Activity?.Finish();
        }
        
        #endregion

        #region Options menu related

        void UpdateSaveButtonState()
        {
            var isFormValid = IsFormValid();

            fab.Enabled = isFormValid;
            fab.Alpha = isFormValid ? 1f : 0.6f;
        }

        bool IsFormValid()
        {
            var subjectEmpty  = subjectView.Empty;
            var incorrectDates = autoReplyRule.ActiveTo < autoReplyRule.ActiveFrom;

            return !subjectEmpty && !incorrectDates;
        }

        #endregion

        #region Formatting view related

        public void OnActionModeStarted()
        {
            formattingView.Visibility = ViewStates.Visible;
            FormattingViewVisibilityChanged();
        }

        public void OnActionModeFinished()
        {
            formattingView.Visibility = ViewStates.Gone;
            FormattingViewVisibilityChanged();
        }

        void FormattingViewVisibilityChanged()
        {
            var height = formattingView.Height;
            var delta = formattingView.Visibility == ViewStates.Visible ? height : -height;
            Activity.RunOnUiThread(() => scrollView.ScrollBy(0, delta));
        }

        #endregion
    }
}