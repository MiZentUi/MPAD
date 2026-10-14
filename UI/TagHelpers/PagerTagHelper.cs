using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace UI.TagHelpers;

[HtmlTargetElement("Pager")]
public class PagerTagHelper : TagHelper
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PagerTagHelper(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    [HtmlAttributeName("current-page")]
    public int CurrentPage { get; set; } = 1;

    [HtmlAttributeName("total-pages")]
    public int TotalPages { get; set; } = 1;

    [HtmlAttributeName("category")]
    public string? Category { get; set; }

    [HtmlAttributeName("service")]
    public string? Service
    {
        get => Category;
        set => Category = value;
    }

    [HtmlAttributeName("admin")]
    public bool Admin { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (TotalPages < 1)
        {
            output.SuppressOutput();
            return;
        }

        int current = Math.Clamp(CurrentPage, 1, TotalPages);
        int prev = current == 1 ? 1 : current - 1;
        int next = current == TotalPages ? TotalPages : current + 1;

        var nav = new TagBuilder("nav");
        nav.MergeAttribute("aria-label", "Pages");

        var ul = new TagBuilder("ul");
        ul.AddCssClass("pagination");
        ul.AddCssClass("justify-content-center");

        ul.InnerHtml.AppendHtml(CreatePageItem(prev, "«", "Previous", current == 1, false));
        for (int i = 1; i <= TotalPages; i++)
            ul.InnerHtml.AppendHtml(CreatePageItem(i, i.ToString(), null, false, i == current));
        ul.InnerHtml.AppendHtml(CreatePageItem(next, "»", "Next", current == TotalPages, false));

        nav.InnerHtml.AppendHtml(ul);

        output.TagName = null;
        output.Content.SetHtmlContent(nav);
    }

    private TagBuilder CreatePageItem(int pageNo, string text, string? ariaLabel, bool disabled, bool active)
    {
        var li = new TagBuilder("li");
        li.AddCssClass("page-item");
        if (disabled)
            li.AddCssClass("disabled");
        if (active)
            li.AddCssClass("active");

        var a = new TagBuilder("a");
        a.AddCssClass("page-link");
        a.MergeAttribute("href", disabled ? "#" : GetPageUrl(pageNo) ?? "#");
        if (ariaLabel is not null)
            a.MergeAttribute("aria-label", ariaLabel);
        a.InnerHtml.Append(text);

        li.InnerHtml.AppendHtml(a);
        return li;
    }

    private string? GetPageUrl(int pageNo)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return null;

        if (Admin)
        {
            return _linkGenerator.GetPathByPage(httpContext, values: new { pageNo });
        }

        string? filter = string.IsNullOrEmpty(Category) ? null : Category;
        return _linkGenerator.GetPathByAction(
            httpContext,
            action: "Index",
            controller: "Product",
            values: new { service = filter, pageNo });
    }
}
