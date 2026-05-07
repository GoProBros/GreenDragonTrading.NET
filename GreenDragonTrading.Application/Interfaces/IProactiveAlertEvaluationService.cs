using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Service interface for evaluating proactive signal-level alerts through AI service.
    /// </summary>
    public interface IProactiveAlertEvaluationService
    {
        /// <summary>
        /// Evaluates one proactive signal job and returns normalized result from AI service.
        /// </summary>
        Task<ProactiveAlertEvaluationClientResult> EvaluateAsync(
            ProactiveAiEvaluationJobDto job,
            CancellationToken cancellationToken = default);
    }
}
