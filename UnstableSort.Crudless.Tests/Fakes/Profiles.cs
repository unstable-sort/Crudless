﻿using System;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Errors;
using UnstableSort.Crudless.Requests;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Tests.Fakes
{
    public enum SortKeys
    {
        Key1, Key2, Key3, Key4
    }

    public static class SortKeys2
    {
        public const string Key1 = "Key1";
        public const string Key2 = "Key2";
        public const string Key3 = "Key3";
    }

    public class SortEntity : IEntity
    {
        public int Id { get; set; }

        public bool IsDeleted { get; set; }

        public int Col1 { get; set; }

        public string Col2 { get; set; }

        public long Col3 { get; set; }
    }

    public class SortTestRequest : GetAllRequest<SortEntity, UserGetDto>
    {
        public SortKeys SortKey { get; set; }
    }

    public class DebugErrorHandler : ErrorHandler
    {
        protected override Response HandleError(CreateEntityFailedError error) => throw new Exception(error.Reason, error.Exception);

        protected override Response HandleError(CreateResultFailedError error) => throw new Exception(error.Reason, error.Exception);

        protected override Response HandleError(CrudlessError error) => throw new Exception(error.Exception?.Message, error.Exception);
        
        protected override Response HandleError(HookFailedError error) => throw new Exception(error.Reason, error.Exception);

        protected override Response HandleError(RequestCanceledError error) => throw new Exception(error.Exception?.Message, error.Exception);

        protected override Response HandleError(RequestFailedError error) => throw new Exception(error.Reason, error.Exception);

        protected override Response HandleError(UpdateEntityFailedError error) => throw new Exception(error.Reason, error.Exception);
    }

    public class RequestProfile : RequestProfile<ICrudlessRequest>
    {
        public RequestProfile()
        {
            UseErrorConfiguration(config =>
            {
                config.FailedToFindInDeleteIsError = true;
                config.FailedToFindInGetAllIsError = true;
                config.FailedToFindInGetIsError = true;
                config.FailedToFindInUpdateIsError = true;
            });

            ForEntity<NonEntity>()
                .UseOptions(config => config.UseProjection = false);
        }
    }

    public class DefaultUpdateRequestProfile<TEntity, TKey, TIn, TOut>
        : RequestProfile<UpdateRequest<TEntity, TKey, TIn, TOut>>
        where TEntity : class
    {
        public DefaultUpdateRequestProfile()
        {
            ForEntity<IEntity>().SelectBy(r => r.Key, e => e.Id);
        }
    }

    public class DefaultSaveRequestProfile<TEntity, TKey, TIn, TOut>
        : RequestProfile<SaveRequest<TEntity, TKey, TIn, TOut>>
        where TEntity : class
    {
        public DefaultSaveRequestProfile()
        {
            ForEntity<IEntity>().SelectBy(r => r.Key, e => e.Id);
        }
    }

    public class DefaultEntityBulkProfile
        : RequestProfile<ICrudlessRequest>
    {
        public DefaultEntityBulkProfile()
        {
            ForEntity<IEntity>()
                .BulkCreateWith(config => config.WithPrimaryKey(x => x.Id))
                .BulkUpdateWith(config => config.WithPrimaryKey(x => x.Id).IgnoreColumns(x => x.Id))
                .BulkDeleteWith(config => config.WithPrimaryKey(x => x.Id));
        }
    }
}
