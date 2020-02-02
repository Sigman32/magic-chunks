using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace MagicChunks.Helpers
{
    public static class HtmlExtensions
    {
        public static HtmlAgilityPack.HtmlNode FindChildByAttrFilterMatch(this HtmlAgilityPack.HtmlNode source, Match attributeFilterMatch)
        {
            var elementName = attributeFilterMatch.Groups["element"].Value;
            var attrName = attributeFilterMatch.Groups["key"].Value;
            var attrValue = attributeFilterMatch.Groups["value"].Value;

            var item = source?.GetChildElementByAttrValue(elementName, attrName, attrValue);

            if (item != null)
                return item;

            return source.CreateChildElement(elementName, attrName, attrValue);
        }

        public static HtmlAgilityPack.HtmlNode GetChildElementByAttrValue(this HtmlAgilityPack.HtmlNode source, string name, string attr, string attrValue)
        {
            var elements = source.ChildNodes
                .Where(e => String.Compare(e.OriginalName, name, StringComparison.InvariantCultureIgnoreCase) == 0);

            return elements
                .FirstOrDefault(e => e.Attributes.Any(a => (a.OriginalName == attr) && (a.Value == attrValue)));

        }

        public static HtmlAgilityPack.HtmlNode CreateChildElement(this HtmlAgilityPack.HtmlNode source, string elementName,
    string attrName = null, string attrValue = null)
        {
            var item = HtmlAgilityPack.HtmlNode.CreateNode($"<{elementName}/>");

            if (!String.IsNullOrWhiteSpace(attrName) && !String.IsNullOrWhiteSpace(attrValue))
            {
                item.SetAttributeValue(attrName, attrValue);
            }

            source.AppendChild(item);
            return item;
        }
    }
}
