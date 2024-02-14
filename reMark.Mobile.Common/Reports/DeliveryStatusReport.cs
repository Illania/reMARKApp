using System;
using System.Collections.Generic;
using HtmlTags;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Common.Utilities.Extensions;

namespace reMark.Mobile.Common.Reports
{
    public static class DeliveryStatusReport
    {

        public static string GetReportHtml(DeliveryReportData data)
        {
            var properties = new Dictionary<string, string>() {
                { "Sent from", data.Status.WasSentByLineName },
                { "Status details", data.Status.StatusDetail.ToString() },
                { "Message", data.Status.LastMessage },
                { "Start time", data.Status.TimeStart.ToString("MM/dd/yyyy hh:mm tt") },
                { "End time", data.Status.TimeEnd.ToString("MM/dd/yyyy hh:mm tt") },
                { "Duration", GetDurationString(data.Status) },
                { "Attempts count", data.Status.Attempts.ToString() },
                { "Last attempt time", data.Status.LastConnectAttempt.ToString("MM/dd/yyyy hh:mm tt") }
            };

            var htmlDocument = new HtmlDocument();

            #region CSS Styles
            htmlDocument.AddStyle($@"

                    table {{
                        border-collapse: collapse;
                        margin: 25px 0;
                        font-size: 0.9em;
                        font-family: sans-serif;
                        width: 100%;
                        min-width: 400px;
                        box-shadow: 0 0 20px rgba(0, 0, 0, 0.15);
                    }}

                    thead tr {{
                        background-color: {ThemeColors.DarkBlue.ToHtml()};
                        color: #ffffff;
                        text-align: left;
                    }}

                    tbody tr {{
                        border-bottom: 1px solid #dddddd;
                    }}

                    tbody tr:nth-of-type(even) {{
                        background-color: #f3f3f3;
                    }}

                    tbody tr:last-of-type {{
                        border-bottom: 2px solid {ThemeColors.DarkBlue.ToHtml()};
                    }}

                    tbody tr.active-row {{
                        background-color: {ThemeColors.LightBlue.ToHtml()};
                    }}

                    th,td {{
                        padding: 12px 15px;
                        white-space: pre-wrap;
                        word-wrap:break-word;
                    }}

                    .property_name {{
                        font-weight: bold;
                    }}
      
            ");
            #endregion

            var table = htmlDocument.Body.Add<TableTag>()
                .AddHeaderRow(cells =>
                {
                    cells.Cell($"Delivery report for {data.ReferenceName} --> {data.Destination}").Attr("colspan",2);
                });
           
            foreach (var p in properties)
                table
                    .AddBodyRow(cells =>
                    {
                        cells.Cell(p.Key).AddClass("property_name");
                        cells.Cell(p.Value);
                    });

            return htmlDocument.ToString();
        }

        static string GetDurationString(DestinationStatus destStatus)
        {
            TimeSpan duration;

            if (destStatus.TimeStart < destStatus.TimeEnd && !destStatus.TimeEnd.Equals(DateTime.MinValue))
            {
                duration = destStatus.TimeEnd.Subtract(destStatus.TimeStart);
            } 
            else
            {
                duration = destStatus.TimeStart > DateTime.Now
                    ? destStatus.TimeStart.Subtract(DateTime.Now)
                    : DateTime.Now.Subtract(destStatus.TimeStart);
            }

            return (duration > TimeSpan.MinValue) ? duration.ToReadableString() : string.Empty;
        }

    }
}
