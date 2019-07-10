using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleInjector;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Context;
using UnstableSort.Crudless.Errors;
using UnstableSort.Crudless.Mediator;
using UnstableSort.Crudless.Requests;
using UnstableSort.Crudless.Validation;

namespace UnstableSort.Crudless
{
    public class CrudlessOptions
    {
        public bool ValidateAllRequests { get; set; } = false;
    }

    public interface ICrudlessInitializationTask
    {
        void Run(Container container, Assembly[] assemblies, CrudlessOptions options);
    }

    public class CrudlessInitializer
    {
        private readonly Container _container;

        private readonly List<Assembly> _assemblies
            = new List<Assembly>();

        private readonly List<ICrudlessInitializationTask> _tasks
            = new List<ICrudlessInitializationTask>();

        private CrudlessOptions _options = new CrudlessOptions();

        public CrudlessInitializer(Container container, Assembly[] assemblies = null)
        {
            _container = container;

            _assemblies.Add(typeof(CrudlessInitializer).Assembly);

            if (assemblies != null)
                _assemblies.AddRange(assemblies);

            _tasks.AddRange(new ICrudlessInitializationTask[]
            {
                new DefaultMediatorInitializer(),
                new UniversalRequestInitializer(),
                new CrudlessValidationInitializer(),
                new CrudlessRequestInitializer(),
                new CrudlessErrorHandlingInitializer(),
            });
        }

        public CrudlessInitializer WithAssemblies(params Assembly[] assemblies)
        {
            _assemblies.AddRange(assemblies);

            return this;
        }
        
        public CrudlessInitializer ValidateAllRequests(bool validate = true)
        {
            _options.ValidateAllRequests = validate;

            return this;
        }

        public CrudlessInitializer AddInitializer(ICrudlessInitializationTask task)
        {
            _tasks.Add(task);

            return this;
        }

        public CrudlessInitializer RemoveInitializers<T>()
            where T : ICrudlessInitializationTask
        {
            _tasks.RemoveAll(task => typeof(T).IsAssignableFrom(task.GetType()));

            return this;
        }

        public void Initialize()
        {
            var assemblies = _assemblies.Distinct().ToArray();
            var configManager = new CrudlessConfigManager(_container.GetInstance, assemblies);

            _container.RegisterInstance(configManager);
            _container.RegisterSingleton<IDataAgentFactory, DataAgentFactory>();

            TypeRequestHookFactory.BindContainer(_container.GetInstance);
            TypeEntityHookFactory.BindContainer(_container.GetInstance);
            TypeItemHookFactory.BindContainer(_container.GetInstance);
            TypeResultHookFactory.BindContainer(_container.GetInstance);
            TypeAuditHookFactory.BindContainer(_container.GetInstance);
            TypeFilterFactory.BindContainer(_container.GetInstance);
            TypeSorterFactory.BindContainer(_container.GetInstance);
            DataAgentFactory.BindContainer(_container.GetInstance);
            
            _tasks.ForEach(t => t.Run(_container, assemblies, _options));
        }
    }

    internal class CrudlessErrorHandlingInitializer : ICrudlessInitializationTask
    {
        public void Run(Container container, Assembly[] assemblies, CrudlessOptions options)
        {
            container.RegisterInitializer<ICrudlessRequestHandler>(handler =>
            {
                if (handler.ErrorDispatcher.Handler == null)
                    handler.ErrorDispatcher.Handler = container.GetInstance<IErrorHandler>();
            });

            container.Register(typeof(IErrorHandler), typeof(ErrorHandler), Lifestyle.Singleton);
        }
    }

    internal class CrudlessValidationInitializer : ICrudlessInitializationTask
    {
        private static Predicate<DecoratorPredicateContext> ShouldValidate(bool validateAllRequests)
        {
            return c => (validateAllRequests && !c.ImplementationType.RequestHasAttribute(typeof(DoNotValidateAttribute))) || 
                        c.ImplementationType.RequestHasAttribute(typeof(ValidateAttribute));
        }

        private static Predicate<DecoratorPredicateContext> ShouldMaybeValidate(bool validateAllRequests)
        {
            var shouldValidate = ShouldValidate(validateAllRequests);

            return c => !c.ImplementationType.RequestHasAttribute(typeof(DoNotValidateAttribute)) &&
                !shouldValidate(c) &&
                (!validateAllRequests || c.ImplementationType.RequestHasAttribute(typeof(MaybeValidateAttribute)));
        }

        private static Type ValidatorFactory(DecoratorPredicateContext c)
        {
            var tRequestHandler = c.ImplementationType
                .GetInterfaces()
                .Single(x => x.IsGenericType && (
                    x.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                    x.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)));

            var handlerArguments = tRequestHandler.GetGenericArguments();
            var tRequest = handlerArguments[0];

            ValidateAttribute FindAttribute(Type t)
            {
                var attr = t.GetCustomAttribute<ValidateAttribute>(false);

                if (attr == null && t.BaseType != null)
                    attr = FindAttribute(t.BaseType);

                if (attr == null)
                {
                    foreach (var x in t.GetInterfaces())
                    {
                        attr = FindAttribute(x);
                        if (attr != null) return attr;
                    }
                }

                return attr;
            }

            var validateAttribute = FindAttribute(tRequest);
            var tValidator = validateAttribute?.ValidatorType ??
                typeof(IRequestValidator<>).MakeGenericType(tRequest);

            if (handlerArguments.Length == 1)
                return typeof(ValidateDecorator<,>).MakeGenericType(tRequest, tValidator);

            if (handlerArguments.Length == 2)
            {
                var tResult = handlerArguments[1];
                return typeof(ValidateDecorator<,,>).MakeGenericType(tRequest, tResult, tValidator);
            }

            return null;
        }

        public void Run(Container container, Assembly[] assemblies, CrudlessOptions options)
        {
            var shouldValidate = ShouldValidate(options.ValidateAllRequests);
            var shouldMaybeValidate = ShouldMaybeValidate(options.ValidateAllRequests);
            
            container.Register(typeof(IRequestValidator<>), assemblies);

            container.RegisterInstance(new ValidatorFactory(container.GetInstance));

            container.RegisterDecorator(typeof(IRequestHandler<>), ValidatorFactory, Lifestyle.Transient, shouldValidate);
            container.RegisterDecorator(typeof(IRequestHandler<,>), ValidatorFactory, Lifestyle.Transient, shouldValidate);

            container.RegisterDecorator(typeof(IRequestHandler<>), typeof(MaybeValidateDecorator<>), Lifestyle.Transient, shouldMaybeValidate);
            container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(MaybeValidateDecorator<,>), Lifestyle.Transient, shouldMaybeValidate);
        }
    }

    internal class CrudlessRequestInitializer : ICrudlessInitializationTask
    {
        public void Run(Container container, Assembly[] assemblies, CrudlessOptions options)
        {
            bool IfNotHandled(PredicateContext c) => !c.Handled;

            container.Register(typeof(IRequestHandler<>), assemblies);
            container.Register(typeof(IRequestHandler<,>), assemblies);

            container.Register(typeof(CreateRequestHandler<,>), assemblies);
            container.Register(typeof(CreateRequestHandler<,,>), assemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(CreateRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(CreateRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(CreateAllRequestHandler<,>), assemblies);
            container.Register(typeof(CreateAllRequestHandler<,,>), assemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(CreateAllRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(CreateAllRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(GetRequestHandler<,,>), assemblies);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(GetRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(GetAllRequestHandler<,,>), assemblies);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(GetAllRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(PagedGetAllRequestHandler<,,>), assemblies);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(PagedGetAllRequestHandler<,,>), IfNotHandled);
            
            container.Register(typeof(UpdateRequestHandler<,>), assemblies);
            container.Register(typeof(UpdateRequestHandler<,,>), assemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(UpdateRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(UpdateRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(UpdateAllRequestHandler<,>), assemblies);
            container.Register(typeof(UpdateAllRequestHandler<,,>), assemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(UpdateAllRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(UpdateAllRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(DeleteRequestHandler<,>), assemblies);
            container.Register(typeof(DeleteRequestHandler<,,>), assemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(DeleteRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(DeleteRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(DeleteAllRequestHandler<,>), assemblies);
            container.Register(typeof(DeleteAllRequestHandler<,,>), assemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(DeleteAllRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(DeleteAllRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(SaveRequestHandler<,>), assemblies);
            container.Register(typeof(SaveRequestHandler<,,>), assemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(SaveRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(SaveRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(MergeRequestHandler<,>), assemblies);
            container.Register(typeof(MergeRequestHandler<,,>), assemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(MergeRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(MergeRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(SynchronizeRequestHandler<,>), assemblies);
            container.Register(typeof(SynchronizeRequestHandler<,,>), assemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(SynchronizeRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(SynchronizeRequestHandler<,,>), IfNotHandled);
        }
    }

    internal class UniversalRequestInitializer : ICrudlessInitializationTask
    {
        public void Run(Container container, Assembly[] assemblies, CrudlessOptions options)
        {
            var universalProfiles = assemblies
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => !x.IsAbstract &&
                            x.BaseType != null &&
                            x.BaseType.IsGenericType &&
                            x.BaseType.GetGenericTypeDefinition() == typeof(UniversalRequestProfile<>))
                .ToArray();

            bool ShouldDecorate(DecoratorPredicateContext context)
            {
                var tRequest = context.ServiceType.GetGenericArguments()[0];

                foreach (var type in tRequest.BuildTypeHierarchyUp())
                {
                    if (type.GetInterface(typeof(ICrudlessRequest).Name) != null)
                        return false;

                    if (universalProfiles.Any(x => x.BaseType.GetGenericArguments()[0] == type))
                        return true;
                }

                return false;
            }

            container.RegisterDecorator(typeof(IRequestHandler<>), typeof(UniversalRequestDecorator<>), ShouldDecorate);
            container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(UniversalRequestDecorator<,>), ShouldDecorate);
        }
    }

    // TODO: Rename to avoid conflict with namespace
    public static class Crudless
    {
        public static CrudlessInitializer CreateInitializer(Container container, Assembly[] assemblies = null)
            => new CrudlessInitializer(container, assemblies);
    }
}