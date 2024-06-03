namespace Api.QueryHandlers.Quality;

public interface IQualityQueryHandler
{
    Task<QualityReport> Handle();
}
