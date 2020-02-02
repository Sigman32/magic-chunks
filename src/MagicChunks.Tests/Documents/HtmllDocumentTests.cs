using System;
using System.Text.RegularExpressions;
using MagicChunks.Documents;
using Xunit;

namespace MagicChunks.Tests.Documents
{
    public class HtmlDocumentTests
    {
        [Fact]
        public void Transform()
        {
            // Arrange

            var document = new HtmlDocument(@"<html>
<a>
 <x>1</x>
</a>
<b>2</b>
<c>3</c>
<e>
  <item key=""item1"">1</item>
  <item key=""item2"">2</item>
  <item key=""item3"">3</item>
</e>
<f>
  <item key=""item1"">
    <val>1</val>
  </item>
  <item key=""item2"">
    <val>2</val>
  </item>
  <item key=""item3"">
    <val>3</val>
  </item>
</f>
</html>");


            // Act

            document.ReplaceKey(new[] { "html", "a", "y"}, "2");
            document.ReplaceKey(new[] { "html", "a", "@y"}, "3");
            document.ReplaceKey(new[] { "html", "a", "z", "t", "w"}, "3");
            document.ReplaceKey(new[] { "html", "b"}, "5");
            document.ReplaceKey(new[] { "html", "c", "a"}, "1");
            document.ReplaceKey(new[] { "html", "c", "b"}, "2");
            document.ReplaceKey(new[] { "html", "c", "b", "t"}, "3");
            document.ReplaceKey(new[] { "html", "e", "item[@key = 'item2']" }, "5");
            document.ReplaceKey(new[] { "html", "e", "item[@key=\"item3\"]" }, "6");
            document.ReplaceKey(new[] { "html", "f", "item[@key = 'item2']", "val"}, "7");
            document.ReplaceKey(new[] { "html", "f", "item[@key=\"item3\"]", "val"}, "8");
            document.ReplaceKey(new[] { "html", "d"}, "4");

            var result = document.ToString();


            // Assert

            Assert.Equal(@"<html>
<a y=""3"">
 <x>1</x>
<y>2</y><z><t><w>3</w></t></z></a>
<b>5</b>
<c><a>1</a><b><t>3</t></b></c>
<e>
  <item key=""item1"">1</item>
  <item key=""item2"">5</item>
  <item key=""item3"">6</item>
</e>
<f>
  <item key=""item1"">
    <val>1</val>
  </item>
  <item key=""item2"">
    <val>7</val>
  </item>
  <item key=""item3"">
    <val>8</val>
  </item>
</f>
<d>4</d></html>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformWithNamesapce()
        {
            // Arrange

            var document = new HtmlDocument(@"<html xmlns=""http://www.w3.org/1999/xhtml"">
  <head></head>
  <BODY>
    <p></p>
    <DIV id=""d1""></DIV>
    <div id=""d2""><P></P></div>
  </BODY>
</html>");


            // Act

            document.ReplaceKey(new[] {"html", "body", "p"}, "Text");
            document.ReplaceKey(new[] {"html", "body", "div[@id='d1']"}, "Div");
            document.ReplaceKey(new[] {"html", "body", "div[@id='d2']", "p"}, "Text2");

            var result = document.ToString();


            // Assert

            Assert.Equal(@"<html xmlns=""http://www.w3.org/1999/xhtml"">
  <head></head>
  <BODY>
    <p>Text</p>
    <DIV id=""d1"">Div</DIV>
    <div id=""d2""><P>Text2</P></div>
  </BODY>
</html>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void Remove()
        {
            // Arrange

            var document = new HtmlDocument(@"<html>
    <a>
      <x>1</x>
    </a>
    <b>
      <x>1</x>
    </b>
    <c key=""item1"" foo=""bar"">3</c>
</html>");


            // Act

            document.RemoveKey(new[] { "html", "a"});
            document.RemoveKey(new[] { "html", "b", "x"});
            document.RemoveKey(new[] { "html", "c", "@key"});

            var result = document.ToString();


            // Assert

            Assert.Equal(@"<html>
    
    <b>
      
    </b>
    <c foo=""bar"">3</c>
</html>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void ValidateEmptyPath()
        {
            // Arrange
            HtmlDocument document = new HtmlDocument("<html/>");

            // Act
            ArgumentException result = Assert.Throws<ArgumentException>(() => document.ReplaceKey(new[] { "a", "", "b" }, ""));

            // Assert
            Assert.True(result.Message?.StartsWith("There is empty items in the path."));
        }

        [Fact]
        public void ValidateWhiteSpacePath()
        {
            // Arrange
            HtmlDocument document = new HtmlDocument("<html/>");

            // Act
            ArgumentException result = Assert.Throws<ArgumentException>(() => document.ReplaceKey(new[] { "a", "   ", "b" }, ""));

            // Assert
            Assert.True(result.Message?.StartsWith("There is empty items in the path."));
        }

        [Fact]
        public void ValidateAmpersandsEscaping()
        {
            // Arrange
            HtmlDocument document = new HtmlDocument(@"<html>
  <connectionStrings>
    <add name=""Connection"" connectionString="""" />
  </connectionStrings>
</html>");

            // Act
            document.ReplaceKey(new [] { "html", "connectionStrings", "add[@name=\"Connection\"]", "@connectionString" }, @"metadata=res://*/Model.csdl|res://*/Model.ssdl|res://*/Model.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=other-server\instance;initial catalog=database;integrated security=True;multipleactiveresultsets=True;&quot;");

            var result = document.ToString();

            // Assert
            Assert.Equal(@"<html>
  <connectionStrings>
    <add name=""Connection"" connectionString=""metadata=res://*/Model.csdl|res://*/Model.ssdl|res://*/Model.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=other-server\instance;initial catalog=database;integrated security=True;multipleactiveresultsets=True;&quot;""></add>
  </connectionStrings>
</html>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }


        [Fact]
        public void ValidateDoubleColumn()
        {
            // Arrange
            HtmlDocument document = new HtmlDocument(@"<html>
  <connectionStrings>
    <add name=""api:Connection"" connectionString="""" />
  </connectionStrings>
</html>");

            // Act
            document.ReplaceKey(new[] { "html", "connectionStrings", "add[@name='api:Connection']", "@connectionString" }, @"metadata=res://*/Model.csdl|res://*/Model.ssdl|res://*/Model.msl;provider=System.Data.SqlClient;provider connection string=""data source=other-server\instance;initial catalog=database;integrated security=True;multipleactiveresultsets=True;""");

            var result = document.ToString();

            // Assert1
            Assert.Equal(@"<html>
  <connectionstrings>
    <add name=""api:Connection"" connectionstring=""metadata=res://*/Model.csdl|res://*/Model.ssdl|res://*/Model.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=other-server\instance;initial catalog=database;integrated security=True;multipleactiveresultsets=True;&quot;""></add>
  </connectionstrings>
</html>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }
    }
}