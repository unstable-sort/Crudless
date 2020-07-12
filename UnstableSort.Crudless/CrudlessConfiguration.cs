﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Configuration;
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

    public abstract class CrudlessInitializationTask
    {
        public abstract void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options);

        public virtual bool Supports(string option) => false;
    }

    public class CrudlessInitializer
    {
        private readonly ServiceProviderContainer _container;

        private readonly List<Assembly> _assemblies
            = new List<Assembly>();

        private readonly List<CrudlessInitializationTask> _tasks
            = new List<CrudlessInitializationTask>();

        private CrudlessOptions _options = new CrudlessOptions();
        
        public CrudlessInitializer(ServiceProviderContainer container, Assembly[] assemblies = null, IMapper mapper = null)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));

            _assemblies.Add(typeof(CrudlessInitializer).Assembly);

            if (assemblies != null)
                _assemblies.AddRange(assemblies);

            _tasks.AddRange(new CrudlessInitializationTask[]
            {
                new CrudlessAutoMapperInitializer(mapper),
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

        public CrudlessInitializer AddInitializer(CrudlessInitializationTask task)
        {
            _tasks.Add(task);

            return this;
        }

        public CrudlessInitializer RemoveInitializers<T>()
            where T : CrudlessInitializationTask
        {
            _tasks.RemoveAll(task => typeof(T).IsAssignableFrom(task.GetType()));

            return this;
        }

        public bool Supports(string option) => _tasks.Any(x => x.Supports(option));

        public void Initialize()
        {
            var assemblies = _assemblies.Distinct().ToArray();
            var configManager = new CrudlessConfigManager(assemblies);

            using (var scope = _container.AllowOverrides())
            {
                _container.RegisterInstance(configManager);
                _container.RegisterInstance(_container);
            }

            _tasks.ForEach(t => t.Run(_container, assemblies, _options));
        }
    }

    internal class CrudlessAutoMapperInitializer : CrudlessInitializationTask
    {
        private readonly IMapper _mapper;

        public CrudlessAutoMapperInitializer(IMapper mapper)
        {
            _mapper = mapper;
        }

        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            if (_mapper != null)
            {
                using (var scope = container.AllowOverrides())
                    container.RegisterInstance(_mapper);
            }
        }
    }

    internal class CrudlessErrorHandlingInitializer : CrudlessInitializationTask
    {
        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            container.RegisterInitializer<ICrudlessRequestHandler>(handler =>
            {
                if (handler.ErrorDispatcher.Handler == null)
                    handler.ErrorDispatcher.Handler = container.ProvideInstance<IErrorHandler>();
            });

            container.RegisterSingleton<IErrorHandler, ErrorHandler>();
        }
    }

    internal class CrudlessValidationInitializer : CrudlessInitializationTask
    {
        private static Predicate<DecoratorConditionalContext> ShouldValidate(bool validateAllRequests)
        {
            return c => (validateAllRequests && !c.ImplementationType.RequestHasAttribute(typeof(DoNotValidateAttribute))) || 
                        c.ImplementationType.RequestHasAttribute(typeof(ValidateAttribute));
        }

        private static Predicate<DecoratorConditionalContext> ShouldMaybeValidate(bool validateAllRequests)
        {
            var shouldValidate = ShouldValidate(validateAllRequests);

            return c => !c.ImplementationType.RequestHasAttribute(typeof(DoNotValidateAttribute)) &&
                !shouldValidate(c) &&
                (!validateAllRequests || c.ImplementationType.RequestHasAttribute(typeof(MaybeValidateAttribute)));
        }

        private static Type ValidatorFactory(DecoratorConditionalContext c)
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

        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            var shouldValidate = ShouldValidate(options.ValidateAllRequests);
            var shouldMaybeValidate = ShouldMaybeValidate(options.ValidateAllRequests);
            
            container.Register(typeof(IRequestValidator<>), assemblies);

            container.RegisterInstance(new ValidatorFactory(container));
            
            container.RegisterDecorator(typeof(IRequestHandler<>), ValidatorFactory, shouldValidate);
            container.RegisterDecorator(typeof(IRequestHandler<,>), ValidatorFactory, shouldValidate);

            container.RegisterDecorator(typeof(IRequestHandler<>), typeof(MaybeValidateDecorator<>), shouldMaybeValidate);
            container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(MaybeValidateDecorator<,>), shouldMaybeValidate);
        }
    }

    internal class CrudlessRequestInitializer : CrudlessInitializationTask
    {
        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            bool IfNotHandled(ConditionalContext c) => !c.Handled;
            
            assemblies.GetConcreteImplementations(typeof(RequestHook<>)).ForEach(container.Register);
            assemblies.GetConcreteImplementations(typeof(EntityHook<,>)).ForEach(container.Register);
            assemblies.GetConcreteImplementations(typeof(ItemHook<,>)).ForEach(container.Register);
            assemblies.GetConcreteImplementations(typeof(ResultHook<,>)).ForEach(container.Register);
            assemblies.GetConcreteImplementations(typeof(AuditHook<,>)).ForEach(container.Register);
            assemblies.GetConcreteImplementations(typeof(Filter<,>)).ForEach(container.Register);

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

    internal class UniversalRequestInitializer : CrudlessInitializationTask
    {
        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            var universalProfiles = assemblies
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => !x.IsAbstract &&
                            x.BaseType != null &&
                            x.BaseType.IsGenericType &&
                            x.BaseType.GetGenericTypeDefinition() == typeof(UniversalRequestProfile<>))
                .ToArray();

            bool ShouldDecorate(DecoratorConditionalContext context)
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
        public static CrudlessInitializer CreateInitializer(ServiceProviderContainer provider, Assembly[] assemblies = null, IMapper mapper = null)
            => new CrudlessInitializer(provider, assemblies, mapper);

        public static CrudlessInitializer CreateInitializer(ServiceProviderContainer provider, Assembly[] assemblies)
            => new CrudlessInitializer(provider, assemblies, null);

        public static CrudlessInitializer CreateInitializer(ServiceProviderContainer provider, IMapper mapper)
            => new CrudlessInitializer(provider, null, mapper);
    }
}