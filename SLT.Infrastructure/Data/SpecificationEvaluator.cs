using Microsoft.EntityFrameworkCore;
using SLT.Core.Specifications;

namespace SLT.Infrastructure.Data;

public static class SpecificationEvaluator<T> where T : class
{
    public static IQueryable<T> GetQuery(
        IQueryable<T> inputQuery,
        ISpecification<T> specification)
    {
        var query = inputQuery;

        // Apply criteria (WHERE clause)
        if (specification.Criteria != null)
            query = query.Where(specification.Criteria);

        // Apply eager loading (Include)
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply string-based includes (ThenInclude chains)
        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy != null)
            query = query.OrderBy(specification.OrderBy);
        else if (specification.OrderByDescending != null)
            query = query.OrderByDescending(specification.OrderByDescending);

        // Apply paging
        if (specification.IsPagingEnabled)
            query = query.Skip(specification.Skip).Take(specification.Take);

        return query;
    }
}