using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DiscountManager.Api.Extensions;

public static class LinqExtensions
{
    /// <summary>
    /// Filters a <see cref="IQueryable{T}"/> by a given predicate if given condition is true.
    /// </summary>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }

    /// <summary>
    /// Filters a <see cref="IEnumerable{T}"/> by a given predicate if given condition is true.
    /// </summary>
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> query, bool condition, Func<T, bool> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }

    /// <summary>
    /// Used for paging. Can be used as an alternative to Skip(...).Take(...)
    /// </summary>
    public static IQueryable<T> PageBy<T>(this IQueryable<T> query, int skipCount, int maxResultCount)
    {
        return query.Skip(skipCount).Take(maxResultCount);
    }

    /// <summary>
    /// Used for paging. Can be used as an alternative to Skip(...).Take(...)
    /// </summary>
    public static IEnumerable<T> PageBy<T>(this IEnumerable<T> query, int skipCount, int maxResultCount)
    {
        return query.Skip(skipCount).Take(maxResultCount);
    }

    /// <summary>
    /// Specifies the related objects to include in the query results.
    /// </summary>
    public static IQueryable<T> IncludeIf<T, TProperty>(this IQueryable<T> query, bool condition, Expression<Func<T, TProperty>> navigationPropertyPath)
        where T : class
    {
        return condition ? query.Include(navigationPropertyPath) : query;
    }

    /// <summary>
    /// Sorts the elements of a sequence according to a key if given condition is true.
    /// </summary>
    public static IOrderedQueryable<T> OrderByIf<T, TKey>(this IQueryable<T> query, bool condition, Expression<Func<T, TKey>> keySelector)
    {
        return condition ? query.OrderBy(keySelector) : (IOrderedQueryable<T>)query;
    }

    /// <summary>
    /// Sorts the elements of a sequence in descending order according to a key if given condition is true.
    /// </summary>
    public static IOrderedQueryable<T> OrderByDescendingIf<T, TKey>(this IQueryable<T> query, bool condition, Expression<Func<T, TKey>> keySelector)
    {
        return condition ? query.OrderByDescending(keySelector) : (IOrderedQueryable<T>)query;
    }

    /// <summary>
    /// If the condition is true, ignores global query filters.
    /// </summary>
    public static IQueryable<T> IgnoreQueryFiltersIf<T>(this IQueryable<T> query, bool condition) where T : class
    {
        return condition ? query.IgnoreQueryFilters() : query;
    }

    /// <summary>
    /// Adds a "AsNoTracking" to the query if the condition is true.
    /// </summary>
    public static IQueryable<T> AsNoTrackingIf<T>(this IQueryable<T> query, bool condition) where T : class
    {
        return condition ? query.AsNoTracking() : query;
    }
}
