using Ganss.XSS;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace Teknik.Utilities
{
    public static class HtmlHelper
    {
        /// <summary>
        /// Transforms a string of Markdown into HTML.
        /// </summary>
        /// <param name="helper">HtmlHelper - Not used, but required to make this an extension method.</param>
        /// <param name="text">The Markdown that should be transformed.</param>
        /// <returns>The HTML representation of the supplied Markdown.</returns>
        public static HtmlString Markdown(this IHtmlHelper helper, string text)
        {
            // Just call the other one, to avoid having two copies (we don't use the HtmlHelper).
            return MarkdownHelper.Markdown(text);
        }

        /// <summary>
        /// Sanitizes a string from HTML.
        /// </summary>
        /// <param name="helper">HtmlHelper - Not used, but required to make this an extension method.</param>
        /// <param name="text">The Markdown that should be transformed.</param>
        /// <returns>The HTML representation of the supplied Markdown.</returns>
        public static HtmlString Sanitize(this IHtmlHelper helper, string text)
        {
            // Sanitize Text
            var sanitizer = new HtmlSanitizer();
            var sanText = sanitizer.Sanitize(text);

            // Wrap the html in an MvcHtmlString otherwise it'll be HtmlEncoded and displayed to the user as HTML :(
            return new HtmlString(sanText);
        }
    }
}
