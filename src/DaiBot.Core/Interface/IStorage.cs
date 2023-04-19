using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaiBot.Core.Interface
{
    public interface IStorage
    {
        public T? Get<T>(string key);

        public string? GetString(string key);

        public void Set<T>(string key, T value);
    }
}
