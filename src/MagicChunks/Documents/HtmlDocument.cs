using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using MagicChunks.Core;
using MagicChunks.Helpers;

namespace MagicChunks.Documents
{
    public class HtmlDocument : IDocument
    {
        private static readonly Regex AttributeFilterRegex = new Regex(@"(?<element>.+?)\[\s*\@(?<key>[\w\:]+)\s*\=\s*[\'\""]?(?<value>.+?)[\'\""]?\s*\]$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        protected readonly HtmlAgilityPack.HtmlDocument Document;

        public HtmlDocument(string source)
        {
            try
            {
                Document = new HtmlAgilityPack.HtmlDocument();
                Document.LoadHtml(source);
                if (String.Compare(Document.DocumentNode.FirstChild.OriginalName, "html", StringComparison.InvariantCultureIgnoreCase) != 0)
                    throw new ArgumentException("Only html files supported", nameof(source));
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Wrong document format", nameof(source), ex);
            }
        }

        public void ReplaceKey(string[] path, string value)
        {
            if ((path == null) || (path.Any() == false))
                throw new ArgumentException("Path is not speicified.", nameof(path));

            if (path.Any(String.IsNullOrWhiteSpace))
                throw new ArgumentException("There is empty items in the path.", nameof(path));

            HtmlAgilityPack.HtmlNode current = Document.DocumentNode.FirstChild;
            //string documentNamespace = Document.DocumentNode?.Name.NamespaceName ?? String.Empty;

            if (current == null)
                throw new ArgumentException("Root element is not present.", nameof(path));

            if (String.Compare(current.OriginalName, path.First(), StringComparison.InvariantCultureIgnoreCase) != 0)
                throw new ArgumentException("Root element name does not match path.", nameof(path));

            current = FindPath(path.Skip(1).Take(path.Length - 2), current);

            UpdateTargetElement(value, path.Last(), current);
        }

        public void RemoveKey(string[] path)
        {
            if ((path == null) || (!path.Any()))
                throw new ArgumentException("Path is not specified.", nameof(path));

            if (path.Any(String.IsNullOrWhiteSpace))
                throw new ArgumentException("There is empty items in the path.", nameof(path));

            HtmlAgilityPack.HtmlNode current = Document.DocumentNode.FirstChild;

            if (current == null)
                throw new ArgumentException("Root element is not present.", nameof(path));

            if (String.Compare(current.OriginalName, path.First(), StringComparison.InvariantCultureIgnoreCase) != 0)
                throw new ArgumentException("Root element name does not match path.", nameof(path));

            current = FindPath(path.Skip(1).Take(path.Length - 2), current);

            RemoveTargetElement(path.Last(), current);
        }

        private static HtmlAgilityPack.HtmlNode FindPath(IEnumerable<string> path, HtmlAgilityPack.HtmlNode current)
        {
            foreach (string pathElement in path)
            {
                if (pathElement.StartsWith("@"))
                    throw new ArgumentException("Attribute element could be only at end of the path.", nameof(path));

                var attributeFilterMatch = AttributeFilterRegex.Match(pathElement);

                var currentElement = current?.ChildNodes.FirstOrDefault(e => String.Compare(e.OriginalName, pathElement, StringComparison.InvariantCultureIgnoreCase) == 0);

                if (attributeFilterMatch.Success)
                {
                    current = current.FindChildByAttrFilterMatch(attributeFilterMatch);
                }
                else if (currentElement != null)
                {
                    current = currentElement;
                }
                else
                {
                    if (!current.ChildNodes.Any())
                        current.InnerHtml = "";

                    current = current?.CreateChildElement(pathElement);
                }
            }
            return current;
        }

        private static void UpdateTargetElement(string value, string targetElement, HtmlAgilityPack.HtmlNode current)
        {
            var attributeFilterMatch = AttributeFilterRegex.Match(targetElement);

            if (targetElement.StartsWith("@"))
            {   // Attriubte update
                current.SetAttributeValue(targetElement.TrimStart('@'), value.Replace("&quot;", @"""").Replace("&lt;", @"<").Replace("&gt;", @">"));
            }
            else if (!attributeFilterMatch.Success)
            {   // Property update
                var elementToUpdate = current.ChildNodes.FirstOrDefault(e => String.Compare(e.OriginalName, targetElement, StringComparison.InvariantCultureIgnoreCase) == 0);

                if (elementToUpdate == null)
                {
                    if (!current.ChildNodes.Any(t=>t.NodeType == HtmlAgilityPack.HtmlNodeType.Element))
                        current.InnerHtml = "";
                    elementToUpdate = current.CreateChildElement(targetElement);
                }

                elementToUpdate.InnerHtml = value;
            }
            else
            {   // Filtered element update
                current = current.FindChildByAttrFilterMatch(attributeFilterMatch);
                current.InnerHtml = value;
            }
        }

        private static void RemoveTargetElement(string targetElement, HtmlAgilityPack.HtmlNode current)
        {
            var attributeFilterMatch = AttributeFilterRegex.Match(targetElement);

            if (targetElement.StartsWith("@"))
            {   // Attriubte update
                current.Attributes.FirstOrDefault(x => x.Name == targetElement.TrimStart('@'))
                    ?.Remove();
            }
            else if (!attributeFilterMatch.Success)
            {   // Property update
                var elementToRemove = current.ChildNodes.FirstOrDefault(e => String.Compare(e.OriginalName, targetElement, StringComparison.InvariantCultureIgnoreCase) == 0);
                elementToRemove.Remove();
            }
            else
            {   // Filtered element update
                current.FindChildByAttrFilterMatch(attributeFilterMatch).Remove();
            }
        }

        public override string ToString()
        {
            string result = null;

            if (Document != null)
            {
                var sb = new StringBuilder();
                using(var htmlWritter = new StringWriter(sb))
                {
                    Document.Save(htmlWritter);
                }
                result = sb.ToString();
            }

            return result ?? string.Empty;
        }

        public void Dispose()
        {
        }
    }
}