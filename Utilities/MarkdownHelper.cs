using Ganss.XSS;
using Markdig;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Teknik.Utilities
{
    /// <summary>
    /// Helper class for transforming Markdown.
    /// </summary>
    public static partial class MarkdownHelper
	{
		/// <summary>
		/// Transforms a string of Markdown into HTML.
		/// </summary>
		/// <param name="text">The Markdown that should be transformed.</param>
		/// <returns>The HTML representation of the supplied Markdown.</returns>
		public static HtmlString Markdown(string text)
        {
            // Sanitize Text
            var sanitizer = new HtmlSanitizer();
            var sanText = sanitizer.Sanitize(text);

            // Transform the supplied text (Markdown) into HTML.
            string html = Markdig.Markdown.ToHtml(sanText, BuildPipeline());

			// Wrap the html in an MvcHtmlString otherwise it'll be HtmlEncoded and displayed to the user as HTML :(
			return new HtmlString(html);
		}

        public static MarkdownPipeline BuildPipeline()
        {
            return new MarkdownPipelineBuilder()  // Use similar to advanced extension without auto-identifier
                .UseAbbreviations()
                .UseAutoIdentifiers()
                .UseCitations()
                .UseCustomContainers()
                .UseDefinitionLists()
                .UseEmphasisExtras()
                .UseFigures()
                .UseFooters()
                .UseFootnotes()
                .UseGridTables()
                .UseMathematics()
                .UseMediaLinks()
                .UsePipeTables()
                .UseListExtras()
                .UseGenericAttributes().Build();
        }
	}
}