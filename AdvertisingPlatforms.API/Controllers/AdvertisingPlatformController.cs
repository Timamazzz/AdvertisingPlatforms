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
public class AdvertisingPlatformController(IAdvertisingPlatformService service, IFileHelper fileHelper) : ControllerBase
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
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request)
    {
        try
        {
            await using var stream = request.File.OpenReadStream();
            var lines = fileHelper.ReadLinesAsync(stream);
            await service.LoadFromFileAsync(lines);

            return Ok("Файл успешно загружен.");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Произошла ошибка при обработке файла.");
        }
    }


    /// <summary>
    /// Получает рекламные площадки для указанной локации.
    /// </summary>
    /// <param name="location">Локация в формате "/ru/svrd/ekb".</param>
    /// <returns>
    /// Возвращает список рекламных площадок для указанного региона.
    /// </returns>
    /// <response code="200">Список рекламных площадок.</response>
    /// <response code="400">Некорректный запрос (например, пустая локация).</response>
    /// <response code="404">Локация не найдена.</response>
    /// <response code="500">Ошибка сервера.</response>
    [HttpGet("{location}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPlatforms([FromRoute] string location)
    {
        try
        {
            var platforms = await service.GetPlatformsForLocationAsync(location);

            if (platforms.Count == 0)
            {
                return NotFound("Для данной локации не найдено рекламных площадок.");
            }

            return Ok(platforms);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Произошла ошибка при обработке запроса.");
        }
    }
}