namespace Api.Correlation;

public interface ICorrelationIdGenerator
{
    string Get();
    void Set(string correlationId);
}