using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Repo
{
    public interface IDataRepository
    {
		Task<int> SaveAsync();
    }
}
