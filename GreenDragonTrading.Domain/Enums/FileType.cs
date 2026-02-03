namespace GreenDragonTrading.Domain.Enums;

/// <summary>
/// File type enumeration
/// </summary>
public enum FileType
{
    /// <summary>
    /// PDF document
    /// </summary>
    Pdf = 1,

    /// <summary>
    /// Word document (docx)
    /// </summary>
    Docx = 2,

    /// <summary>
    /// JPEG image
    /// </summary>
    Jpeg = 3,

    /// <summary>
    /// PNG image
    /// </summary>
    Png = 4,

    /// <summary>
    /// GIF image
    /// </summary>
    Gif = 5,

    /// <summary>
    /// WebP image
    /// </summary>
    WebP = 6,

    /// <summary>
    /// Other file type
    /// </summary>
    Other = 99
}
