using AdvertisingPlatforms.API.Interfaces;

namespace AdvertisingPlatforms.API.Utils;

/// <summary>
/// Реализация вспомогательного класса для работы с файлами.
/// Позволяет асинхронно читать текстовый файл построчно без загрузки всего файла в память.
/// </summary>
public class FileHelper : IFileHelper
{
    /// <summary>
    /// Асинхронно читает строки из переданного потока.
    /// </summary>
    /// <param name="stream">Файловый поток для чтения.</param>
    /// <returns>Последовательность строк из файла.</returns>
    public async IAsyncEnumerable<string> ReadLinesAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (!string.IsNullOrWhiteSpace(line))
                yield return line;
        }
    }
}