using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using AutoMapper.Configuration;
using SimpleInjector;

namespace UnstableSort.Crudless.Tests
{
    public static class AutoMapperConfig
    {
        // ReSharper disable once UnusedMember.Local
        private static void MapStringEnum<TEnum>(IProfileExpression config)
            where TEnum : struct
        {
            config.CreateMap<string, TEnum>().ConvertUsing(x => Enum.Parse<TEnum>(x, true));
        }

        private static void MapStringEnums(this IMapperConfigurationExpression config, params Assembly[] assemblies)
        {
            var enumTypes = assemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsEnum);

            var mapMethod = typeof(AutoMapperConfig)
                .GetMethod(nameof(MapStringEnum), BindingFlags.NonPublic | BindingFlags.Static);

            enumTypes.ForEach(x => mapMethod.MakeGenericMethod(x).Invoke(null, new object[] { config }));
        }

        public static void ConfigureAutoMapper(this Container container, params Assembly[] additionalAssemblies)
        {
            container.Options.AllowOverridingRegistrations = true;

            var assemblies = new List<Assembly>(additionalAssemblies.Length + 1);
            assemblies.Add(typeof(AutoMapperConfig).Assembly);
            assemblies.AddRange(additionalAssemblies);

            var configAssemblies = assemblies.Distinct().ToArray();

            container.RegisterSingleton<IMapper>(() =>
            {
                var mce = new MapperConfigurationExpression();

                mce.ConstructServicesUsing(container.GetInstance);
                mce.AddMaps(configAssemblies);
                mce.MapStringEnums(configAssemblies);
                
                return new Mapper(new MapperConfiguration(mce), container.GetInstance);
            });

            container.Options.AllowOverridingRegistrations = false;
        }
    }
}
