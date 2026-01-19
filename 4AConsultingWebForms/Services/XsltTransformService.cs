using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace _4AConsultingWebForms.Services
{
    public static class XsltTransformService
    {
        private static readonly object _lockObject = new object();
        private static XslCompiledTransform _htmlToXmlTransform;
        private static XslCompiledTransform _xmlToHtmlTransform;

        public static string TransformHtmlToXml(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                return string.Empty;
            }

            try
            {
                XDocument xhtmlDoc = ConvertHtmlToXDocument(htmlContent);
                
                if (_htmlToXmlTransform == null)
                {
                    lock (_lockObject)
                    {
                        if (_htmlToXmlTransform == null)
                        {
                            _htmlToXmlTransform = LoadXsltTemplate("HtmlToXml.xslt");
                        }
                    }
                }

                using (var stringWriter = new StringWriter())
                using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
                {
                    OmitXmlDeclaration = false,
                    Indent = true,
                    Encoding = Encoding.UTF8
                }))
                {
                    _htmlToXmlTransform.Transform(xhtmlDoc.CreateReader(), xmlWriter);
                    xmlWriter.Flush();
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error transforming HTML to XML using XSLT", ex);
                return null;
            }
        }

        public static string TransformXmlToHtml(XDocument xml)
        {
            if (xml == null || xml.Root == null)
            {
                return string.Empty;
            }

            try
            {
                if (_xmlToHtmlTransform == null)
                {
                    lock (_lockObject)
                    {
                        if (_xmlToHtmlTransform == null)
                        {
                            _xmlToHtmlTransform = LoadXsltTemplate("XmlToHtml.xslt");
                        }
                    }
                }

                using (var stringWriter = new StringWriter())
                {
                    _xmlToHtmlTransform.Transform(xml.CreateReader(), null, stringWriter);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error transforming XML to HTML using XSLT", ex);
                return null;
            }
        }

        private static XslCompiledTransform LoadXsltTemplate(string templateName)
        {
            string templatePath = GetTemplatePath(templateName);
            
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"XSLT template not found: {templatePath}");
            }

            var transform = new XslCompiledTransform();
            var settings = new XsltSettings(true, true);
            
            using (var reader = XmlReader.Create(templatePath))
            {
                transform.Load(reader, settings, new XmlUrlResolver());
            }

            return transform;
        }

        private static string GetTemplatePath(string templateName)
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Server.MapPath($"~/App_Data/XsltTemplates/{templateName}");
            }
            
            string path = HostingEnvironment.MapPath($"~/App_Data/XsltTemplates/{templateName}");
            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "XsltTemplates", templateName);
            }
            
            return path;
        }

        private static XDocument ConvertHtmlToXDocument(string htmlContent)
        {
            try
            {
                string cleanedHtml = htmlContent.Trim();
                
                if (string.IsNullOrWhiteSpace(cleanedHtml))
                {
                    return new XDocument(new XElement("html",
                        new XElement("head"),
                        new XElement("body")));
                }

                var body = new XElement("body");
                
                try
                {
                    using (var reader = new StringReader(cleanedHtml))
                    {
                        var settings = new XmlReaderSettings
                        {
                            ConformanceLevel = ConformanceLevel.Fragment,
                            DtdProcessing = DtdProcessing.Ignore,
                            CheckCharacters = false,
                            IgnoreWhitespace = false
                        };
                        
                        using (var xmlReader = XmlReader.Create(reader, settings))
                        {
                            while (xmlReader.Read())
                            {
                                if (xmlReader.NodeType == XmlNodeType.Element)
                                {
                                    body.Add(XNode.ReadFrom(xmlReader));
                                }
                                else if (xmlReader.NodeType == XmlNodeType.Text)
                                {
                                    string text = xmlReader.Value;
                                    if (!string.IsNullOrWhiteSpace(text))
                                    {
                                        body.Add(new XText(text));
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    try
                    {
                        var fragment = XElement.Parse($"<fragment>{cleanedHtml}</fragment>", LoadOptions.PreserveWhitespace);
                        body.Add(fragment.Nodes());
                    }
                    catch
                    {
                        try
                        {
                            string wrappedHtml = $"<html><head></head><body>{cleanedHtml}</body></html>";
                            var doc = XDocument.Parse(wrappedHtml, LoadOptions.PreserveWhitespace);
                            var bodyElement = doc.Root?.Element("body");
                            if (bodyElement != null)
                            {
                                body.Add(bodyElement.Nodes());
                            }
                        }
                        catch
                        {
                            body.Add(new XCData(cleanedHtml));
                        }
                    }
                }
                
                return new XDocument(
                    new XElement("html",
                        new XElement("head"),
                        body));
            }
            catch (Exception ex)
            {
                Logger.LogError("Error converting HTML to XDocument", ex);
                var body = new XElement("body", new XCData(htmlContent ?? string.Empty));
                return new XDocument(
                    new XElement("html",
                        new XElement("head"),
                        body));
            }
        }
    }
}
