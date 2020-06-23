using System;
using System.Linq;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class LineUtilities
    {
        public static Line GetLineForCreationModeFlag(DocumentCreationModeFlag creationModeFlag, Document previousDocument)
        {
            var defaultOutgoingLine = ServerConfig.SystemSettings.DocumentsModuleInfo.DefaultOutgoingLine;
            var availableOutgoingLines = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines;

            if (creationModeFlag == DocumentCreationModeFlag.New)
                return defaultOutgoingLine;

            if (creationModeFlag == DocumentCreationModeFlag.Edit)
                return previousDocument.Lines.FirstOrDefault();

            if (creationModeFlag == DocumentCreationModeFlag.Reply ||
                creationModeFlag == DocumentCreationModeFlag.ReplyAll ||
                creationModeFlag == DocumentCreationModeFlag.Forward)
            {
                if (PlatformConfig.Preferences.AlwaysUseDefaultLine && defaultOutgoingLine != null)
                    return defaultOutgoingLine;

                if (availableOutgoingLines.Count == 1)
                    return availableOutgoingLines.FirstOrDefault();

                if (previousDocument.Lines.FirstOrDefault(l => l.Guid == defaultOutgoingLine?.Guid) != null)
                    return defaultOutgoingLine;
                else
                {
                    var intersection = previousDocument.Lines.Intersect(availableOutgoingLines, LambdaEqualityComparer<Line>.Create(l => l.Guid)).ToArray();
                    if (intersection.Length == 1)
                        return intersection.FirstOrDefault();
                    else
                        return null;
                }
            }

            return null;
        }
    }
}
