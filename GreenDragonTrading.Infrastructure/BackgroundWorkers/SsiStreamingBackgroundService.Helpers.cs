using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    public partial class SsiStreamingBackgroundService
    {
        /// <summary>
        /// Builds a <see cref="PriceDepthDto"/> from market snapshot and derived depth ratios.
        /// </summary>
        /// <param name="ticker">Symbol ticker.</param>
        /// <param name="d">Market symbol snapshot.</param>
        /// <returns>Computed price-depth DTO for broadcasting.</returns>
        private static PriceDepthDto BuildPriceDepthDto(string ticker, MarketSymbolDto d)
        {
            var refPrice = d.ReferencePrice;

            double Chg(double price) => price > 0 && refPrice > 0 ? price - refPrice : 0;
            double ChgPct(double price) => price > 0 && refPrice > 0 ? ((price - refPrice) / refPrice) * 100 : 0;

            var bidSum = d.BidVol1 + d.BidVol2 + d.BidVol3;
            var askSum = d.AskVol1 + d.AskVol2 + d.AskVol3;
            var depthTotal = bidSum + askSum;
            var bullPct = depthTotal > 0 ? (int)Math.Round((bidSum / depthTotal) * 100) : 50;

            var fBuyPct = d.TotalVol > 0 ? Math.Round((d.FBuyVol / d.TotalVol) * 100, 1) : 0.0;
            var fSellPct = d.TotalVol > 0 ? Math.Round((d.FSellVol / d.TotalVol) * 100, 1) : 0.0;

            var maxDepthVol = Math.Max(1, new[] { d.AskVol1, d.AskVol2, d.AskVol3, d.BidVol1, d.BidVol2, d.BidVol3 }.Max());

            return new PriceDepthDto
            {
                Ticker = ticker,
                AskPrice1 = d.AskPrice1, AskVol1 = d.AskVol1,
                AskPrice2 = d.AskPrice2, AskVol2 = d.AskVol2,
                AskPrice3 = d.AskPrice3, AskVol3 = d.AskVol3,
                BidPrice1 = d.BidPrice1, BidVol1 = d.BidVol1,
                BidPrice2 = d.BidPrice2, BidVol2 = d.BidVol2,
                BidPrice3 = d.BidPrice3, BidVol3 = d.BidVol3,
                ReferencePrice = refPrice,
                CeilingPrice = d.CeilingPrice,
                FloorPrice = d.FloorPrice,
                Change = d.Change,
                RatioChange = d.RatioChange,
                TotalVol = d.TotalVol,
                AskChange1 = Chg(d.AskPrice1), AskChangePct1 = ChgPct(d.AskPrice1),
                AskChange2 = Chg(d.AskPrice2), AskChangePct2 = ChgPct(d.AskPrice2),
                AskChange3 = Chg(d.AskPrice3), AskChangePct3 = ChgPct(d.AskPrice3),
                BidChange1 = Chg(d.BidPrice1), BidChangePct1 = ChgPct(d.BidPrice1),
                BidChange2 = Chg(d.BidPrice2), BidChangePct2 = ChgPct(d.BidPrice2),
                BidChange3 = Chg(d.BidPrice3), BidChangePct3 = ChgPct(d.BidPrice3),
                FBuyVol = d.FBuyVol,
                FSellVol = d.FSellVol,
                FBuyVal = d.FBuyVal,
                FSellVal = d.FSellVal,
                TotalBuyVol = d.TotalBuyVol,
                TotalSellVol = d.TotalSellVol,
                BullPct = bullPct,
                BearPct = 100 - bullPct,
                FBuyPct = fBuyPct,
                FSellPct = fSellPct,
                MaxDepthVol = maxDepthVol,
                Side = d.Side,
                TradingSession = d.TradingSession,
            };
        }

        /// <summary>
        /// Adds numeric field to update map only when value changed.
        /// </summary>
        /// <typeparam name="T">Struct type of the field.</typeparam>
        /// <param name="updates">Accumulated changed fields map.</param>
        /// <param name="fieldName">Target field name.</param>
        /// <param name="newValue">Incoming value.</param>
        /// <param name="existingValue">Current cached value.</param>
        private static void AddIfChanged<T>(Dictionary<string, object> updates, string fieldName, T? newValue, T existingValue)
            where T : struct
        {
            var valueToCompare = newValue ?? default;
            if (!EqualityComparer<T>.Default.Equals(valueToCompare, existingValue))
            {
                updates[fieldName] = valueToCompare;
            }
        }

        /// <summary>
        /// Adds string field to update map only when value changed.
        /// </summary>
        /// <param name="updates">Accumulated changed fields map.</param>
        /// <param name="fieldName">Target field name.</param>
        /// <param name="newValue">Incoming value.</param>
        /// <param name="existingValue">Current cached value.</param>
        private static void AddIfChanged(Dictionary<string, object> updates, string fieldName, string? newValue, string existingValue)
        {
            var valueToCompare = newValue ?? string.Empty;
            if (!string.Equals(valueToCompare, existingValue, StringComparison.Ordinal))
            {
                updates[fieldName] = valueToCompare;
            }
        }
    }
}
