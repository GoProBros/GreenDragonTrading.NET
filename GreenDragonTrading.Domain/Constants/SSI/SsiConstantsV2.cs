namespace GreenDragonTrading.Domain.Constants.SSI
{
    public static class SsiConstantsV2
    {
        /// <summary>
        /// SSI Exchange Codes : HOSE / HSX
        /// </summary>
        public const string SSI_EXCHANGE_HSX = "HOSE";

        /// <summary>
        /// SSI Exchange Codes : HNX
        /// </summary>
        public const string SSI_EXCHANGE_HNX = "HNX";

        /// <summary>
        /// SSI Exchange Codes : UPCOM
        /// </summary>
        public const string SSI_EXCHANGE_UPCOM = "UPCOM";

        /// <summary>
        /// SSI Symbol Types V2: Stock
        /// </summary>
        public const string SSI_SYMBOL_TYPE_STOCK = "S";

        /// <summary>
        /// SSI Symbol Types V2: ETF
        /// </summary>
        public const string SSI_SYMBOL_TYPE_ETF = "E";

        /// <summary>
        /// SSI Symbol Types V2: Bond
        /// </summary>
        public const string SSI_SYMBOL_TYPE_BOND = "D";

        /// <summary>
        /// SSI Symbol Types V2: Mutual Fund
        /// </summary>
        public const string SSI_SYMBOL_TYPE_MUTUAL_FUND = "U";

        /// <summary>
        /// SSI Symbol Types V2: Futures
        /// </summary>
        public const string SSI_SYMBOL_TYPE_FUTURES = "FU";

        /// <summary>
        /// SSI Symbol Types V2: Covered Warrant
        /// </summary>
        public const string SSI_SYMBOL_TYPE_COVERED_WARRANT = "W";

        /// <summary>
        /// SSI Symbol Trading Status: Normal
        /// </summary>
        public const string SSI_SYMBOL_TRADING_STATUS_NORMAL = "N";

        /// <summary>
        /// SSI Symbol Trading Status: Delisted
        /// </summary>
        public const string SSI_SYMBOL_TRADING_STATUS_DELISTED = "D";

        /// <summary>
        /// SSI Symbol Trading Status: Halted
        /// </summary>
        public const string SSI_SYMBOL_TRADING_STATUS_HALT = "H";

        /// <summary>
        /// SSI Symbol Trading Status: Suspended
        /// </summary>
        public const string SSI_SYMBOL_TRADING_STATUS_SUSPEND = "S";

        /// <summary>
        /// SSI Symbol Trading Status: New Listing
        /// </summary>
        public const string SSI_SYMBOL_TRADING_STATUS_NEW_LIST = "NL";

        /// <summary>
        /// SSI Symbol Trading Status: Near Delisting
        /// </summary>
        public const string SSI_SYMBOL_TRADING_STATUS_NEAR_DELIST = "ND";

        /// <summary>
        /// SSI Symbol Trading Status: Special Trading
        /// </summary>
        public const string SSI_SYMBOL_TRADING_STATUS_SPECIAL_TRADING = "ST";

        /// <summary>
        /// SSI Symbol Trading Status: Suspended A
        /// </summary>
        public const string SSI_SYMBOL_TRADING_STATUS_SUSPENDED_A = "SA";

        /// <summary>
        /// SSI Symbol Trading Status: Suspended PT
        /// </summary>
        public const string SSI_SYMBOL_TRADING_STATUS_SUSPENDED_PT = "SP";

        #region SSI Streaming
        /// <summary>
        /// SSI Streaming Data Type: F - Securities status
        /// </summary>
        public const string SSI_STREAMING_DATA_TYPE_F = "F";

        /// <summary>
        /// SSI Streaming Data Type: X - Securities snapshot
        /// </summary>
        public const string SSI_STREAMING_DATA_TYPE_X = "X";
        /// <summary>
        /// SSI Streaming Data Type: X-Quote - provides best bid/ask. 
        /// </summary>
        public const string SSI_STREAMING_DATA_TYPE_X_QUOTE = "X-QUOTE";

        /// <summary>
        /// SSI Streaming Data Type: X-Trade
        /// </summary>
        public const string SSI_STREAMING_DATA_TYPE_X_TRADE = "X-TRADE";

        /// <summary>
        /// SSI Streaming Data Type: Foreign room
        /// </summary>
        public const string SSI_STREAMING_DATA_TYPE_FOREIGN = "R";

        /// <summary>
        /// SSI Streaming Data Type: B - OHLCV returns open, high, low, close, volume of securities/indexes by tick. 
        /// </summary>
        public const string SSI_STREAMING_DATA_TYPE_B = "B";

        /// <summary>
        /// SSI Streaming Data Type: R - Foreign room
        /// </summary>
        public const string SSI_STREAMING_DATA_TYPE_R = "R";

        /// <summary>
        /// SSI Streaming Data Type: MI - Realtime update index values of HOSE, HNX, UPCOM. 
        /// </summary>
        public const string SSI_STREAMING_DATA_TYPE_MI = "MI";

        /// <summary>
        /// SSI Streaming Data Type: OL - Odlot message including open, high, low, close, volume .. of stocks 
        /// </summary>
        public const string SSI_STREAMING_DATA_TYPE_OL = "OL";

        /// <summary>
        /// SSI Streaming Channel: X-Quote - provides best bid/ask. 
        /// </summary>
        public const string SSI_STREAMING_CHANNEL_X_QUOTE = "X-QUOTE";

        /// <summary>
        /// SSI Streaming Channel: X-Trade
        /// </summary>
        public const string SSI_STREAMING_CHANNEL_X_TRADE = "X-TRADE";

        /// <summary>
        /// SSI Streaming Channel: Foreign room
        /// </summary>
        public const string SSI_STREAMING_CHANNEL_FOREIGN = "R";

        /// <summary>
        /// SSI Streaming Channel: Securities snapshot
        /// </summary>
        public const string SSI_STREAMING_CHANNEL_X = "X";

        /// <summary>
        /// SSI Streaming Channel: Realtime OHLCV (Open, High, Low, Close, Volume)
        /// </summary>
        public const string SSI_STREAMING_CHANNEL_B = "B";
        #endregion SSI Streaming
    }
}
