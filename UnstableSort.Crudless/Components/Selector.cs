﻿using System;
using System.Linq.Expressions;
using UnstableSort.Crudless.Exceptions;

namespace UnstableSort.Crudless
{
    public interface ISelector
    {
        void Bind<TRequest, TEntity>(Func<TRequest, Expression<Func<TEntity, bool>>> selector)
            where TEntity : class;

        Func<object, Expression<Func<TEntity, bool>>> Get<TEntity>()
            where TEntity : class;
    }

    public class Selector : ISelector
    {
        private Func<object, LambdaExpression> _selector;
        private Type _boundRequestType;
        private Type _boundEntityType;

        internal static Selector From<TRequest, TEntity>(
            Func<TRequest, Expression<Func<TEntity, bool>>> selector)
            where TEntity : class
        {
            var s = new Selector();
            s.Bind(selector);
            return s;
        }

        public void Bind<TRequest, TEntity>(
            Func<TRequest, Expression<Func<TEntity, bool>>> selector)
            where TEntity : class
        {
            _selector = request => selector((TRequest) request);

            _boundRequestType = typeof(TRequest);
            _boundEntityType = typeof(TEntity);
        }

        public Func<object, Expression<Func<TEntity, bool>>> Get<TEntity>() 
            where TEntity : class
        {
            return Select<TEntity>;
        }

        private Expression<Func<TEntity, bool>> Select<TEntity>(object request)
            where TEntity : class
        {
            if (!_boundRequestType.IsInstanceOfType(request))
            {
                var message =
                    $"Unable to create a selector for entity '{typeof(TEntity)}' " +
                    $"from a request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{_boundRequestType}'.";

                throw new RequestFailedException(message)
                {
                    RequestTypeProperty = request.GetType()
                };
            }

            var selectExpr = _selector(request);

            if (selectExpr == null)
                return null;

            if (typeof(TEntity) == _boundEntityType)
                return (Expression<Func<TEntity, bool>>) selectExpr;

            if (!_boundEntityType.IsAssignableFrom(typeof(TEntity)))
                return null;

            return (Expression<Func<TEntity, bool>>)selectExpr.SubstituteParameter(_boundEntityType, typeof(TEntity));
        }
    }
}
