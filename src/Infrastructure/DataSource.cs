using System.Globalization;
using System.IO.Compression;
using CsvHelper;
using CsvHelper.Configuration;
using Domain;
using Serilog;

namespace Infrastructure;

internal class DataSource
{
    private static DateTime _today = DateTime.Today; 
	private static string _stringDate = string.Empty;
	private const string UrlBase = "http://download.geonames.org/export/dump/";
	private static readonly IReaderConfiguration Config = new CsvConfiguration(CultureInfo.InvariantCulture)
	{
		HasHeaderRecord = false,
		Delimiter = "\t",
		BadDataFound = null,
		MissingFieldFound = null
	};


    private readonly ILogger _log = Log.ForContext<DataSource>();

    public async Task<IEnumerable<GeoName>> GetFullDbAsync(CancellationToken token = default)
    {
        try
        {
	        if (_today != DateTime.Today) 
		        await ChangeDay();

            _log.Information("Getting db from geonames.org.");
            const string url = UrlBase + "cities500.zip";
            await using var zip = await DownloadFileAsync(url, token);

            var files = Unzip(zip);
            await using var file = files["cities500.txt"];

            using var csv = new CsvReader(new StreamReader(file), Config);

            if (token.IsCancellationRequested)
                throw new TaskCanceledException("A task was cancelled.");

            return csv.GetRecords<GeoName>().ToList();
        }
        catch (TaskCanceledException ex)
        {
            throw new TaskCanceledException("A task was cancelled.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get db from geonames.org.", ex);
        }
    }

    public async Task<IEnumerable<GeoName>> GetModificationsAsync(CancellationToken token = default)
    {
        try
        {
	        if (_today != DateTime.Today)
		        await ChangeDay();

			_log.Information("Getting modifications from geonames.org.");
            var url = string.Format(UrlBase + "modifications-" + _stringDate + ".txt");
            await using var file = await DownloadFileAsync(url, token);

            using var csv = new CsvReader(new StreamReader(file), Config);
            if (token.IsCancellationRequested)
                throw new TaskCanceledException("A task was cancelled.");

            return csv.GetRecords<GeoName>().ToList();
        }
        catch (TaskCanceledException ex)
        {
            throw new TaskCanceledException("A task was cancelled.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get modifications from geonames.org.", ex);
        }
    }

    public async Task<IEnumerable<DeleteGeoName>> GetDeletesAsync(CancellationToken token = default)
    {
        try
        {
	        if (_today != DateTime.Today)
		        await ChangeDay();

			_log.Information("Getting deletes from geonames.org.");
            var url = string.Format(UrlBase + "deletes-" + _stringDate + ".txt");
            await using var file = await DownloadFileAsync(url, token);

            using var csv = new CsvReader(new StreamReader(file), Config);
            if (token.IsCancellationRequested)
                throw new TaskCanceledException("A task was cancelled.");

            return csv.GetRecords<DeleteGeoName>().ToList();
        }
        catch (TaskCanceledException ex)
        {
            throw new TaskCanceledException("A task was cancelled.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get deletes from geonames.org.", ex);
        }
    }

    private async Task<Stream> DownloadFileAsync(string url, CancellationToken token = default)
    {
        try
        {
	        Stream stream = new MemoryStream();
            using var client = new HttpClient();
            using var response =
                await client.GetAsync(url, token);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStreamAsync(token);
                await content.CopyToAsync(stream, token);
                stream.Position = 0;
                return stream;
            }

            throw new Exception($"Bad response status code {response.StatusCode}.");
        }
        catch (TaskCanceledException ex)
        {
            throw new TaskCanceledException("A task was cancelled.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to download file from {url}", ex);
        }
    }

    private static IDictionary<string, Stream> Unzip(Stream zip)
    {
        try
        {
            var files = new Dictionary<string, Stream>();

            using var archive = new ZipArchive(zip);
            foreach (var entry in archive.Entries)
            {
                var file = new MemoryStream();
                entry.Open().CopyTo(file);
                file.Position = 0;
                files.Add(entry.FullName, file);
            }

            return files;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get files from zip.", ex);
        }
    }

    private static Task ChangeDay()
    {
	    //Example for 06.05.2023 is 2023-05-05
	    _today = DateTime.Today;
	    _stringDate = $"{_today.Year}-" +
	            $"{(_today.Month >= 10 ? _today.Month.ToString() : "0" + _today.Month)}" +
	            $"-{(_today.Day >= 10 ? (_today.Day - 1).ToString() : "0" + (_today.Day - 1))}";

	    return Task.CompletedTask;
    }
}