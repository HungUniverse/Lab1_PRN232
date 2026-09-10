using System.Linq.Expressions;

namespace PRN232.Lab1.Service.Base;

public sealed record SortTerm(string Field, bool Descending);

public sealed class QueryOptions
{
    public int Page { get; init; }
    public int Size { get; init; }
    public string? Search { get; init; }
    public IReadOnlyCollection<SortTerm> SortTerms { get; init; } = Array.Empty<SortTerm>();
    public HashSet<string> Fields { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> Expansions { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public static class QueryOptionsParser
{
    public static bool TryParse(
        ListQueryRequest request,
        IEnumerable<string> allowedFields,
        IEnumerable<string> allowedSorts,
        IEnumerable<string> allowedExpansions,
        out QueryOptions options,
        out string[] errors)
    {
        var validationErrors = new List<string>();
        var fieldSet = ParseCsvValues(request.Fields).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var expansionSet = ParseCsvValues(request.Expand).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var allowedFieldSet = new HashSet<string>(allowedFields, StringComparer.OrdinalIgnoreCase);
        var allowedSortSet = new HashSet<string>(allowedSorts, StringComparer.OrdinalIgnoreCase);
        var allowedExpansionSet = new HashSet<string>(allowedExpansions, StringComparer.OrdinalIgnoreCase);

        if (request.Page < 1)
            validationErrors.Add("page must be greater than or equal to 1.");
        if (request.Size is < 1 or > 100)
            validationErrors.Add("size must be between 1 and 100.");

        foreach (var field in fieldSet.Where(field => !allowedFieldSet.Contains(field)))
            validationErrors.Add($"Unsupported field '{field}'.");

        foreach (var expansion in expansionSet.Where(expansion => !allowedExpansionSet.Contains(expansion)))
            validationErrors.Add($"Unsupported expansion '{expansion}'.");

        var sortTerms = new List<SortTerm>();
        foreach (var rawSort in ParseCsvValues(request.Sort))
        {
            var descending = rawSort.StartsWith('-');
            var field = rawSort.TrimStart('-', '+');
            if (string.IsNullOrWhiteSpace(field) || !allowedSortSet.Contains(field))
            {
                validationErrors.Add($"Unsupported sort field '{field}'.");
                continue;
            }

            sortTerms.Add(new SortTerm(field, descending));
        }

        options = new QueryOptions
        {
            Page = request.Page,
            Size = request.Size,
            Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim(),
            Fields = fieldSet,
            Expansions = expansionSet,
            SortTerms = sortTerms
        };
        errors = validationErrors.ToArray();
        return errors.Length == 0;
    }

    private static IEnumerable<string> ParseCsvValues(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
}

public static class QueryOrdering
{
    public static IOrderedQueryable<T> Apply<T, TKey>(
        IQueryable<T> query,
        IOrderedQueryable<T>? orderedQuery,
        Expression<Func<T, TKey>> keySelector,
        bool descending)
    {
        if (orderedQuery is null)
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

        return descending
            ? orderedQuery.ThenByDescending(keySelector)
            : orderedQuery.ThenBy(keySelector);
    }
}
