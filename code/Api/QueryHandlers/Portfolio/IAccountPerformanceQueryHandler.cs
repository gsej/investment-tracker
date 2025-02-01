using Api.QueryHandlers.Performance;

namespace Api.QueryHandlers.Portfolio;

public interface IAccountPerformanceQueryHandler
{
    Task<AccountPerformanceResult> Handle(AccountPerformanceRequest request);
}
