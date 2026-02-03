using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ResourceManagement.Domain.Interfaces
{
    public interface IExcelService
    {
        Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName);
        Task<List<T>> ImportFromExcelAsync<T>(Stream fileStream) where T : new();
    }
}
