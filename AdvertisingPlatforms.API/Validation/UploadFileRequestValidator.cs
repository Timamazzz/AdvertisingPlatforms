using FluentValidation;
using AdvertisingPlatforms.API.Contracts.Requests;
using System.Text.RegularExpressions;

namespace AdvertisingPlatforms.API.Validation;

public class UploadFileRequestValidator : AbstractValidator<UploadFileRequest>
{
    public UploadFileRequestValidator()
    {
        Console.WriteLine("Hello valid");
        RuleFor(x => x.File)
            .NotNull().WithMessage("Файл обязателен.")
            .Must(file => file is { Length: > 0 }).WithMessage("Файл пуст.");
    }
}