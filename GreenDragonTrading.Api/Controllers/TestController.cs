using GreenDragonTrading.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ISsiServiceV1 _ssiServiceV1;
        private readonly ISsiServiceV2 _ssiServiceV2;
        private readonly ISsiAuthService _ssiAuth;

        public TestController(ISsiServiceV1 ssiServiceV1, ISsiAuthService ssiAuth, ISsiServiceV2 ssiServiceV2)
        {
            _ssiServiceV1 = ssiServiceV1;
            _ssiServiceV2 = ssiServiceV2;
            _ssiAuth = ssiAuth;
        }

        /// <summary>
        /// Gets the list of symbols for a given exchange from SSI API.
        /// </summary>
        [HttpGet("v1/symbols/{exchange}")]
        public async Task<IActionResult> GetSymbols(string exchange)
        {
            var symbols = await _ssiServiceV1.FetchSymbolsListAsync(exchange);
            return Ok(new { Data = symbols, Count = symbols.Count });
        }

        /// <summary>
        /// Gets detailed information for a specific symbol from SSI API.
        /// </summary>
        [HttpGet("symbol-details/{symbol}")]
        public async Task<IActionResult> GetSymbolDetails(string symbol)
        {
            var details = await _ssiServiceV1.FetchSymbolsDetailsAsync(symbol);
            return Ok(details);
        }

        /// <summary>
        /// Gets the list of industries from SSI API.
        /// </summary>
        [HttpGet("industries")]
        public async Task<IActionResult> GetIndustries()
        {
            var industries = await _ssiServiceV1.FetchIndustryListAsync();
            return Ok(industries);
        }

        /// <summary>
        /// Get access token from ssi api
        /// </summary>
        /// <returns>Access token from ssi api</returns>
        [HttpGet("access-token")]
        public async Task<IActionResult> GetAccessToken()
        {
            var token = await _ssiAuth.GetAccessTokenAsync();
            return Ok(token);
        }

        //// <summary>
        ///// Gets the list of symbols for a given exchange from SSI API.
        ///// </summary>
        //[HttpGet("v2/symbols")]
        //public async Task<IActionResult> GetSymbolsV2()
        //{
        //    var symbols = await _ssiServiceV2.FetchSecuritiesDetails();
        //    return Ok(new { Data = symbols });
        //}

    }
}
