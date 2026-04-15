using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.QueryFinancialReportsByFields
{
    public class QueryFinancialReportsByFieldsQueryHandler(
        IUnitOfWork uow,
        ILogger<QueryFinancialReportsByFieldsQueryHandler> logger)
        : IRequestHandler<QueryFinancialReportsByFieldsQuery, ApiResponse<PaginatedResponse<FinancialReportFieldQueryItemDto>>>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<QueryFinancialReportsByFieldsQueryHandler> _logger = logger;

        public async Task<ApiResponse<PaginatedResponse<FinancialReportFieldQueryItemDto>>> Handle(
            QueryFinancialReportsByFieldsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var reports = await _uow.FinancialReports.GetForFieldQueryAsync(
                    string.IsNullOrWhiteSpace(request.Ticker) ? null : request.Ticker.Trim().ToUpperInvariant(),
                    request.YearFrom,
                    request.YearTo,
                    request.Period.HasValue ? (int)request.Period.Value : null,
                    request.Status.HasValue ? (int)request.Status.Value : null,
                    request.MaxScanRecords,
                    cancellationToken);

                var contexts = reports
                    .Select(report => new FinancialReportFieldContext(report))
                    .ToList();

                if (request.Filters.Count > 0)
                {
                    contexts = contexts
                        .Where(context => request.Filters.All(filter => MatchFilter(context, filter)))
                        .ToList();
                }

                var sorted = ApplySorting(contexts, request.SortBy, request.SortDirection);
                var totalCount = sorted.Count;

                var selectedPaths = BuildSelectedPaths(request);

                var items = sorted
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(context => MapToItem(context, selectedPaths))
                    .ToList();

                var paginated = PaginatedResponse<FinancialReportFieldQueryItemDto>.Create(
                    items,
                    totalCount,
                    request.PageIndex,
                    request.PageSize);

                return ApiResponse<PaginatedResponse<FinancialReportFieldQueryItemDto>>.Success(
                    paginated,
                    "Lấy dữ liệu BCTC theo field thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying financial reports by fields");
                throw;
            }
        }

        private static List<FinancialReportFieldContext> ApplySorting(
            IEnumerable<FinancialReportFieldContext> contexts,
            string sortBy,
            string sortDirection)
        {
            var descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

            return NormalizePath(sortBy) switch
            {
                "ticker" => descending
                    ? contexts.OrderByDescending(x => x.Report.Ticker).ToList()
                    : contexts.OrderBy(x => x.Report.Ticker).ToList(),
                "year" => descending
                    ? contexts.OrderByDescending(x => x.Report.Year).ToList()
                    : contexts.OrderBy(x => x.Report.Year).ToList(),
                "period" => descending
                    ? contexts.OrderByDescending(x => x.Report.Period).ToList()
                    : contexts.OrderBy(x => x.Report.Period).ToList(),
                "status" => descending
                    ? contexts.OrderByDescending(x => x.Report.Status).ToList()
                    : contexts.OrderBy(x => x.Report.Status).ToList(),
                "createdat" => descending
                    ? contexts.OrderByDescending(x => x.Report.CreatedAt).ToList()
                    : contexts.OrderBy(x => x.Report.CreatedAt).ToList(),
                "updatedat" => descending
                    ? contexts.OrderByDescending(x => x.Report.UpdatedAt).ToList()
                    : contexts.OrderBy(x => x.Report.UpdatedAt).ToList(),
                _ => contexts
                    .OrderByDescending(x => x.Report.Year)
                    .ThenByDescending(x => x.Report.Period)
                    .ToList()
            };
        }

        private static List<string> BuildSelectedPaths(QueryFinancialReportsByFieldsQuery request)
        {
            if (request.SelectFields.Count > 0)
            {
                return request.SelectFields
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Select(path => path.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            if (request.Filters.Count > 0)
            {
                return request.Filters
                    .Select(filter => filter.Path)
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            return ["reportData", "indicatorData"];
        }

        private static FinancialReportFieldQueryItemDto MapToItem(
            FinancialReportFieldContext context,
            IReadOnlyCollection<string> selectedPaths)
        {
            var fields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in selectedPaths)
            {
                var exists = TryGetFieldValue(context, path, out var value);
                fields[path] = exists ? value : null;
            }

            return new FinancialReportFieldQueryItemDto
            {
                Id = context.Report.Id,
                Ticker = context.Report.Ticker,
                Year = context.Report.Year,
                Period = context.Report.Period,
                Status = context.Report.Status,
                CreatedAt = context.Report.CreatedAt,
                UpdatedAt = context.Report.UpdatedAt,
                Fields = fields
            };
        }

        private static bool MatchFilter(
            FinancialReportFieldContext context,
            FinancialReportFieldFilterConditionDto filter)
        {
            var @operator = filter.Operator.Trim().ToLowerInvariant();
            var exists = TryGetFieldValue(context, filter.Path, out var leftValue);

            if (@operator == "exists")
            {
                return exists;
            }

            if (@operator == "notexists")
            {
                return !exists;
            }

            if (@operator == "isnull")
            {
                return exists && leftValue == null;
            }

            if (@operator == "isnotnull")
            {
                return exists && leftValue != null;
            }

            if (!exists || leftValue == null || string.IsNullOrWhiteSpace(filter.Value))
            {
                return false;
            }

            return @operator switch
            {
                "eq" => CompareEquals(leftValue, filter.Value),
                "neq" => !CompareEquals(leftValue, filter.Value),
                "contains" => leftValue.ToString()?.Contains(filter.Value, StringComparison.OrdinalIgnoreCase) == true,
                "gt" => CompareOrdering(leftValue, filter.Value) > 0,
                "gte" => CompareOrdering(leftValue, filter.Value) >= 0,
                "lt" => CompareOrdering(leftValue, filter.Value) < 0,
                "lte" => CompareOrdering(leftValue, filter.Value) <= 0,
                _ => false
            };
        }

        private static bool CompareEquals(object left, string rightRaw)
        {
            switch (left)
            {
                case decimal decimalValue:
                    return decimal.TryParse(rightRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var rightDecimal)
                        && decimalValue == rightDecimal;
                case int intValue:
                    return int.TryParse(rightRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var rightInt)
                        && intValue == rightInt;
                case long longValue:
                    return long.TryParse(rightRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var rightLong)
                        && longValue == rightLong;
                case bool boolValue:
                    return bool.TryParse(rightRaw, out var rightBool) && boolValue == rightBool;
                case DateTimeOffset dtoValue:
                    return DateTimeOffset.TryParse(
                            rightRaw,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                            out var rightDto)
                        && dtoValue == rightDto;
                case DateTime dateTimeValue:
                    return DateTime.TryParse(
                            rightRaw,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                            out var rightDateTime)
                        && dateTimeValue == rightDateTime;
                case Enum enumValue:
                    return Enum.TryParse(enumValue.GetType(), rightRaw, true, out var enumParsed)
                        ? Equals(enumValue, enumParsed)
                        : int.TryParse(rightRaw, out var enumInt) && Convert.ToInt32(enumValue) == enumInt;
                default:
                    return string.Equals(
                        left.ToString(),
                        rightRaw,
                        StringComparison.OrdinalIgnoreCase);
            }
        }

        private static int CompareOrdering(object left, string rightRaw)
        {
            switch (left)
            {
                case decimal decimalValue:
                    if (decimal.TryParse(rightRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var rightDecimal))
                    {
                        return decimalValue.CompareTo(rightDecimal);
                    }
                    return int.MinValue;
                case int intValue:
                    if (int.TryParse(rightRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var rightInt))
                    {
                        return intValue.CompareTo(rightInt);
                    }
                    return int.MinValue;
                case long longValue:
                    if (long.TryParse(rightRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var rightLong))
                    {
                        return longValue.CompareTo(rightLong);
                    }
                    return int.MinValue;
                case DateTimeOffset dtoValue:
                    if (DateTimeOffset.TryParse(
                        rightRaw,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var rightDto))
                    {
                        return dtoValue.CompareTo(rightDto);
                    }
                    return int.MinValue;
                case DateTime dateTimeValue:
                    if (DateTime.TryParse(
                        rightRaw,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var rightDateTime))
                    {
                        return dateTimeValue.CompareTo(rightDateTime);
                    }
                    return int.MinValue;
                case Enum enumValue:
                    if (Enum.TryParse(enumValue.GetType(), rightRaw, true, out var enumParsed))
                    {
                        return Convert.ToInt32(enumValue).CompareTo(Convert.ToInt32(enumParsed));
                    }
                    if (int.TryParse(rightRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var rightEnumInt))
                    {
                        return Convert.ToInt32(enumValue).CompareTo(rightEnumInt);
                    }
                    return int.MinValue;
                default:
                    return string.Compare(
                        left.ToString(),
                        rightRaw,
                        StringComparison.OrdinalIgnoreCase);
            }
        }

        private static bool TryGetFieldValue(
            FinancialReportFieldContext context,
            string rawPath,
            out object? value)
        {
            value = null;

            var normalizedPath = NormalizePath(rawPath);

            switch (normalizedPath)
            {
                case "id":
                    value = context.Report.Id;
                    return true;
                case "ticker":
                    value = context.Report.Ticker;
                    return true;
                case "year":
                    value = context.Report.Year;
                    return true;
                case "period":
                    value = context.Report.Period;
                    return true;
                case "status":
                    value = context.Report.Status;
                    return true;
                case "createdat":
                    value = context.Report.CreatedAt;
                    return true;
                case "updatedat":
                    value = context.Report.UpdatedAt;
                    return true;
                case "reportdata":
                    value = JsonElementToObject(context.ReportDataElement);
                    return true;
                case "indicatordata":
                    value = context.IndicatorDataElement.HasValue
                        ? JsonElementToObject(context.IndicatorDataElement.Value)
                        : null;
                    return true;
            }

            var path = rawPath.Trim();

            if (!path.StartsWith("reportData.", StringComparison.OrdinalIgnoreCase)
                && !path.StartsWith("indicatorData.", StringComparison.OrdinalIgnoreCase))
            {
                path = $"reportData.{path}";
            }

            var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 2)
            {
                return false;
            }

            JsonElement current;
            if (segments[0].Equals("reportData", StringComparison.OrdinalIgnoreCase))
            {
                current = context.ReportDataElement;
            }
            else if (segments[0].Equals("indicatorData", StringComparison.OrdinalIgnoreCase))
            {
                if (!context.IndicatorDataElement.HasValue)
                {
                    return false;
                }

                current = context.IndicatorDataElement.Value;
            }
            else
            {
                return false;
            }

            for (var index = 1; index < segments.Length; index++)
            {
                if (!TryGetPropertyIgnoreCase(current, segments[index], out var next))
                {
                    return false;
                }

                current = next;
            }

            value = JsonElementToObject(current);
            return true;
        }

        private static bool TryGetPropertyIgnoreCase(
            JsonElement element,
            string propertyName,
            out JsonElement value)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                value = default;
                return false;
            }

            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static object? JsonElementToObject(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetDecimal(out var decimalValue)
                    ? decimalValue
                    : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Object => element.GetRawText(),
                JsonValueKind.Array => element.GetRawText(),
                _ => element.ToString()
            };
        }

        private static string NormalizePath(string path)
        {
            return path.Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace(".", string.Empty, StringComparison.Ordinal)
                .Trim()
                .ToLowerInvariant();
        }

        private sealed class FinancialReportFieldContext
        {
            public FinancialReport Report { get; }
            public JsonElement ReportDataElement { get; }
            public JsonElement? IndicatorDataElement { get; }

            public FinancialReportFieldContext(FinancialReport report)
            {
                Report = report;
                ReportDataElement = JsonSerializer.SerializeToElement(report.ReportData);
                IndicatorDataElement = report.IndicatorData == null
                    ? null
                    : JsonSerializer.SerializeToElement(report.IndicatorData);
            }
        }
    }
}
