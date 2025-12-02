using GreenDragonTrading.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ISsiService _ssiService;

        public TestController(ISsiService ssiService)
        {
            _ssiService = ssiService;
        }

        [HttpGet("symbols/{exchange}")]
        public async Task<IActionResult> GetSymbols(string exchange)
        {
            var symbols = await _ssiService.FetchSymbolsListAsync(exchange);
            return Ok(new { Data = symbols, Count = symbols.Count });
        }

        [HttpGet("symbol-details/{symbol}")]
        public async Task<IActionResult> GetSymbolDetails(string symbol)
        {
            var details = await _ssiService.FetchSymbolsDetailsAsync(symbol);
            return Ok(details);
        }

        [HttpGet("industries")]
        public async Task<IActionResult> GetIndustries()
        {
            var industries = await _ssiService.FetchIndustryListAsync();
            return Ok(industries);
        }
    }
}
