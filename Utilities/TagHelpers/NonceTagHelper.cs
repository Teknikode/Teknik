using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Teknik.Utilities.TagHelpers
{
    [HtmlTargetElement("script")]
    public class NonceTagHelper : TagHelper
    {
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.SetAttribute("nonce", ViewContext.HttpContext.Items[Constants.NONCE_KEY]);
        }
    }
}
