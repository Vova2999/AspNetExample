using System.Linq.Expressions;
using System.Text;
using AspNetExample.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

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

    public static HtmlString DisplayPagination<TModel>(
        this IHtmlHelper<TModel> htmlHelper,
        SortingPaginationModelBase model)
    {
        var stringBuilder = new StringBuilder();
        var pageValue = model.Page ?? Constants.FirstPage;
        var totalCountValue = model.TotalCount ?? 0;
        var totalPagesValue = model.TotalPages ?? Constants.FirstPage;

        stringBuilder
            .Append("<div style=\"margin-top: 8px;\">")
            .Append("Количество: ")
            .Append(totalCountValue)
            .Append("</div>")
            .AppendLine();
        stringBuilder
            .Append("<div style=\"margin-top: 4px;\">")
            .AppendLine();
        stringBuilder
            .Append("<a onclick=\"changePage(")
            .Append(pageValue - 1)
            .Append(");\" class=\"btn btn-sm btn-outline-primary")
            .Append(model.HasPrevPage ? string.Empty : " disabled")
            .Append("\"><i class=\"fa fa-chevron-left\"></i> Назад</a>")
            .AppendLine();
        stringBuilder
            .Append("Страница ")
            .Append(pageValue)
            .Append(" из ")
            .Append(totalPagesValue)
            .AppendLine();
        stringBuilder
            .Append("<a onclick=\"changePage(")
            .Append(pageValue + 1)
            .Append(");\" class=\"btn btn-sm btn-outline-primary")
            .Append(model.HasNextPage ? string.Empty : " disabled")
            .Append("\">Вперед <i class=\"fa fa-chevron-right\"></i></a>")
            .AppendLine();
        stringBuilder
            .Append("</div>")
            .AppendLine();

        return new HtmlString(stringBuilder.ToString());
    }
}