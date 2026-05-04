using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.MacroeconomicData.Commands.UpsertMacroeconomicData;
using GreenDragonTrading.Application.UseCases.MacroeconomicData.Queries.GetMacroeconomicData;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// API quản lý dữ liệu kinh tế vĩ mô (Macroeconomic Data). Luôn lưu trữ 1 record duy nhất.
    /// </summary>
    [ApiController]
    [Route("api/v1/macroeconomic-data")]
    public class MacroeconomicDataController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MacroeconomicDataController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lấy dữ liệu kinh tế vĩ mô cấu hình hiện tại
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Danh sách thông số vĩ mô</returns>
        [HttpGet]
        public async Task<IActionResult> GetMacroeconomicData(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetMacroeconomicDataQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật (Upsert) dữ liệu kinh tế vĩ mô cho hệ thống
        /// </summary>
        /// <param name="command">Dữ liệu vĩ mô mới nhất</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dữ liệu đã được cập nhật</returns>
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpsertMacroeconomicData([FromBody] UpsertMacroeconomicDataCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
    }
}