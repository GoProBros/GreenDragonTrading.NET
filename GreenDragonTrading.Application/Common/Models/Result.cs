namespace GreenDragonTrading.Application.Common.Models
{
    /// <summary>
    /// Base result for commands that don't return data
    /// </summary>
    public record Result(bool Success, string Message);

    /// <summary>
    /// Generic result for commands that return data
    /// </summary>
    public record Result<T>(bool Success, string Message, T? Data = default) : Result(Success, Message);
}
