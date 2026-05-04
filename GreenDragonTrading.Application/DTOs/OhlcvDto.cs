namespace GreenDragonTrading.Application.DTOs
{
    /// <summary>
    /// DTO cho OHLCV response
    /// </summary>
    public class OhlcvDto
    {
        /// <summary>
        /// Thời gian nến (Unix timestamp milliseconds cho frontend)
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// Giá mở cửa
        /// </summary>
        public decimal Open { get; set; }

        /// <summary>
        /// Giá cao nhất
        /// </summary>
        public decimal High { get; set; }

        /// <summary>
        /// Giá thấp nhất
        /// </summary>
        public decimal Low { get; set; }

        /// <summary>
        /// Giá đóng cửa
        /// </summary>
        public decimal Close { get; set; }

        /// <summary>
        /// Khối lượng
        /// </summary>
        public long Volume { get; set; }

        /// <summary>
        /// Giá trị giao dịch (optional)
        /// </summary>
        public decimal? Value { get; set; }
    }

    /// <summary>
    /// Query parameters cho OHLCV
    /// </summary>
    public class OhlcvQueryDto
    {
        /// <summary>
        /// Mã chứng khoán
        /// </summary>
        public string Ticker { get; set; } = null!;

        /// <summary>
        /// Khung thời gian: M1, M5, M15, M30, H1, H4, D1, W1, MN1
        /// </summary>
        public string Timeframe { get; set; } = "D1";

        /// <summary>
        /// Từ ngày (ISO 8601 format)
        /// </summary>
        public DateTime FromTime { get; set; }

        /// <summary>
        /// Đến ngày (ISO 8601 format)
        /// </summary>
        public DateTime ToTime { get; set; }

        /// <summary>
        /// Số lượng nến muốn lấy (alternative to FromTime/ToTime)
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Có cache hay không
        /// </summary>
        public bool UseCache { get; set; } = true;
    }

    /// <summary>
    /// Response cho OHLCV query
    /// </summary>
    public class OhlcvResponseDto
    {
        public string Ticker { get; set; } = null!;
        public string Timeframe { get; set; } = null!;
        public int Count { get; set; }
        public List<OhlcvDto> Data { get; set; } = new();
        public DateTime? FirstTime { get; set; }
        public DateTime? LastTime { get; set; }
        public string Source { get; set; } = null!; // "Database", "Cache", "Computed"
    }

    /// <summary>
    /// DTO cho import OHLCV từ SSI
    /// </summary>
    public class ImportOhlcvDto
    {
        public string Ticker { get; set; } = null!;
        public string Timeframe { get; set; } = null!;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    /// <summary>
    /// Response sau khi import
    /// </summary>
    public class ImportOhlcvResultDto
    {
        public string Ticker { get; set; } = null!;
        public string Timeframe { get; set; } = null!;
        public int RecordsImported { get; set; }
        public int RecordsUpdated { get; set; }
        public int RecordsSkipped { get; set; }
        public DateTime ImportedAt { get; set; }
        public string Status { get; set; } = "Success";
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Response từ SSI API cho Daily OHLCV
    /// </summary>
    public class SsiDailyOhlcvDto
    {
        public string Symbol { get; set; } = null!;
        public string TradingDate { get; set; } = null!; // "yyyy-MM-dd"
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
        public decimal? Value { get; set; }
    }

    /// <summary>
    /// Response từ SSI Streaming cho M1 OHLCV
    /// </summary>
    public class SsiStreamingOhlcvDto
    {
        public string Ticker { get; set; } = null!;
        public DateTime Time { get; set; }
        public decimal Price { get; set; }
        public long Volume { get; set; }
        public decimal? Value { get; set; }
    }
}