
using Android.App;
using Android.Content;
using Android.OS;
using Microsoft.Identity.Client;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Utilities;
using Android.Content.PM;
using reMark.Mobile.Droid.Ui.Common;
using System.Reflection.Metadata;
using reMark.Mobile.Common.Model;
using Document = reMark.Mobile.Common.Model.Document;
using Android.Graphics.Drawables;

namespace reMark.Mobile.Droid.Ui.Activities
{

    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize )]
    public class MsalAuthActivity: BaseAppCompatActivity
    {

        public const string StatusResultKey = "StatusResult_43dc8df1-dc88-4e39-81d6-59ea495c37ff";
        public const string ExceptionResultKey = "ExceptionResult_43dc8df1-dc88-4e39-81d6-59ea495c3578";
        
        public const string DocumentIntentKey = "Document_11dc8df1-dc88-4e39-81d6-59ea495c37ff";
        public const string DocumentPreviewIntentKey = "DocumentPreview_12dc8df1-dc88-4e39-81d6-59ea495c37ff";
        public const string CalendarInvitationIntentKey = "CalendarInvitation_13dc8df1-dc88-4e39-81d6-59ea495c37ff";
        public const string ParticipantStatusIntentKey = "ParticipantStatus_14dc8df1-dc88-4e39-81d6-59ea495c37ff";
        public const string IsSilentIntentKey = "IsSilent_15dc8df1-dc88-4e39-81d6-59ea495c37ff";
        public const string OriginalDocumentIdIntentKey = "OriginalDocumentIdIntent_16dc8df1-dc88-4e39-81d6-59ea495c37ff";
        public const string OriginalDocumentFolderIdIntentKey = "OriginalDocumentFolderIdIntentKey_17dc8df1-dc88-4e39-81d6-59ea495c37ff";


        public static Intent CreateIntent(Context context, Document document, DocumentPreview documentPreview, 
        CalendarInvitation invitation, ParticipantStatus status, bool isSilent, int originalDocumentId,
        int originalDocumentFolderId)
        {
            var intent = new Intent(context, typeof(MsalAuthActivity));
          
            if (document != null)
                intent.PutExtra(DocumentIntentKey, Serializer.Serialize(document));

            if (documentPreview != null)
                intent.PutExtra(DocumentPreviewIntentKey, Serializer.Serialize(documentPreview));

            if (invitation != null)
                intent.PutExtra(CalendarInvitationIntentKey, Serializer.Serialize(invitation));

            intent.PutExtra(ParticipantStatusIntentKey, Serializer.Serialize(status));
                
            intent.PutExtra(IsSilentIntentKey, isSilent);

            intent.PutExtra(OriginalDocumentIdIntentKey, originalDocumentId);

            intent.PutExtra(OriginalDocumentFolderIdIntentKey, originalDocumentFolderId);         

            return intent;
        }


        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Build.VERSION.SdkInt >=  BuildVersionCodes.R) {
                OverridePendingTransition(0, 0);
             }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                SetTranslucent(true);
                Window?.SetBackgroundDrawable(new ColorDrawable(Android.Graphics.Color.Transparent));
            }

            CommonConfig.Logger.Info($"Creating {nameof(MsalAuthActivity)}...");

            var responseDocument = Serializer.Deserialize<Document>(Intent?.Extras?.GetString(DocumentIntentKey));
            var responseDocumentPreview = Serializer.Deserialize<DocumentPreview>(Intent?.Extras?.GetString(DocumentPreviewIntentKey));
            var invitation = Serializer.Deserialize<CalendarInvitation>(Intent?.Extras?.GetString(CalendarInvitationIntentKey));
            var status = Serializer.Deserialize<ParticipantStatus>(Intent?.Extras?.GetString(ParticipantStatusIntentKey));
            var isSilent = Intent?.Extras?.GetBoolean(IsSilentIntentKey) ?? true;
            var originalDocumentId = Intent?.Extras?.GetInt(OriginalDocumentIdIntentKey) ?? 0;
            var originalDocumentFolderId = Intent?.Extras?.GetInt(OriginalDocumentFolderIdIntentKey) ?? 0;

            if (Managers.MicrosoftGraphClient == null || !Managers.MicrosoftGraphClient.IsAuthenticated())
            {
                await Authenticate();
            }

            await ReplyToInvitation(responseDocument, responseDocumentPreview, invitation, status, isSilent, originalDocumentId, originalDocumentFolderId);

            async Task Authenticate()
            {
                try
                {
                    Managers.MicrosoftGraphClient = new MicrosoftGraphClient();
                    var authResult = await Managers.MicrosoftGraphClient.Authenticate(this, forceInteractive: false);
                }
                catch (Exception ex)
                {
                    var intent = new Intent();
                    intent.PutExtra(ExceptionResultKey, Serializer.Serialize(ex));
                    SetResult(Result.Canceled, intent);
                    Finish();
                }
            }

            async Task ReplyToInvitation(Document responseDocument, DocumentPreview responseDocumentPreview, CalendarInvitation invitation, ParticipantStatus status, bool isSilent, int originalDocumentId, int originalDocumentFolderId)
            {
                try
                {
                     await Managers.DocumentsManager.ReplyToCalendarInvitationAsync(
                            responseDocument, responseDocumentPreview,
                            invitation, status, isSilent,
                            originalDocumentId, originalDocumentFolderId);

                    var intent = new Intent();
                    intent.PutExtra(StatusResultKey, Serializer.Serialize(status));
                    SetResult(Result.Ok, intent);
                    Finish();
                }
                catch(Exception ex)
                {
                    var intent = new Intent();
                    intent.PutExtra(ExceptionResultKey, Serializer.Serialize(ex));
                    SetResult(Result.Canceled, intent);
                    Finish();
                }  
            }
        }

        protected override void OnActivityResult(int requestCode,
                                         Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode,
                                                                                    resultCode,
                                                                                    data);
        }

       
        protected override void OnPause() {
            base.OnPause();
            if (Build.VERSION.SdkInt >=  BuildVersionCodes.R) {
                OverridePendingTransition(0, 0);

             }
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (Build.VERSION.SdkInt >=  BuildVersionCodes.R) {
                OverridePendingTransition(0, 0);

             }
        }

        protected override void OnDestroy()
        {
             base.OnStop();
             if (Build.VERSION.SdkInt >=  BuildVersionCodes.R) {
                OverridePendingTransition(0, 0);

             }
        }


    }
}
