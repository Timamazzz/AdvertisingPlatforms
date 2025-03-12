using AdvertisingPlatforms.Application.Interfaces;
using AdvertisingPlatforms.API.Contracts.Requests;
using AdvertisingPlatforms.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisingPlatforms.API.Controllers;

/// <summary>
/// Контроллер для управления рекламными площадками.
/// Позволяет загружать список рекламных площадок из текстового файла и получать рекламные площадки по региону.
/// </summary>
/// <param name="service">Сервис для работы с рекламными площадками.</param>
/// <param name="fileHelper">Утилита для обработки файлов.</param>
[ApiController]
[Route("api/advertising-platforms")]
public class AdvertisingPlatformController(
    IAdvertisingPlatformService service,
    IFileHelper fileHelper,
    ILogger<AdvertisingPlatformController> logger) : ControllerBase
{
    /// <summary>
    /// Загружает рекламные площадки из текстового файла.
    /// </summary>
    /// <param name="request">Файл с рекламными площадками в формате .txt.</param>
    /// <returns>
    /// Возвращает 200 OK в случае успешной загрузки.  
    /// Возвращает 400 Bad Request, если файл отсутствует, пуст или имеет некорректный формат.
    /// </returns>
    /// <response code="200">Файл успешно загружен.</response>
    /// <response code="400">Ошибка валидации: файл отсутствует, пуст или имеет неверный формат.</response>
    /// <response code="500">Ошибка сервера.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request)
    {
        logger.LogInformation("Получен файл {FileName} размером {FileSize} байт", request.File.FileName,
            request.File.Length);


        await using var stream = request.File.OpenReadStream();
        var lines = fileHelper.ReadLinesAsync(stream);
        await service.LoadFromFileAsync(lines);

        logger.LogInformation("Файл {FileName} успешно обработан", request.File.FileName);
        return Ok(new { message = "Файл успешно загружен." });
    }


    /// <summary>
    /// Получает рекламные площадки, действующие в указанной локации.
    /// </summary>
    /// <param name="location">Локация в формате "/ru".</param>
    /// <returns>
    /// Возвращает список рекламных площадок, если они есть.
    /// </returns>
    /// <response code="200">Успешный запрос. Возвращает список рекламных площадок.</response>
    /// <response code="400">Некорректный запрос (например, передана пустая строка).</response>
    /// <response code="404">Указанная локация не существует.</response>
    /// <response code="404">Локация найдена, но для нее нет рекламных площадок.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    /// <response code="503">Данные о рекламных площадках не загружены.</response>
    [HttpGet("{location}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetPlatforms([FromRoute] string location)
    {
        logger.LogInformation("Запрос рекламных площадок для локации: {Location}", location);

        var platforms = await service.GetPlatformsForLocationAsync(location);

        logger.LogInformation("Найдено {Count} площадок для локации {Location}", platforms.Count, location);
        return Ok(platforms);
    }
}