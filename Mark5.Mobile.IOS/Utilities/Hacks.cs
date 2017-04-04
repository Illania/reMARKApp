//
// Project: Mark5.Mobile.IOS
// File: Hacks.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using Mark5.Mobile.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{
    
    public static class Hacks
    {

        public static void CorrectFontInActions(UITableViewCell cell, UIFont font)
        {
            try
            {
                var labels = FindUTVCDCV(cell);

                if (labels == null)
                    return;

                foreach (var label in labels)
                    label.Font = font;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger?.Error(ex);
            }
        }

        /// <summary>
        /// Used to find buttons in table view cell row actions.
        /// </summary>
        /// <returns>The utvcdcv.</returns>
        /// <param name="cell">Cell for which adjustments should be applied.</param>
        static UILabel[] FindUTVCDCV(UITableViewCell cell)
        {
            var utvcdcv_namepfx = "_";
            var utvcdcv_name0 = "UI";
            var utvcdcv_name1 = "TableView";
            var utvcdcv_name2 = "Cell";
            var utvcdcv_name3 = "Delete";
            var utvcdcv_name4 = "Confirmation";
            var utvcdcv_name5 = "View";
            var utvcdcv_name6 = "Action";
            var utvcdcv_name7 = "Button";
            var utvcdcv_name8 = "Label";

            UIView utvcdcvs = null;
            foreach (var v in cell.Subviews)
                if (v.Class.Name == utvcdcv_name0 + utvcdcv_name1 + utvcdcv_name2 + utvcdcv_name3 + utvcdcv_name4 + utvcdcv_name5)
                    utvcdcvs = v;

            if (utvcdcvs == null)
                return null;

            var l = new List<UILabel>();

            foreach (var utvcdcv in utvcdcvs.Subviews)
                if (utvcdcv.Class.Name == utvcdcv_namepfx + utvcdcv_name0 + utvcdcv_name1 + utvcdcv_name2 + utvcdcv_name6 + utvcdcv_name7)
                    foreach (var ubl in utvcdcv.Subviews)
                        if (ubl.Class.Name == utvcdcv_name0 + utvcdcv_name7 + utvcdcv_name8)
                            l.Add((UILabel)ubl);

            return l.Count < 1 ? null : l.ToArray();
        }
    }
}
