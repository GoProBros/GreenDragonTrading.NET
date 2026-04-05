namespace GreenDragonTrading.Domain.Constants;

public static class TelegramConstants
{
    public const string StartCommand = "/start";
    public const string WebhookSecretHeaderName = "X-Telegram-Bot-Api-Secret-Token";
    public const string DeepLinkBaseUrl = "https://t.me";

    public const string StartTokenCreatedMessage = "Tạo token liên kết Telegram thành công";
    public const string MissingStartTokenMessage = "Khong tim thay ma lien ket (st). Vui long tao lai lien ket tu ung dung.";
    public const string LinkSuccessBotReplyMessage = "Lien ket Telegram thanh cong. Ban se nhan canh bao tai day.";

    public const string InvalidLinkTokenMessage = "Token liên kết không hợp lệ.";
    public const string InvalidOrExpiredLinkTokenMessage = "Token liên kết đã hết hạn hoặc không hợp lệ. Vui lòng lấy token mới từ ứng dụng.";
    public const string LinkUserNotFoundMessage = "Không tìm thấy người dùng để liên kết Telegram.";
    public const string TelegramAlreadyLinkedMessage = "Telegram account này đã liên kết với tài khoản khác.";
    public const string LinkSuccessServiceMessage = "Liên kết Telegram thành công.";
}