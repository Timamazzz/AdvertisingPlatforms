namespace AdvertisingPlatforms.API.Interfaces;

/// <summary>
/// Интерфейс для работы с файловыми потоками.
/// Позволяет построчно читать содержимое файла в асинхронном режиме.
/// </summary>
public interface IFileHelper
{
    /// <summary>
    /// Асинхронно читает строки из переданного потока.
    /// </summary>
    /// <param name="stream">Файловый поток для чтения.</param>
    /// <returns>Последовательность строк из файла.</returns>
    IAsyncEnumerable<string> ReadLinesAsync(Stream stream);
}