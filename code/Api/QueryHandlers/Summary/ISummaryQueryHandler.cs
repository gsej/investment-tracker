namespace Api.QueryHandlers.Summary;

public interface ISummaryQueryHandler
{
    Task<SummaryResult> Handle(SummaryRequest request);
}