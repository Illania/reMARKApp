using System.Collections.Generic;
using System.Linq;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Common.Utilities
{
    public static class DocumentsDeleteChecker
    {
        public static bool CanDeleteDocuments(List<DocumentPreview> documents)
        {
            if ((!ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed
                && documents.Any(dp => dp.Direction != DocumentDirection.Draft)) 
                || documents.Count == 0)
            {
                return false;
            }

            if (documents.All(dp => dp.Direction == DocumentDirection.Draft)
                || !ServerConfig.SystemSettings.SystemInfo.DeleteDocumentsAllowedLinesAvailable)
            {
                return true;
            }

            var linesAllowedToDelete =
                ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteDocumentsAllowedLines;
            if (linesAllowedToDelete == null || !linesAllowedToDelete.Any())
                return false;

            if (documents.Count != 1) 
                return true;
            
            var document = documents.FirstOrDefault();
            if (document == null) 
                return false;
            
            var linesGuids = document.Lines.Select(l => l.Guid);
            var intersection = linesAllowedToDelete.Intersect(linesGuids);
            return intersection.Any();
        }
    }
}