using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StorageManager.Interfaces
{
    public interface IAsyncQueryProvider : IQueryProvider
    {
        Task<object> ExecuteAsync(Expression expression);
        Task<TResult> ExecuteAsync<TResult>(Expression expression);
    }
}