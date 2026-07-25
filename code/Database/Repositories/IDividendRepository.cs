using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Entities;

namespace Database.Repositories;

public interface IDividendRepository
{
    Task BulkAdd(IEnumerable<Dividend> dividends);
}
