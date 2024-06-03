namespace FileReaders;

public interface IRecordedTotalValueReader
{
    IEnumerable<RecordedTotalValue> Read(string fileName);
}
