using ASC.Web.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ASC.Web.Data
{
    public interface INavigationCacheOperations
    {
        Task<NavigationMenu> GetNavigationCacheAsync();

        Task CreateNavigationCacheAsync();
    }
}
