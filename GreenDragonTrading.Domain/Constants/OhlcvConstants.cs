namespace GreenDragonTrading.Domain.Constants
{
    /// <summary>
    /// Constants cho OHLCV operations
    /// </summary>
    public static class OhlcvConstants
    {
        /// <summary>
        /// Các timeframe được hỗ trợ
        /// </summary>
        public static class Timeframes
        {
            public const string M1 = "M1";   // 1 phút
            public const string M5 = "M5";   // 5 phút
            public const string M15 = "M15"; // 15 phút
            public const string M30 = "M30"; // 30 phút
            public const string H1 = "H1";   // 1 giờ
            public const string H4 = "H4";   // 4 giờ
            public const string D1 = "D1";   // 1 ngày
            public const string W1 = "W1";   // 1 tuần
            public const string MN1 = "MN1"; // 1 tháng

            /// <summary>
            /// Timeframes lưu vào DB (raw data từ SSI)
            /// </summary>
            public static readonly string[] StoredTimeframes = { M1, D1 };

            /// <summary>
            /// Timeframes tính toán từ M1
            /// </summary>
            public static readonly string[] ComputedFromM1 = { M5, M15, M30, H1, H4 };

            /// <summary>
            /// Timeframes tính toán từ D1
            /// </summary>
            public static readonly string[] ComputedFromD1 = { W1, MN1 };

            /// <summary>
            /// Tất cả timeframes
            /// </summary>
            public static readonly string[] All = { M1, M5, M15, M30, H1, H4, D1, W1, MN1 };

            /// <summary>
            /// Map timeframe sang số phút
            /// </summary>
            public static readonly Dictionary<string, int> ToMinutes = new()
            {
                { M1, 1 },
                { M5, 5 },
                { M15, 15 },
                { M30, 30 },
                { H1, 60 },
                { H4, 240 },
                { D1, 1440 }
            };

            /// <summary>
            /// Validate timeframe
            /// </summary>
            public static bool IsValid(string timeframe)
            {
                return All.Contains(timeframe, StringComparer.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Kiểm tra có phải timeframe được lưu trong DB không
            /// </summary>
            public static bool IsStored(string timeframe)
            {
                return StoredTimeframes.Contains(timeframe, StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Nguồn dữ liệu
        /// </summary>
        public static class Sources
        {
            public const string SsiStreaming = "SSI_STREAMING";
            public const string SsiApi = "SSI_API";
            public const string Computed = "COMPUTED";
            public const string Manual = "MANUAL";
        }

        /// <summary>
        /// Redis cache keys
        /// </summary>
        public static class CacheKeys
        {
            public const string OhlcvPrefix = "OHLCV";

            public static string GetKey(string ticker, string timeframe, DateTime date)
            {
                return $"{OhlcvPrefix}:{ticker}:{timeframe}:{date:yyyy-MM-dd}";
            }

            public static string GetLatestKey(string ticker, string timeframe)
            {
                return $"{OhlcvPrefix}:LATEST:{ticker}:{timeframe}";
            }
        }

        /// <summary>
        /// Cache expiration times
        /// </summary>
        public static class CacheExpiration
        {
            public static readonly TimeSpan M1 = TimeSpan.FromMinutes(1);
            public static readonly TimeSpan M5 = TimeSpan.FromMinutes(5);
            public static readonly TimeSpan M15 = TimeSpan.FromMinutes(15);
            public static readonly TimeSpan H1 = TimeSpan.FromHours(1);
            public static readonly TimeSpan D1 = TimeSpan.FromHours(12);
            public static readonly TimeSpan Default = TimeSpan.FromMinutes(5);

            public static TimeSpan GetExpiration(string timeframe)
            {
                return timeframe.ToUpper() switch
                {
                    "M1" => M1,
                    "M5" => M5,
                    "M15" => M15,
                    "H1" => H1,
                    "D1" => D1,
                    _ => Default
                };
            }
        }
    }
}