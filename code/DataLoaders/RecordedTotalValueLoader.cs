using Common.Extensions;
using Common.Tracing;
using Database.Repositories;
using FileReaders;
using Microsoft.Extensions.Logging;
namespace DataLoaders;

public class RecordedTotalValueLoader
{
    private readonly ILogger<RecordedTotalValueLoader> _logger;
    private readonly IRecordedTotalValueRepository _recordedTotalValueRepository;
    private readonly IReader<RecordedTotalValue> _reader;

    public RecordedTotalValueLoader(ILogger<RecordedTotalValueLoader> logger,
        IRecordedTotalValueRepository recordedTotalValueRepository,
        IReader<RecordedTotalValue> reader)
    {
        _logger = logger;
        _recordedTotalValueRepository = recordedTotalValueRepository;
        _reader = reader;
    }

    public async Task LoadFile(string fileName, string source)
    {
        var recordedValues = (await _reader.Read(fileName)).ToList();
        
        using (InvestmentTrackerActivitySource.Instance.StartActivity($"File: {fileName}"))
        using (_logger.BeginScope(new Dictionary<string, string> { ["File"] = fileName }))
        {
            foreach (var recordedValueDto in recordedValues)
            {
              
                    decimal totalValueInGbp;

                    var totalValueParsable = decimal.TryParse(recordedValueDto.TotalValueInGbp, null, out totalValueInGbp);

                    if (!totalValueParsable)
                    {
                        _logger.LogWarning("Could not parse total value {totalValue}", recordedValueDto.TotalValueInGbp);
                        continue;
                    }

                    var recordedTotalValue = new Database.Entities.RecordedTotalValue(
                        recordedValueDto.AccountCode,
                        recordedValueDto.Date.ToDateOnly(),
                        totalValueInGbp,
                        recordedValueDto.Source);

                    _recordedTotalValueRepository.Add(recordedTotalValue);

                await _recordedTotalValueRepository.SaveChangesAsync();
            }
        }
    }
}
    
