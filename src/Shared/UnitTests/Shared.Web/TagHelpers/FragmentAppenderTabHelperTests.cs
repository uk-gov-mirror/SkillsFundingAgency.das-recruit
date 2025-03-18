using System.Collections.Generic;
using System.Text.Encodings.Web;
using Esfa.Recruit.Shared.Web.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.TagHelpers;

public class FragmentAppenderTabHelperTests
{
    private TagHelperContext _tagHelperContext;
    private TagHelperOutput _tagHelperOutput;
    private const string Url = "https://www.findapprenticeship.service.gov.uk/apprenticeships";
    private const string UrlFormat = "<a href=\"HtmlEncode[[{0}]]\"></a>";
    
    private static Task<TagHelperContent> Func(bool result, HtmlEncoder encoder)
    {
        var tagHelperContent = new DefaultTagHelperContent();
        tagHelperContent.SetHtmlContent(string.Empty);
        return Task.FromResult<TagHelperContent>(tagHelperContent);
    }
    
    [SetUp]
    public void SetUp()
    {
        _tagHelperContext = new TagHelperContext([], new Dictionary<object, object>(), "id");
        _tagHelperOutput = new TagHelperOutput("a", [new TagHelperAttribute("href", Url)], Func);
    }
    
    [TestCase("")]
    [TestCase(null)]
    public async Task Output_Is_Unaffected_When_No_Fragment_Specified(string fragment)
    {
        // arrange
        var sut = new FragmentAppenderTagHelper
        {
            AspFragment = fragment
        };

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().Be(string.Format(UrlFormat, Url));
    }
    
    [Test]
    public async Task Output_Is_Unaffected_When_No_Href_Attribute_Exists()
    {
        // arrange
        var tagHelperOutput = new TagHelperOutput("a", [], Func);
        var sut = new FragmentAppenderTagHelper
        {
            AspFragment = "something"
        };

        // act
        await sut.ProcessAsync(_tagHelperContext, tagHelperOutput);

        // assert
        tagHelperOutput.AsString().Should().Be("<a></a>");
    }
    
    [Test]
    public async Task Output_Has_Fragment_Appended()
    {
        // arrange
        var sut = new FragmentAppenderTagHelper
        {
            AspFragment = "something"
        };

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().Be(string.Format(UrlFormat, $"{Url}#something"));
    }
}