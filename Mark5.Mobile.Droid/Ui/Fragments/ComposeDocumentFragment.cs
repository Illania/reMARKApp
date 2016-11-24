//
// Project: Mark5.Mobile.Droid
// File: ComposeDocumentFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Database;
using Android.Provider;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Support;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ComposeDocumentFragment : RetainableStateFragment
    {
        const string DefaultTitle = "New document";
        const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB
        public const int AttachmentRequestCode = 111;

        public DocumentDirection PreviousDocumentDirection { get; set; }
        public DocumentCreationModeFlag CreationModeFlag { get; set; }
        public int? PreviousDocumentFolderId { get; set; }
        public int? PreviousDocumentId { get; set; }

        Document PreviousDocument { get; set; }
        DocumentPreview PreviousDocumentPreview { get; set; }

        Document Document { get; set; } = new Document();
        DocumentPreview DocumentPreview { get; set; } = new DocumentPreview();
        public Guid OutgoingDocumentGuid { get; set; } //TODO eventually this should be set for preexisting documents (take care when doing the local documents)

        ToView toView;
        CcView ccView;
        BccView bccView;
        PriorityView priorityView;
        LineView lineView;
        SubjectView subjectView;
        AttachmentsView attachmentsView;
        ContentView contentView;

        IMenu optionsMenu;
        List<ComposeDocumentView> subViews = new List<ComposeDocumentView>();

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;
        bool documentShown;
        bool resuming;
        bool templateLoaded;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"{nameof(ComposeDocumentFragment)} [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}]");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            progress.Visibility = ViewStates.Gone;
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            scrollView.Visibility = ViewStates.Visible;
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            toView = new ToView(Context);
            toView.Edited += Subview_Edited;
            subViews.Add(toView);

            ccView = new CcView(Context);
            ccView.Edited += Subview_Edited;
            subViews.Add(ccView);

            bccView = new BccView(Context);
            bccView.Edited += Subview_Edited;
            subViews.Add(bccView);

            priorityView = new PriorityView(Context);
            subViews.Add(priorityView);

            lineView = new LineView(Context);
            lineView.Edited += Subview_Edited;
            subViews.Add(lineView);

            subjectView = new SubjectView(Context);
            subjectView.Edited += Subview_Edited;
            subViews.Add(subjectView);

            attachmentsView = new AttachmentsView(Context);
            attachmentsView.AttachmentClicked += AttachmentsView_AttachmentClicked;
            subViews.Add(attachmentsView);

            contentView = new ContentView(Context);
            subViews.Add(contentView);

            foreach (var subview in subViews)
            {
                linearLayout.AddView(subview);
                if (subview != contentView)
                {
                    linearLayout.AddView(new Divider(Context));
                }
            }

            HasOptionsMenu = true;

            return rootView;
        }

        public override async void OnResume()
        {
            base.OnResume();

            if (resuming) //On resume called again after access permission requests
            {
                return;
            }

            resuming = true;

            if (OutgoingDocumentGuid == Guid.Empty)
            {
                OutgoingDocumentGuid = Guid.NewGuid();
            }

            await LoadDocument();
            await ShowDocument();

            resuming = false;
        }

        async Task LoadDocument()
        {
            if (PreviousDocument != null || CreationModeFlag == DocumentCreationModeFlag.New)
            {
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.loading_document, Resource.String.please_wait);

            try
            {
                var sourceType = PreviousDocumentDirection == DocumentDirection.Draft ? SourceType.Auto : SourceType.Local;
                var container = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(PreviousDocumentFolderId.Value, PreviousDocumentId.Value, sourceType);
                PreviousDocument = container.Document;
                PreviousDocumentPreview = container.DocumentPreview;
            }
            catch (Exception ex)
            {
                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
                Activity.Finish();
            }

            dismissAction();
        }

        async Task ShowDocument()
        {
            if (documentShown)
            {
                return;
            }

            documentShown = true;

            foreach (var subView in subViews)
            {
                subView.Document = Document;
                subView.DocumentPreview = DocumentPreview;
                subView.PreviousDocument = PreviousDocument;
                subView.PreviousDocumentPreview = PreviousDocumentPreview;
                subView.CreationModeFlag = CreationModeFlag;
                await subView.RefreshView();
            }

            UpdateSendButtonState();

            await AskIfShouldUseTemplates();
        }

        #region Subviews event handlers

        void Subview_Edited(object sender, EventArgs e)
        {
            ((AppCompatActivity)Activity).SupportActionBar.Title = !subjectView.Empty ? subjectView.Subject : DefaultTitle;
            UpdateSendButtonState();
        }

        async void AttachmentsView_AttachmentClicked(object sender, IAttachmentDescription attachment)
        {
            var option = await Dialogs.ShowListDialog(Context, Resource.String.attachment_clicked, Resource.Array.attachment_clicked_options);

            if (option == 0) //Open attachment
            {
                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.opening_attachment, Resource.String.please_wait);

                string path = null;

                var outgoingAttachment = attachment as OutgoingDocumentAttachmentDescription;
                if (outgoingAttachment != null)
                {
                    path = outgoingAttachment.Path;
                }
                else
                {
                    var remoteAttachment = attachment as AttachmentDescription;

                    path = await Managers.DocumentsManager.GetAttachmentAsync(remoteAttachment, PreviousDocumentFolderId.Value, Document, false, SourceType.Local);

                    if (string.IsNullOrWhiteSpace(path))
                    {
                        if (remoteAttachment.SizeInBytes > LargeAttachmentSizeInBytes
                            && PlatformConfig.Preferences.LargeAttachmentWarning
                            && Integration.IsConnectedToMeteredConnection()
                            && !await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.warning, Resource.String.large_attachment))
                        {
                            dismissAction();
                            return;
                        }

                        path = await Managers.DocumentsManager.GetAttachmentAsync(remoteAttachment, PreviousDocumentFolderId.Value, PreviousDocument, false, SourceType.Remote);
                    }
                }

                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new Exception("Unable to get attachment path.");
                }

                try
                {
                    var uri = FileProvider.GetUriForFile(Context, Context.PackageName + ".fileprovider", new Java.IO.File(path));
                    var mimeType = Context.ContentResolver.GetType(uri);

                    var openFileIntent = new Intent(Intent.ActionView);
                    openFileIntent.SetDataAndType(uri, mimeType);
                    openFileIntent.AddFlags(ActivityFlags.NewTask);
                    openFileIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
                    Intent chooser = Intent.CreateChooser(openFileIntent, Resources.GetString(Resource.String.choose_application));
                    Context.StartActivity(chooser);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Failed to view attachment [AttachmentName={outgoingAttachment?.Name}, PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}]", ex);
                    dismissAction();
                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                }
                finally
                {
                    dismissAction();
                }

            }
            else if (option == 1) //Remove attachment
            {
                var outgoingAttachment = attachment as OutgoingDocumentAttachmentDescription;
                if (outgoingAttachment != null)
                {
                    try
                    {
                        await Managers.DocumentsManager.RemoveOutgoingAttachmentAsync(OutgoingDocumentGuid, outgoingAttachment.Name);
                        attachmentsView.RemoveAttachment(sender, outgoingAttachment);
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error($"Error while removing attachment [AttachmentName={outgoingAttachment?.Name}, PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}]", ex);
                        await Dialogs.ShowErrorDialogAsync(Activity, new Exception(Resources.GetString(Resource.String.error_removing_local_attachment)));
                    }
                }
                else
                {
                    var remoteAttachment = attachment as AttachmentDescription;
                    attachmentsView.RemoveAttachment(sender, remoteAttachment);
                }
            }
        }

        #endregion

        #region Actions

        void SendDocument(bool draft = false)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, draft ? Resource.String.saving_draft : Resource.String.sending_document, Resource.String.please_wait);

            Task.Run(async () =>
            {
                foreach (var subView in subViews)
                {
                    await subView.UpdateDocument();
                }

                DocumentPreview.Direction = draft ? DocumentDirection.Draft : DocumentDirection.Outgoing;
                DocumentPreview.DateReceivedTimestamp = DateTime.Now.ToUniversalTime().ConvertDateTimeToTimestampMilliseconds();
                await Managers.DocumentsManager.InsertDocumentInOutgoingAsync(OutgoingDocumentGuid, Document, DocumentPreview, CreationModeFlag,
                                                                        PreviousDocumentId ?? -1, PreviousDocumentFolderId ?? -1,
                                                                       0, false, false);
            }).ContinueWith(async t =>
           {
               dismissAction();

               if (t.IsFaulted)
               {
                   CommonConfig.Logger.Error($"Failed to insert document in outgoing [isDraft={draft}, PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", t.Exception.InnerException);
                   await Dialogs.ShowErrorDialogAsync(Activity, t.Exception.InnerException);
               }
               else
               {
                   Activity.Finish();
               }
           }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void AskIfShouldSaveAsDraft()
        {
            Dialogs.ShowYesNoDialog(Context, Resource.String.save_draft, Resource.String.confirm_save_as_draft, () => SendDocument(true), CloseComposeActivity);
        }

        void CloseComposeActivity()
        {
            Task.Run(async () =>
           {
               await Managers.DocumentsManager.DeleteOutgoingDocumentFolder(OutgoingDocumentGuid);
           }).ContinueWith(t =>
          {
              Activity.Finish();
          }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void HandleLocalAttachment(Intent data)
        {
            OutgoingDocumentAttachmentDescription attachment = null;
            bool attachmentTooBig = false;
            Stream stream = null;

            Task.Run(async () =>
            {
                var uri = data.Data;
                stream = Activity.ContentResolver.OpenInputStream(uri);

                string filename;

                if (uri.Scheme == "file")
                {
                    filename = uri.LastPathSegment;
                }
                else
                {
                    ICursor cursor = null;
                    try
                    {
                        cursor = Activity.ContentResolver.Query(uri, null, null, null, null);
                        var nameIndex = cursor.GetColumnIndex(OpenableColumns.DisplayName);
                        cursor.MoveToFirst();

                        filename = cursor.GetString(nameIndex);
                    }
                    finally
                    {
                        cursor.Close();
                    }
                }

                var path = await Managers.DocumentsManager.SaveOutgoingAttachmentAsync(OutgoingDocumentGuid, filename, stream);
                var size = (new Java.IO.File(path)).Length();

                if (size > ServerConfig.SystemSettings.DocumentsModuleInfo.MaximumAttachmentSizeBytes)
                {
                    attachmentTooBig = true;
                    await Managers.DocumentsManager.RemoveOutgoingAttachmentAsync(OutgoingDocumentGuid, filename);
                    throw new Exception();
                }

                attachment = new OutgoingDocumentAttachmentDescription
                {
                    Name = filename,
                    SizeInBytes = size,
                    Stream = stream,
                    Path = path
                };

            }).ContinueWith(async t =>
            {
                stream?.Dispose();

                if (t.IsFaulted)
                {
                    CommonConfig.Logger.Error($"Failed to save attachment to memory [AttachmentName={attachment?.Name}, PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}]", t.Exception.InnerException);
                    var resourceStringId = attachmentTooBig ? Resource.String.attachment_too_big : Resource.String.error_saving_local_attachment;
                    await Dialogs.ShowErrorDialogAsync(Activity, new Exception(Resources.GetString(resourceStringId)));
                }
                else
                {
                    attachmentsView.AddAttachment(attachment);
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        #endregion


        #region Options menu related

        static class MenuItemActions
        {
            public const int AddAttachment = 10;
            public const int SendDocument = 20;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            optionsMenu = menu;

            var attachmentItem = menu.Add(Menu.None, MenuItemActions.AddAttachment, MenuItemActions.AddAttachment, Resource.String.add_attachment);
            attachmentItem.SetIcon(Resource.Drawable.add_attachment);
            attachmentItem.SetShowAsAction(ShowAsAction.Always);

            var sendItem = menu.Add(Menu.None, MenuItemActions.SendDocument, MenuItemActions.SendDocument, Resource.String.send);
            sendItem.SetIcon(Resource.Drawable.send_white);
            sendItem.SetShowAsAction(ShowAsAction.Always);
            sendItem.SetEnabled(false);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.SendDocument)
            {
                SendDocument();
            }
            if (item.ItemId == MenuItemActions.AddAttachment)
            {
                AddAttachment();
            }

            return true;
        }

        void AddAttachment()
        {
            var intent = new Intent(Intent.ActionGetContent);
            intent.SetType("*/*");
            intent.AddCategory(Intent.CategoryOpenable);
            Intent i = Intent.CreateChooser(intent, "File");
            Activity.StartActivityForResult(i, AttachmentRequestCode);
        }

        void UpdateSendButtonState()
        {
            var sendItem = optionsMenu?.FindItem(MenuItemActions.SendDocument);
            if (sendItem != null)
            {
                var isFormValid = IsFormValid();
                sendItem.SetEnabled(isFormValid);
                sendItem.Icon.Mutate().Alpha = isFormValid ? 255 : 130; //Change of alpha is not done for icons automatically
            }
        }

        bool IsFormValid()
        {
            if (subjectView.Empty)
            {
                return false;
            }

            var recipientAdded = false;
            foreach (var recipientView in new List<RecipientsView> { toView, ccView, bccView })
            {
                recipientAdded |= !recipientView.Empty;
            }

            if (!recipientAdded)
            {
                return false;
            }

            return !lineView.LineSelectedIsAmbiguous;
        }

        #endregion

        #region Template methods

        async Task AskIfShouldUseTemplates()
        {
            if (templateLoaded)
            {
                return;
            }

            if (CreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                CommonConfig.Logger.Info("Document opened in edit mode, no need to add template");
                return;
            }

            var useTemplate = PlatformConfig.Preferences.UseTemplate;
            if (useTemplate == Preferences.TemplateUsageMode.DontUse)
            {
                return;
            }

            if (useTemplate == Preferences.TemplateUsageMode.Local)
            {
                await GetLocalTemplate();
            }
            else if (useTemplate == Preferences.TemplateUsageMode.Default)
            {
                await GetDefaultTemplate();
            }
            else if (useTemplate == Preferences.TemplateUsageMode.AlwaysAsk)
            {
                var result = await Dialogs.ShowListDialog(Context, Resource.String.template_question, Resource.Array.template_question_options, true);
                switch (result)
                {
                    case 0:
                        await GetDefaultTemplate(true);
                        break;
                    case 1:
                        await GetLocalTemplate();
                        break;
                    case 2:
                        await GetAllTemplates();
                        break;
                }
            }

            templateLoaded = true;
        }

        async Task GetAllTemplates()
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_templates, Resource.String.please_wait);
            List<TemplatePreview> templatesPreviews = null;

            try
            {
                templatesPreviews = await Managers.DocumentsManager.GetTemplatePreviewsAsync();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting default template [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
            }

            if (templatesPreviews != null)
            {
                var templatesForCreationMode = templatesPreviews.Where(t => t.CreationMode.HasFlag(CreationModeFlag));
                if (templatesForCreationMode.Any())
                {
                    var templateNames = templatesForCreationMode.Select(t => (t.Private ? "[Private] " : "[Public] ") + t.Name).ToArray();
                    var result = await Dialogs.ShowListDialog(Context, Resource.String.template_question, templateNames, true);
                    var selectedPreview = templatesPreviews[result];
                    await GetTemplate(selectedPreview);
                }
                else
                {
                    await Dialogs.ShowConfirmDialogAsync(Context, Resource.String.no_templates_title, Resource.String.no_templates_content);
                }
            }
        }

        async Task GetLocalTemplate()
        {
            var localTemplate = PlatformConfig.Preferences.LocalTemplate;
            await contentView.InsertLocalTemplate(localTemplate);
        }

        async Task GetDefaultTemplate(bool errorMessageIfNull = false)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_template, Resource.String.please_wait);

            try
            {
                var template = await Managers.DocumentsManager.GetDefaultTemplateAsync(CreationModeFlag);
                if (template != null)
                {
                    await ApplyTemplate(template);
                }
                else if (errorMessageIfNull)
                {
                    throw new Exception(Resources.GetString(Resource.String.template_null));
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting default template [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        async Task GetTemplate(TemplatePreview templatePreview)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_template, Resource.String.please_wait);

            try
            {
                var template = await Managers.DocumentsManager.GetTemplateAsync(templatePreview.Id);
                if (template != null)
                {
                    await ApplyTemplate(template);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting template [template.Id={templatePreview?.Id}, PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        async Task ApplyTemplate(Template template)
        {
            ProcessTemplate(template);

            await contentView.InsertTemplate(template);

            if (!string.IsNullOrEmpty(template.Subject))
            {
                subjectView.SetSubject(template.Subject);
            }

            lineView.SetLineFromGuid(template.LineGuid);
        }

        void ProcessTemplate(Template template)
        {
            var templateContent = template.Content;

            var currentTime = DateTime.Now;
            var dateString = currentTime.ToString("dd-MM-yyyy");
            var timeString = currentTime.ToString("HH:mm");

            string fromNameString = string.Empty;
            if (PreviousDocumentPreview != null && PreviousDocumentPreview.Addresses != null)
            {
                fromNameString = PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.From).Select(da => da.Name).FirstOrDefault() ?? string.Empty;
            }

            if (template.ContentType == ContentType.Html)
            {
                templateContent = templateContent.Replace("&lt;FROMNAME&gt;", fromNameString);
                templateContent = templateContent.Replace("&lt;DATE&gt;", dateString);
                templateContent = templateContent.Replace("&lt;TIME&gt;", timeString);
            }
            else
            {
                templateContent = templateContent.Replace("<FROMNAME>", fromNameString);
                templateContent = templateContent.Replace("<DATE>", dateString);
                templateContent = templateContent.Replace("<TIME>", timeString);
            }

            if (template.ContentType == ContentType.Html)
            {
                templateContent = templateContent.Replace("&lt;CUR&gt;", string.Empty);
                templateContent = templateContent.Replace("&lt;SOURCETEXT&gt;", string.Empty);
                templateContent = templateContent.Replace("&lt;COMPANYNAME&gt;", string.Empty);
                templateContent = templateContent.Replace("&lt;FROMNAMEWITHCOMPANY&gt;", string.Empty);
            }
            else
            {
                templateContent = templateContent.Replace("<CUR>", string.Empty);
                templateContent = templateContent.Replace("<SOURCETEXT>", string.Empty);
                templateContent = templateContent.Replace("<COMPANYNAME>", string.Empty);
                templateContent = templateContent.Replace("<FROMNAMEWITHCOMPANY>", string.Empty);
            }

            template.Content = templateContent;
        }

        #endregion

        #region Retained State methods

        public override IRetainableState OnRetainInstanceState()
        {
            return new ComposeDocumentFragmentState()
            {
                Document = Document,
                DocumentPreview = DocumentPreview,
                PreviousDocument = PreviousDocument,
                PreviousDocumentPreview = PreviousDocumentPreview,
                PreviousDocumentFolderId = PreviousDocumentFolderId,
                PreviousDocumentId = PreviousDocumentId,
                OutgoingDocumentGuid = OutgoingDocumentGuid,
                CreationModeFlag = CreationModeFlag,
                TemplateLoaded = templateLoaded,
                ToState = toView.ReturnState(),
                CcState = ccView.ReturnState(),
                BccState = bccView.ReturnState(),
                PriorityState = priorityView.ReturnState(),
                LineState = lineView.ReturnState(),
                SubjectState = subjectView.ReturnState(),
                AttachmentsState = attachmentsView.ReturnState(),
                ContentState = contentView.ReturnState(),
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var cfs = restoredState as ComposeDocumentFragmentState;
            if (cfs != null)
            {
                Document = cfs.Document;
                DocumentPreview = cfs.DocumentPreview;
                PreviousDocument = cfs.PreviousDocument;
                PreviousDocumentPreview = cfs.PreviousDocumentPreview;
                PreviousDocumentFolderId = cfs.PreviousDocumentFolderId;
                PreviousDocumentId = cfs.PreviousDocumentId;
                OutgoingDocumentGuid = cfs.OutgoingDocumentGuid;
                CreationModeFlag = cfs.CreationModeFlag;
                templateLoaded = cfs.TemplateLoaded;
                toView.State = cfs.ToState;
                ccView.State = cfs.CcState;
                bccView.State = cfs.BccState;
                priorityView.State = cfs.PriorityState;
                lineView.State = cfs.LineState;
                subjectView.State = cfs.SubjectState;
                attachmentsView.State = cfs.AttachmentsState;
                contentView.State = cfs.ContentState;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ComposeDocumentFragment)} [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}]";
        }

        class ComposeDocumentFragmentState : IRetainableState
        {
            public Document Document { get; set; }
            public DocumentPreview DocumentPreview { get; set; }
            public Document PreviousDocument { get; set; }
            public DocumentPreview PreviousDocumentPreview { get; set; }
            public int? PreviousDocumentFolderId { get; set; }
            public int? PreviousDocumentId { get; set; }
            public Guid OutgoingDocumentGuid { get; set; }
            public bool TemplateLoaded { get; set; }
            public DocumentCreationModeFlag CreationModeFlag { get; set; }
            public IComposeDocumentViewState ToState { get; set; }
            public IComposeDocumentViewState CcState { get; set; }
            public IComposeDocumentViewState BccState { get; set; }
            public IComposeDocumentViewState PriorityState { get; set; }
            public IComposeDocumentViewState LineState { get; set; }
            public IComposeDocumentViewState SubjectState { get; set; }
            public IComposeDocumentViewState AttachmentsState { get; set; }
            public IComposeDocumentViewState ContentState { get; set; }
        }

        #endregion
    }
}
