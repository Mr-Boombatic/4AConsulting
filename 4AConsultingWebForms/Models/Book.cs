using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using _4AConsultingWebForms.Services;

namespace _4AConsultingWebForms.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int? PublicationYear { get; set; }
        public string Description { get; set; }
        public XDocument TableOfContents { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string GetTableOfContentsXml()
        {
            if (TableOfContents == null)
            {
                return string.Empty;
            }

            var settings = new System.Xml.XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = false,
                ConformanceLevel = System.Xml.ConformanceLevel.Fragment
            };

            using (var stringWriter = new StringWriter())
            using (var xmlWriter = System.Xml.XmlWriter.Create(stringWriter, settings))
            {
                if (TableOfContents.Root != null)
                {
                    TableOfContents.Root.WriteTo(xmlWriter);
                }
                xmlWriter.Flush();
                return stringWriter.ToString();
            }
        }

        public void SetTableOfContentsXml(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                TableOfContents = null;
            }
            else
            {
                try
                {
                    TableOfContents = XDocument.Parse(xml);
                }
                catch
                {
                    TableOfContents = new XDocument(new XElement("TableOfContents"));
                }
            }
        }

        public void SetTableOfContentsHtml(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                TableOfContents = null;
                return;
            }

            try
            {
                string xmlResult = Services.XsltTransformService.TransformHtmlToXml(htmlContent);
                
                if (!string.IsNullOrWhiteSpace(xmlResult))
                {
                    try
                    {
                        TableOfContents = XDocument.Parse(xmlResult);
                        return;
                    }
                    catch (Exception ex)
                    {
                        Services.Logger.LogError("Error parsing XSLT-transformed XML, falling back to CDATA", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Services.Logger.LogError("Error transforming HTML to XML using XSLT, falling back to CDATA", ex);
            }

            var element = new XElement("TableOfContents", new XCData(htmlContent));
            TableOfContents = new XDocument(element);
        }

        public string GetTableOfContentsHtml()
        {
            if (TableOfContents == null)
            {
                return string.Empty;
            }

            try
            {
                string htmlResult = Services.XsltTransformService.TransformXmlToHtml(TableOfContents);
                
                if (!string.IsNullOrWhiteSpace(htmlResult))
                {
                    return htmlResult;
                }
            }
            catch (Exception ex)
            {
                Services.Logger.LogError("Error transforming XML to HTML using XSLT, falling back to direct extraction", ex);
            }

            try
            {
                var htmlElement = TableOfContents.Element("TableOfContents");
                if (htmlElement != null)
                {
                    return htmlElement.Value;
                }
            }
            catch
            {
            }

            return TableOfContents.ToString();
        }
    }
}
