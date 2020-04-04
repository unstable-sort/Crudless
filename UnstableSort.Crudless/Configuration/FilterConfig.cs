using System.Collections.Generic;
using System.Linq;
using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.Configuration
{
    public class FilterConfig
    {
        private List<IFilterFactory> _filterFactories = new List<IFilterFactory>();
        
        internal void SetFilters(List<IFilterFactory> filterFactories)
        {
            _filterFactories = filterFactories;
        }

        internal void AddFilters(IEnumerable<IFilterFactory> filterFactories)
        {
            _filterFactories.AddRange(filterFactories);
        }

        public List<IBoxedFilter> GetFilters(IServiceProvider provider)
            => _filterFactories.Select(x => x.Create(provider)).ToList();
    }
}
