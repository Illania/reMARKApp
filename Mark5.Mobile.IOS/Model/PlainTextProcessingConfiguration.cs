using System;
namespace Mark5.Mobile.IOS.Model
{
    public class PlainTextProcessingConfiguration
    {
        public static PlainTextProcessingConfiguration DefaultForViewing
        {
            get => new PlainTextProcessingConfiguration();
        }

        public static PlainTextProcessingConfiguration DefaultForEditing
        {
            get => new PlainTextProcessingConfiguration
            {
                MakeEditable = true
            };
        }

        public static PlainTextProcessingConfiguration Disabled
        {
            get => new PlainTextProcessingConfiguration
            {
                Encode = false,
            };
        }

        public bool Encode { get; set; } = true;
        public bool MakeEditable { get; set; } = false;
        public bool InjectReplyHeader { get; set; } = false;

        public string[] ReplyHeaderParameters { get; set; }
    }
}
