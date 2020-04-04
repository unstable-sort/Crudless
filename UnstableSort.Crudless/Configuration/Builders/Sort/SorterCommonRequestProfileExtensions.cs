using System;
using System.Linq.Expressions;
using UnstableSort.Crudless.Configuration.Builders;
using UnstableSort.Crudless.Configuration.Builders.Sort;

// ReSharper disable once CheckNamespace
namespace UnstableSort.Crudless.Configuration
{
    public static class SorterCommonRequestProfileExtensions
    {
        public static TBuilder SortBy<TBuilder, TRequest, TEntity, TProperty>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Action<BasicSortBuilder<TRequest, TEntity>> sort)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var builder = new BasicSortBuilder<TRequest, TEntity>();
            sort(builder);

            var sorterFactory = builder.Build();

            return config.SetSorter(sorterFactory);
        }

        public static TBuilder SortBy<TBuilder, TRequest, TEntity, TProperty>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TEntity, TProperty>> sortColumn)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var builder = new BasicSortBuilder<TRequest, TEntity>();
            builder.SortBy(sortColumn);

            var sorterFactory = builder.Build();

            return config.SetSorter(sorterFactory);
        }

        public static TBuilder SortByDescending<TBuilder, TRequest, TEntity, TProperty>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TEntity, TProperty>> sortColumn)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var builder = new BasicSortBuilder<TRequest, TEntity>();
            builder.SortBy(sortColumn).Descending();

            var sorterFactory = builder.Build();

            return config.SetSorter(sorterFactory);
        }

        public static TBuilder SortBy<TBuilder, TRequest, TEntity>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config, 
            string sortColumn)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var builder = new BasicSortBuilder<TRequest, TEntity>();
            builder.SortBy(sortColumn);

            var sorterFactory = builder.Build();

            return config.SetSorter(sorterFactory);
        }

        public static TBuilder SortByDescending<TBuilder, TRequest, TEntity>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config, 
            string sortColumn)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var builder = new BasicSortBuilder<TRequest, TEntity>();
            builder.SortBy(sortColumn).Descending();

            var sorterFactory = builder.Build();

            return config.SetSorter(sorterFactory);
        }

        public static TBuilder SortBy<TBuilder, TRequest, TEntity, TProperty>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TEntity, TProperty>> sortColumn,
            Action<BasicSortClauseBuilder<TRequest, TEntity>> andThen)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var builder = new BasicSortBuilder<TRequest, TEntity>();
            andThen(builder.SortBy(sortColumn).Ascending());

            var sorterFactory = builder.Build();

            return config.SetSorter(sorterFactory);
        }

        public static TBuilder SortByDescending<TBuilder, TRequest, TEntity, TProperty>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TEntity, TProperty>> sortColumn,
            Action<BasicSortClauseBuilder<TRequest, TEntity>> andThen)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var builder = new BasicSortBuilder<TRequest, TEntity>();
            andThen(builder.SortBy(sortColumn).Descending());

            var sorterFactory = builder.Build();

            return config.SetSorter(sorterFactory);
        }

        public static TBuilder SortBy<TBuilder, TRequest, TEntity>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            string sortColumn,
            Action<BasicSortClauseBuilder<TRequest, TEntity>> andThen)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var builder = new BasicSortBuilder<TRequest, TEntity>();
            andThen(builder.SortBy(sortColumn).Ascending());

            var sorterFactory = builder.Build();

            return config.SetSorter(sorterFactory);
        }

        public static TBuilder SortByDescending<TBuilder, TRequest, TEntity>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            string sortColumn,
            Action<BasicSortClauseBuilder<TRequest, TEntity>> andThen)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var builder = new BasicSortBuilder<TRequest, TEntity>();
            andThen(builder.SortBy(sortColumn).Descending());

            var sorterFactory = builder.Build();

            return config.SetSorter(sorterFactory);
        }

        public static TBuilder SortAsTable<TBuilder, TRequest, TEntity, TControl>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Func<TRequest, TControl> getControlValue,
            Func<TRequest, SortDirection> getDirection,
            Action<TableSortBuilder<TRequest, TEntity, TControl>> configure)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var builder = new TableSortBuilder<TRequest, TEntity, TControl>()
                .WithControl(getControlValue, getDirection);

            configure(builder);

            return config.SetSorter(builder.Build());
        }

        public static TBuilder SortAsTable<TBuilder, TRequest, TEntity, TControl>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Func<TRequest, TControl> getControlValue,
            SortDirection direction,
            Action<TableSortBuilder<TRequest, TEntity, TControl>> configure)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var builder = new TableSortBuilder<TRequest, TEntity, TControl>()
                .WithControl(getControlValue, direction);

            configure(builder);

            return config.SetSorter(builder.Build());
        }

        public static TBuilder SortAsTable<TBuilder, TRequest, TEntity, TControl>(
           this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
           Func<TRequest, TControl> getControlValue,
           Action<TableSortBuilder<TRequest, TEntity, TControl>> configure)
           where TEntity : class
           where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var builder = new TableSortBuilder<TRequest, TEntity, TControl>()
                .WithControl(getControlValue, SortDirection.Default);

            configure(builder);

            return config.SetSorter(builder.Build());
        }

        public static TBuilder SortAsVariant<TBuilder, TRequest, TEntity, TSwitch>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Func<TRequest, TSwitch> getSwitchValue,
            Action<SwitchSortBuilder<TRequest, TEntity, TSwitch>> configure)
            where TEntity : class
            where TSwitch : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var builder = new SwitchSortBuilder<TRequest, TEntity, TSwitch>(getSwitchValue);

            configure(builder);

            return config.SetSorter(builder.Build());
        }
    }
}
