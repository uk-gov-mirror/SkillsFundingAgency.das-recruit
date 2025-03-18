using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(Attributes = "asp-fragment")]
public class FragmentAppenderTagHelper: TagHelper
{
    public override int Order => int.MaxValue;
    public string AspFragment { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(AspFragment))
        {
            return;
        }

        if (!output.Attributes.TryGetAttribute("href", out var attr))
        {
            return;
        }

        output.Attributes.SetAttribute("href", $"{attr.Value}#{AspFragment}");
    }
}