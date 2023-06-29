using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace UnstableSort.Crudless.Integration.AutoMapper
{
    public class AutoMapperObjectMapper : IObjectMapper
    {
        private readonly IMapper _mapper;

        public AutoMapperObjectMapper(IMapper mapper) 
            => _mapper = mapper;

        public TSource Clone<TSource>(TSource source)
            => _mapper.Map<TSource, TSource>(source);

        public TDestination Map<TSource, TDestination>(TSource source)
            => _mapper.Map<TSource, TDestination>(source);

        public TDestination Map<TDestination>(object source)
            => _mapper.Map<TDestination>(source);

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
            => _mapper.Map(source, destination);

        public IQueryable<TDestination> Project<TSource, TDestination>(IQueryable<TSource> source)
            => source.ProjectTo<TDestination>(_mapper.ConfigurationProvider);
    }
}
