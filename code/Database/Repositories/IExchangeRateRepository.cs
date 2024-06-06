using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Entities;

namespace Database.Repositories;

public interface IExchangeRateRepository
{
    Task<IList<ExchangeRate>> GetAll();
}
