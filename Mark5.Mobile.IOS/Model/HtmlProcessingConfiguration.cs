using System;
namespace Mark5.Mobile.IOS.Model
{
    public class HtmlProcessingConfiguration
    {
        public static HtmlProcessingConfiguration DefaultForViewing
        {
            get => new HtmlProcessingConfiguration();
        }

        public static HtmlProcessingConfiguration DefaultForEditing
        {
            get => new HtmlProcessingConfiguration
            {
                InlineCss = true,
                MakeEditable = true
            };
        }

        public static HtmlProcessingConfiguration Disabled
        {
            get => new HtmlProcessingConfiguration
            {
                MakeHtmlKindaSafe = false,
                CorrectScale = false,
                InjectFonts = false
            };
        }

        public bool MakeHtmlSafe { get; set; } = false;
        public bool MakeHtmlKindaSafe { get; set; } = true;
        public bool CorrectScale { get; set; } = true;
        public bool InjectFonts { get; set; } = true;
        public bool InlineCss { get; set; } = false;
        public bool MakeEditable { get; set; } = false;
        public bool InjectReplyHeader { get; set; } = false;

        public string[] ReplyHeaderParameters { get; set; }
    }

}
