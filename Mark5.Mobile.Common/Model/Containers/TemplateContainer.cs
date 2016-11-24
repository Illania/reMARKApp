//
// Project: Mark5.Mobile.Common
// File: TemplateContainer.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model.Containers
{

    public class TemplateContainer
    {

        public TemplatePreview TemplatePreview { get; private set; }

        public Template Template { get; private set; }

        public TemplateContainer(TemplatePreview templatePreview, Template template)
        {
            if (templatePreview == null)
            {
                throw new ArgumentNullException(nameof(templatePreview));
            }

            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            if (templatePreview.Id != template.Id)
            {
                throw new ArgumentException("TemplatePreview and Template do not match.");
            }

            TemplatePreview = templatePreview;
            Template = template;
        }
    }
}

