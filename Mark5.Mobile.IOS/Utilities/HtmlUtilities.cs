using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using Foundation;
using HtmlAgilityPack;
using MailBee.Html;
using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Model;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class HtmlUtilities
    {
        public static async Task<string> ProcessHtml(string html, HtmlProcessingConfiguration config)
        {
            var sw = Stopwatch.StartNew();

            if (config.MakeHtmlSafe)
            {
                html = await MakeHtmlSafe(html);

                CommonConfig.Logger.Debug($"MakeHtmlSafe {sw.ElapsedMilliseconds}ms");
                sw.Restart();
            }

            if (config.MakeHtmlKindaSafe)
            {
                html = await MakeHtmlKindaSafe(html);

                CommonConfig.Logger.Debug($"MakeHtmlKindaSafe {sw.ElapsedMilliseconds}ms");
                sw.Restart();
            }

            if (config.InlineCss)
            {
                html = await InlineCss(html);

                CommonConfig.Logger.Debug($"InlineCss {sw.ElapsedMilliseconds}ms");
                sw.Restart();
            }

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            CommonConfig.Logger.Debug($"LoadHtml {sw.ElapsedMilliseconds}ms");
            sw.Restart();

            if (config.CorrectScale)
            {
                await CorrectScale(htmlDocument);

                CommonConfig.Logger.Debug($"CorrectScale {sw.ElapsedMilliseconds}ms");
                sw.Restart();
            }

            if (config.InjectFonts)
            {
                await InjectFonts(htmlDocument);

                CommonConfig.Logger.Debug($"InjectFonts {sw.ElapsedMilliseconds}ms");
                sw.Restart();
            }

            if (config.MakeEditable)
            {
                await MakeEditable(htmlDocument);

                CommonConfig.Logger.Debug($"MakeEditable {sw.ElapsedMilliseconds}ms");
                sw.Restart();
            }

            if (config.InjectReplyHeader)
            {
                await InjectReplyHeader(htmlDocument, config.ReplyHeaderParameters);

                CommonConfig.Logger.Debug($"InjectReplyHeader {sw.ElapsedMilliseconds}ms");
                sw.Restart();
            }

            await InjectOverflowCorrection(htmlDocument);

            sw.Stop();

            return htmlDocument.DocumentNode.OuterHtml;
        }

        public static async Task<string> ProcessPlainText(string text, PlainTextProcessingConfiguration config)
        {
            if (config.Encode)
                text = HttpUtility.HtmlEncode(text);

            var html = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/plain", "html"));
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var preNode = htmlDocument.DocumentNode.SelectSingleNode("//pre[@id='plaintext']");
            preNode.InnerHtml = text;

            if (config.MakeEditable)
                await MakeEditable(htmlDocument);

            if (config.InjectReplyHeader)
                await InjectReplyHeader(htmlDocument, config.ReplyHeaderParameters);

            return htmlDocument.DocumentNode.OuterHtml;
        }

        public static Task InjectOverflowCorrection(HtmlDocument html)
        {
            return Task.Run(() =>
            {
                var htmlNode = html.DocumentNode.SelectSingleNode("//html");
                if (htmlNode == null)
                    return;
                htmlNode.SetAttributeValue("style", "overflow:auto;");
            });
        }

        static Task<string> MakeHtmlSafe(string html)
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

        static Task<string> MakeHtmlKindaSafe(string html)
        {
            return Task.Run(() =>
            {
                var htmlDocument = new HtmlDocument();
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

        static Task<string> InlineCss(string html)
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

        static Task CorrectScale(HtmlDocument htmlDocument)
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
                viewportElement.SetAttributeValue("content", "initial-scale=0.8, minimum-scale=0.75, maximum-scale=1.25, user-scalable=yes");

                headNode.PrependChild(viewportElement);
            });
        }

        static Task InjectFonts(HtmlDocument htmlDocument)
        {
            return Task.CompletedTask;

            //

            //Remember to change Build action in properties on all ttf files (list in fonts.css)
            //to BundleResource after uncommenting this code.

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
            //    cssLinkElement.SetAttributeValue("href", "html/fonts.css");
            //    headNode.PrependChild(cssLinkElement);
            //});
        }

        static Task MakeEditable(HtmlDocument htmlDocument)
        {
            return Task.Run(() =>
            {
                var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");
                if (bodyNode == null)
                    return;

                bodyNode.SetAttributeValue("contentEditable", "true");
            });
        }

        static Task InjectReplyHeader(HtmlDocument htmlDocument, string[] parameters)
        {
            return Task.Run(() =>
            {
                var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");
                if (bodyNode == null)
                    return;

                var replyHeader = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/replyHeader", "html"));
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
    }
}
