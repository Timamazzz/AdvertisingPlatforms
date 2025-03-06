using FluentValidation;
using AdvertisingPlatforms.API.Contracts.Requests;

namespace AdvertisingPlatforms.API.Validation;

/// <summary>
/// Настраивает правила валидации для загружаемого файла.
/// </summary>
public class UploadFileRequestValidator : AbstractValidator<UploadFileRequest>
{
    /// <summary>
    /// Допустимые MIME-типы для текстовых файлов.
    /// </summary>
    private static readonly HashSet<string> AllowedMimeTypes = new() { "text/plain" };

    public UploadFileRequestValidator()
    {
        RuleFor(x => x.File)
            .Must(file => file.Length > 0)
            .WithMessage("Файл пуст.")
            .Must(file => AllowedMimeTypes.Contains(file.ContentType))
            .WithMessage($"Недопустимый MIME-тип. Ожидается один из: {string.Join(", ", AllowedMimeTypes)}.");
    }
}