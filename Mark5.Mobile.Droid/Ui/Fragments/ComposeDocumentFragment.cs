//
// Project: Mark5.Mobile.Droid
// File: ComposeDocumentFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ComposeDocumentFragment : RetainableStateFragment
    {
        #region Retained State methods

        public override IRetainableState OnRetainInstanceState()
        {
            //CommonConfig.Logger.Info($"Retaining state [entity.Id={Entity?.Id}, addCommentText={addCommentEditText?.Text}");

            //return new CommentsFragmentState
            //{
            //    Entity = Entity,
            //    AddCommentText = addCommentEditText.Text
            //};

            //TODO to implement
            throw new NotImplementedException();

        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var cfs = restoredState as ComposeDocumentFragmentState;
            if (cfs != null)
            {
                //TODO to implement
            }
        }

        public override string GenerateTag()
        {
            //return $"{nameof(CommentsListFragment)} [businessEntity.Id={Entity.Id}]";
            throw new NotImplementedException();
        }

        class ComposeDocumentFragmentState : IRetainableState
        {
            public Document Document { get; set; }
            public DocumentPreview DocumentPreview { get; set; }
        }

        #endregion
    }
}
