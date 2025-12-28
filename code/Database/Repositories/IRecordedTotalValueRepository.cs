using System.Threading.Tasks;
using Database.Entities;

namespace Database.Repositories;

public interface IRecordedTotalValueRepository
{
    void Add(RecordedTotalValue recordedTotalValue);
    Task<int> SaveChangesAsync();
}
