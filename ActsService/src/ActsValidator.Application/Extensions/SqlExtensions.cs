using Dapper;

namespace ActsValidator.Application.Extensions
{
    public static class SqlExtensions
    {
        public static string ApplyPagination(
            int page,
            int pageSize,
            DynamicParameters parameters)
        {
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@Offset", (page - 1) * pageSize);

            return " LIMIT @PageSize OFFSET @Offset";
        }

        public static string ApplySorting(
            DynamicParameters parameters,
            string? orderBy = null,
            bool orderByDesc = false)
        {
            var orderDirection = orderByDesc ? "desc" : "asc";

            parameters.Add("@OrderBy", orderBy);
            parameters.Add("@Direction", orderDirection);

            return " ORDER BY @OrderBy @Direction";
        }
    }
}
