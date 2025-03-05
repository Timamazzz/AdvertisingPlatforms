namespace AdvertisingPlatforms.API.Contracts.Requests;

/// <summary>
/// Запрос на загрузку файла с рекламными площадками.
/// </summary>
/// <param name="File">Файл в формате .txt, содержащий список рекламных площадок и их регионов.</param>
/// <remarks>
/// **Требования к файлу:**  
/// - Файл должен быть в формате `.txt`.  
/// - Каждая строка должна содержать название рекламной площадки и список регионов, разделённых двоеточием.
/// 
/// **Пример содержимого файла:**  
/// ```
/// Яндекс.Директ:/ru
/// Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
/// ```
/// </remarks>
public record UploadFileRequest(IFormFile File);