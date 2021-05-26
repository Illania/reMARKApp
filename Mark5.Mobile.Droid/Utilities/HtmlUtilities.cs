using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Android.Content;
using HtmlAgilityPack;
using MailBee.Html;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Model;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class HtmlUtilities
    {
        public static async Task<string> ProcessHtml(Context ctx, string html, HtmlProcessingConfiguration config)
        {
            try
            {
                var sw = Stopwatch.StartNew();

                if (config.MakeHtmlSafe)
                {
                    html = await MakeHtmlSafe(html);

                    if (CommonConfig.Logger.IsDebugEnabled())
                        CommonConfig.Logger.Debug($"MakeHtmlSafe {sw.ElapsedMilliseconds}ms");
                    sw.Restart();
                }

                if (config.MakeHtmlKindaSafe)
                {
                    html = await MakeHtmlKindaSafe(html);

                    if (CommonConfig.Logger.IsDebugEnabled())
                        CommonConfig.Logger.Debug($"MakeHtmlKindaSafe {sw.ElapsedMilliseconds}ms");
                    sw.Restart();
                }

                if (config.InlineCss)
                {
                    html = await InlineCss(html);

                    if (CommonConfig.Logger.IsDebugEnabled())
                        CommonConfig.Logger.Debug($"InlineCss {sw.ElapsedMilliseconds}ms");
                    sw.Restart();
                }

                var htmlDocument = new HtmlDocument
                {
                    OptionReadEncoding = false
                };
                htmlDocument.LoadHtml(html);

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"LoadHtml {sw.ElapsedMilliseconds}ms");
                sw.Restart();

                if (config.CorrectScale)
                {
                    await CorrectScale(htmlDocument);

                    if (CommonConfig.Logger.IsDebugEnabled())
                        CommonConfig.Logger.Debug($"CorrectScale {sw.ElapsedMilliseconds}ms");
                    sw.Restart();
                }

                if (config.InjectFonts)
                {
                    await InjectFonts(htmlDocument);

                    if (CommonConfig.Logger.IsDebugEnabled())
                        CommonConfig.Logger.Debug($"InjectFonts {sw.ElapsedMilliseconds}ms");
                    sw.Restart();
                }

                if (config.MakeEditable)
                {
                    await MakeEditable(htmlDocument);

                    if (CommonConfig.Logger.IsDebugEnabled())
                        CommonConfig.Logger.Debug($"MakeEditable {sw.ElapsedMilliseconds}ms");
                    sw.Restart();
                }

                if (config.InjectReplyHeader)
                {
                    await InjectReplyHeader(ctx, htmlDocument, config.ReplyHeaderParameters);

                    if (CommonConfig.Logger.IsDebugEnabled())
                        CommonConfig.Logger.Debug($"InjectReplyHeader {sw.ElapsedMilliseconds}ms");
                    sw.Restart();
                }

                sw.Stop();

                return htmlDocument.DocumentNode.OuterHtml;
            }
            catch(Exception ex)
            {
                CommonConfig.Logger.Error("Error happened during html processing: ", ex);
                return string.Empty;
            }
            
        }

        public static async Task<string> ProcessPlainText(Context ctx, string text, PlainTextProcessingConfiguration config)
        {
            if (config.Encode)
                text = HttpUtility.HtmlEncode(text);

            string html;
            using (var sr = new StreamReader(ctx.Assets.Open("plain.html")))
                html = sr.ReadToEnd();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var preNode = htmlDocument.DocumentNode.SelectSingleNode("//pre[@id='plaintext']");
            preNode.InnerHtml = text;

            if (config.MakeEditable)
                await MakeEditable(htmlDocument);

            if (config.InjectReplyHeader)
                await InjectReplyHeader(ctx, htmlDocument, config.ReplyHeaderParameters);

            return htmlDocument.DocumentNode.OuterHtml;
        }

        public static Task<string> MakeHtmlSafe(string html)
        {
            return Task.Run(() =>
            {
                if (html == null)
                    return null;

                var p = new Processor();
                p.Dom.OuterHtml = html;
                var safeHtml = p.Dom.ProcessToString(RuleSet.GetSafeHtmlRules(), null);
                return safeHtml;
            });
        }

        public static Task<string> MakeHtmlKindaSafe(string html)
        {
            return Task.Run(() =>
            {
                var htmlDocument = new HtmlDocument
                {
                    OptionReadEncoding = false
                };
                htmlDocument.LoadHtml(html);

                //M5APP-920
                var titleTag = htmlDocument.DocumentNode.SelectSingleNode("//head/title");
                if (titleTag != null && string.IsNullOrEmpty(titleTag.InnerHtml))
                    titleTag.InnerHtml = "\n";

                var dn = htmlDocument.DocumentNode;

                var nodesToRemove = new List<HtmlNode>();

                foreach (var xpath in new[] { "//script", "//bgsound", "//embed", "//iframe", "//frame", "//frameset", "//object", "//applet" })
                {
                    var nodes = dn.SelectNodes(xpath);
                    if (nodes != null)
                        nodesToRemove.AddRange(nodes);
                }

                foreach (var nodeToRemove in nodesToRemove)
                    nodeToRemove.Remove();

                return htmlDocument.DocumentNode.OuterHtml;
            });
        }

        public static Task<string> InlineCss(string html)
        {
            return Task.Run(() =>
            {
                if (html == null)
                    return null;

                string result;

                try
                {
                    var inlineResult = PreMailer.Net.PreMailer.MoveCssInline(html, true, null, null, true, true);
                    result = inlineResult.Html;
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Error while inlining css...", ex);
                    result = html;
                }

                return result;
            });
        }

        public static Task CorrectScale(HtmlDocument htmlDocument)
        {
            return Task.Run(() =>
            {
                var headNode = htmlDocument.DocumentNode.SelectSingleNode("//head");
                if (headNode == null)
                    return;

                var existingViewportNodes = headNode.SelectNodes("/meta[@name='viewport']");
                if (existingViewportNodes != null)
                    foreach (var existingViewportNode in existingViewportNodes)
                        existingViewportNode.Remove();

                var viewportElement = htmlDocument.CreateElement("meta");
                viewportElement.SetAttributeValue("id", "viewport");
                viewportElement.SetAttributeValue("name", "viewport");
                viewportElement.SetAttributeValue("content", "initial-scale=1, minimum-scale=0.75, maximum-scale=1.25, user-scalable=yes");
                headNode.PrependChild(viewportElement);
            });
        }

        public static Task InjectFonts(HtmlDocument htmlDocument)
        {
            return Task.CompletedTask;

            //

            //Remember to change Build action in properties on all ttf files (list in fonts.css)
            //to AndroidAsset after uncommenting this code.

            //
            //return Task.Run(() =>
            //{
            //    var headNode = htmlDocument.DocumentNode.SelectSingleNode("//head");
            //    if (headNode == null)
            //        return;

            //    var cssLinkElement = htmlDocument.CreateElement("link");
            //    cssLinkElement.SetAttributeValue("id", "fonts");
            //    cssLinkElement.SetAttributeValue("rel", "stylesheet");
            //    cssLinkElement.SetAttributeValue("type", "text/css");
            //    cssLinkElement.SetAttributeValue("href", "file:///android_asset/fonts.css");
            //    headNode.PrependChild(cssLinkElement);
            //});
        }

        public static Task MakeEditable(HtmlDocument htmlDocument)
        {
            return Task.Run(() =>
            {
                var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");
                if (bodyNode == null)
                    return;

                bodyNode.SetAttributeValue("contentEditable", "true");
            });
        }

        public static Task InjectReplyHeader(Context ctx, HtmlDocument htmlDocument, string[] parameters)
        {
            return Task.Run(() =>
            {
                var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");
                if (bodyNode == null)
                    return;

                string replyHeader;
                using (var sr = new StreamReader(ctx.Assets.Open("replyHeader.html")))
                    replyHeader = sr.ReadToEnd();

                replyHeader = ProcessWebTemplate(replyHeader, parameters);
                var headerDiv = htmlDocument.CreateElement("div");
                headerDiv.SetAttributeValue("id", "replyHeader");
                headerDiv.InnerHtml = replyHeader;
                bodyNode.PrependChild(headerDiv);
            });
        }

        public static string ProcessWebTemplate(string template, params object[] args)
        {
            var output = template;
            for (var i = 0; i < args.Length; i++)
                output = output.Replace($"%%{i}%%", args[i].ToString());
            return output;
        }


        public static string[] GetReplyHeaderParameters(Context context, DocumentPreview documentPreview, Document document)
        {
            var from = GetAddressTextFromPreviousDocument(documentPreview, document, DocumentAddressType.From);
            var date = documentPreview.DateReceivedTimestamp
                                      .ConvertTimestampMillisecondsToDateTime()
                                      .ConvertUtcToUserTime()
                                      .ConvertDateTimeToTimestampMilliseconds()
                                      .FormatUserTimestampAsTimeAndDateString(context);
            var to = GetAddressTextFromPreviousDocument(documentPreview, document, DocumentAddressType.To, DocumentAddressType.Cc);
            var subject = documentPreview.Subject;

            return new[] { from, date, to, subject };
        }

        public static string GetAddressTextFromPreviousDocument(DocumentPreview documentPreview, Document document, params DocumentAddressType[] addressTypes)
        {
            if (documentPreview.Direction == DocumentDirection.Outgoing && addressTypes[0].Equals(DocumentAddressType.From))
            {

                var fromString = string.Empty;
                switch (ServerConfig.SystemSettings.DocumentsModuleInfo.UseForFrom)
                {
                    case UseForFrom.LicenseName:
                        fromString = ServerConfig.SystemSettings.SystemInfo.CustomerName;
                        break;
                    case UseForFrom.UserName:
                        fromString = ServerConfig.SystemSettings.UserInfo.User.FullName;
                        break;
                    case UseForFrom.UserLogin:
                        fromString = ServerConfig.SystemSettings.UserInfo.User.Username;
                        break;
                    case UseForFrom.LineName:
                        fromString = document.Lines.Select(l => l.FromAddress).FirstOrDefault();
                        break;
                }
                return fromString;
            }
            var sb = new StringBuilder();
            var addresses = documentPreview.Addresses.Where(da => addressTypes.Contains(da.AddressType)).ToArray();
            for (var i = 0; i < addresses.Length; i++)
            {
                var hasName = !string.IsNullOrWhiteSpace(addresses[i].Name);
                if (hasName)
                    sb.Append(addresses[i].Name).Append(" &lt;");
                sb.Append(addresses[i].Address);
                if (hasName)
                    sb.Append("&gt;");
                if (i < addresses.Length - 1)
                    sb.Append(", ");
            }

            return sb.ToString();
        }

        public static Task<string> MergeReplyWithPreviousDocument(Context context, string message, string previousContent)
        {
            return Task.Run(() =>
            {
                try
                {
                    var previousDocumentHtml = new HtmlDocument();
                    previousDocumentHtml.LoadHtml(previousContent);

                    string html;
                    using (var sr = new StreamReader(context.Assets.Open("blank.html")))
                        html = sr.ReadToEnd();

                    var mergedHtmlDocument = new HtmlDocument();
                    mergedHtmlDocument.LoadHtml(html);

                    var newNode = mergedHtmlDocument.DocumentNode.SelectSingleNode("//div[@id='new']");
                    newNode.InnerHtml = message ?? string.Empty;
                    newNode.Attributes.Remove("id");
                    var oldNode = mergedHtmlDocument.DocumentNode.SelectSingleNode("//div[@id='old']");
                    oldNode.AppendChildren(previousDocumentHtml.DocumentNode.SelectSingleNode("//body").ChildNodes);
                    oldNode.Attributes.Remove("id");

                    return mergedHtmlDocument.DocumentNode.OuterHtml;
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Error while merging previous content with reply message", ex);

                    return string.Empty;
                }
            });
        }


    }
}