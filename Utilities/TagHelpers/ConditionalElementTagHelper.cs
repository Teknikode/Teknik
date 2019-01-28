using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Teknik.Utilities.TagHelpers
{
    [HtmlTargetElement(Attributes = "conditional")]
    public class ConditionalElementTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-condition")]
        public bool Condition { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            output.Attributes.RemoveAll("conditional");

            if (!Condition)
            {
                output.TagName = string.Empty;
            }
        }
    }
}
