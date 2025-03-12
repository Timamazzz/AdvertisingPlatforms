using AdvertisingPlatforms.API.Interfaces;

namespace AdvertisingPlatforms.API.Utils;

/// <summary>
/// Реализация вспомогательного класса для работы с файлами.
/// Позволяет асинхронно читать текстовый файл построчно без загрузки всего файла в память.
/// </summary>
public class FileHelper(ILogger<FileHelper> logger) : IFileHelper
{
    /// <summary>
    /// Асинхронно читает строки из переданного потока.
    /// </summary>
    /// <param name="stream">Файловый поток для чтения.</param>
    /// <returns>Последовательность строк из файла.</returns>
    public async IAsyncEnumerable<string> ReadLinesAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        int lineNumber = 0;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            lineNumber++;

            if (!string.IsNullOrWhiteSpace(line))
            {
                logger.LogDebug("Считана строка {LineNumber}: {Content}", lineNumber, line);
                yield return line;
            }
            else
            {
                logger.LogDebug("Пропущена пустая строка {LineNumber}", lineNumber);
            }
        }

        logger.LogInformation("Файл успешно прочитан, всего строк: {LineCount}", lineNumber);
    }
}