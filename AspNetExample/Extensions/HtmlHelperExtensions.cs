using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;
using System.Text;

namespace AspNetExample.Extensions;

public static class HtmlHelperExtensions
{
    public static HtmlString DisplaySortColumnTitle<TModel, TResult>(
        this IHtmlHelper<TModel> htmlHelper,
        string? currentSortBy,
        string sortPropertyName,
        Expression<Func<TModel, TResult>> expression)
    {
        var stringBuilder = new StringBuilder();
        var sortDescPropertyName = $"{sortPropertyName}{Constants.DescSuffix}";

        var nextSortPropertyName =
            currentSortBy == sortPropertyName
                ? sortDescPropertyName
                : sortPropertyName;

        stringBuilder
            .Append("<a onclick=\"changeSortBy('")
            .Append(nextSortPropertyName)
            .Append("');\" class=\"link-dark\">")
            .Append(htmlHelper.DisplayNameFor(expression))
            .Append("</a>");

        if (currentSortBy == sortPropertyName)
            stringBuilder.Append(" <i class=\"fa fa-sort-asc\" style=\"vertical-align: middle; margin-bottom: -5px;\"></i>");
        else if (currentSortBy == sortDescPropertyName)
            stringBuilder.Append(" <i class=\"fa fa-sort-desc\" style=\"vertical-align: middle; margin-top: -5px;\"></i>");

        return new HtmlString(stringBuilder.ToString());
    }
}