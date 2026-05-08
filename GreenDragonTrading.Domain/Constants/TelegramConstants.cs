namespace GreenDragonTrading.Domain.Constants;

public static class TelegramConstants
{
    public const string StartCommand = "/start";
    public const string WebhookSecretHeaderName = "X-Telegram-Bot-Api-Secret-Token";
    public const string DeepLinkBaseUrl = "https://t.me";

    public const string StartTokenCreatedMessage = "Tạo token liên kết Telegram thành công";
    public const string MissingStartTokenMessage = "Không tìm thấy mã liên kết (st). Vui lòng tạo lại liên kết từ ứng dụng.";
    public const string LinkSuccessBotReplyMessage = "Liên kết Telegram thành công. Bạn sẽ nhận thông báo tại đây.";

    public const string InvalidLinkTokenMessage = "Token liên kết không hợp lệ.";
    public const string InvalidOrExpiredLinkTokenMessage = "Token liên kết đã hết hạn hoặc không hợp lệ. Vui lòng lấy token mới từ ứng dụng.";
    public const string LinkUserNotFoundMessage = "Không tìm thấy người dùng để liên kết Telegram.";
    public const string TelegramAlreadyLinkedMessage = "Telegram account này đã liên kết với tài khoản khác.";
    public const string LinkSuccessServiceMessage = "Liên kết Telegram thành công.";
}