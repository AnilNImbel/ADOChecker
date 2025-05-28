using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOAnalyser
{
    public interface  IUtility
    {
        Task<string> GetDataAsync(string URl);
        string GetDataSync(string URl);

        Task<string> PostDataAsync(string URl, HttpContent content);

        string PostDataSync(string URl, HttpContent content);
    }
}
