namespace FileReaders;

public interface IReader<T>
{
    Task<IEnumerable<T>> Read(string fileName);
}
