using System;

namespace Mark5.Mobile.Common.Model.Containers
{
    public class TemplateContainer
    {
        public TemplatePreview TemplatePreview { get; }
        public Template Template { get; }

        public TemplateContainer(TemplatePreview templatePreview, Template template)
        {
            if (templatePreview == null)
                throw new ArgumentNullException(nameof(templatePreview));
            if (template == null)
                throw new ArgumentNullException(nameof(template));
            if (templatePreview.Id != template.Id)
                throw new ArgumentException("TemplatePreview and Template do not match.");

            TemplatePreview = templatePreview;
            Template = template;
        }
    }
}