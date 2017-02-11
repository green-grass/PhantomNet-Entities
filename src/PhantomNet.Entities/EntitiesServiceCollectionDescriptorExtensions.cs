﻿using System;
using System.Linq;
using System.Reflection;
using PhantomNet.Entities;

namespace Microsoft.Extensions.DependencyInjection.Extensions
{
    public static class EntitiesServiceCollectionDescriptorExtensions
    {
        #region Manager

        public static IServiceCollection AddManager<TEntity>(this IServiceCollection services,
            Type managerType,
            params Type[] additionalTypeArguments)
            where TEntity : class
        {
            return AddManager(services, typeof(TEntity), managerType, additionalTypeArguments);
        }

        public static IServiceCollection AddManager<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType,
            params Type[] additionalTypeArguments)
            where TEntity : class
            where TSubEntity : class
        {
            return AddManager(services, typeof(TEntity), typeof(TSubEntity), managerType, additionalTypeArguments);
        }

        public static IServiceCollection AddManager(this IServiceCollection services,
            Type entityType, Type managerType,
            params Type[] additionalTypeArguments)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }
            if (managerType == null)
            {
                throw new ArgumentNullException(nameof(managerType));
            }

            var managerTypeArguments = new Type[] { entityType }.Concat(additionalTypeArguments).ToArray();
            Type service;

            try
            {
                service = managerType.MakeGenericType(managerTypeArguments);
            }
            catch (ArgumentException e)
            {
                if (e.ParamName == "instantiation" &&
                    e.Message == "The number of generic arguments provided doesn't equal the arity of the generic type definition.")
                {
                    throw new ArgumentException($"The number of generic arguments ({string.Join(", ", managerTypeArguments.Select(x => x.Name))}) doesn't equal the arity of the generic type definition ({managerType.Name}).", e);
                }
                throw e;
            }

            services.TryAddScoped(service);

            return services;
        }

        public static IServiceCollection AddManager(this IServiceCollection services,
            Type entityType, Type subEntityType, Type managerType,
            params Type[] additionalTypeArguments)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }
            if (subEntityType == null)
            {
                throw new ArgumentNullException(nameof(subEntityType));
            }
            if (managerType == null)
            {
                throw new ArgumentNullException(nameof(managerType));
            }

            var managerTypeArguments = new Type[] { entityType, subEntityType }.Concat(additionalTypeArguments).ToArray();
            var service = managerType.MakeGenericType(managerTypeArguments);

            services.TryAddScoped(service);

            return services;
        }

        #endregion

        #region Validator

        public static IServiceCollection AddValidator<TEntity>(this IServiceCollection services,
            Type managerType, Type validatorType,
            params Type[] additionalTypeArguments)
            where TEntity : class
        {
            return AddValidator(services, typeof(TEntity), managerType, validatorType, additionalTypeArguments);
        }

        public static IServiceCollection AddValidator<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType, Type validatorType,
            params Type[] additionalTypeArguments)
            where TEntity : class
            where TSubEntity : class
        {
            return AddValidator(services, typeof(TEntity), typeof(TSubEntity), managerType, validatorType, additionalTypeArguments);
        }

        public static IServiceCollection AddValidator(this IServiceCollection services,
            Type entityType, Type managerType, Type validatorType,
            params Type[] additionalTypeArguments)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }
            if (managerType == null)
            {
                throw new ArgumentNullException(nameof(managerType));
            }
            if (validatorType == null)
            {
                throw new ArgumentNullException(nameof(validatorType));
            }

            var managerTypeArguments = new Type[] { entityType }.Concat(additionalTypeArguments).ToArray();
            var managerService = managerType.MakeGenericType(managerTypeArguments);
            var service = typeof(IEntityValidator<,>).MakeGenericType(entityType, managerService);
            var validatorTypeArguments = managerTypeArguments.ToList();
            // Remove module marker
            if (validatorType.GetTypeInfo().GenericTypeParameters.Count() == managerType.GetTypeInfo().GenericTypeParameters.Count())
            {
                validatorTypeArguments.RemoveAt(validatorTypeArguments.Count - 1);
            }
            validatorTypeArguments.Add(managerService);
            var implementationType = validatorType.MakeGenericType(validatorTypeArguments.ToArray());

            services.TryAddScoped(service, implementationType);

            return services;
        }

        public static IServiceCollection AddValidator(this IServiceCollection services,
            Type entityType, Type subEntityType, Type managerType, Type validatorType,
            params Type[] additionalTypeArguments)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }
            if (subEntityType == null)
            {
                throw new ArgumentNullException(nameof(subEntityType));
            }
            if (managerType == null)
            {
                throw new ArgumentNullException(nameof(managerType));
            }
            if (validatorType == null)
            {
                throw new ArgumentNullException(nameof(validatorType));
            }

            var managerTypeArguments = new Type[] { entityType, subEntityType }.Concat(additionalTypeArguments).ToArray();
            var managerService = managerType.MakeGenericType(managerTypeArguments);
            var service = typeof(IEntityValidator<,,>).MakeGenericType(entityType, subEntityType, managerService);
            var validatorTypeArguments = managerTypeArguments.ToList();
            // Remove module marker
            if (validatorType.GetTypeInfo().GenericTypeParameters.Count() == managerType.GetTypeInfo().GenericTypeParameters.Count())
            {
                validatorTypeArguments.RemoveAt(validatorTypeArguments.Count - 1);
            }
            validatorTypeArguments.Add(managerService);
            var implementationType = validatorType.MakeGenericType(validatorTypeArguments.ToArray());

            services.TryAddScoped(service, implementationType);

            return services;
        }

        #endregion

        #region LookupNormalizer

        public static IServiceCollection AddLookupNormalizer<TEntity>(this IServiceCollection services,
            Type lookupNormalizerType)
            where TEntity : class
        {
            return AddLookupNormalizer(services, typeof(TEntity), lookupNormalizerType);
        }

        public static IServiceCollection AddLookupNormalizer(this IServiceCollection services,
            Type entityType, Type lookupNormalizerType)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }
            if (lookupNormalizerType == null)
            {
                throw new ArgumentNullException(nameof(lookupNormalizerType));
            }

            var service = typeof(ILookupNormalizer<>).MakeGenericType(entityType);
            var implementationType = lookupNormalizerType.MakeGenericType(entityType);

            services.TryAddScoped(service, implementationType);

            return services;
        }

        #endregion

        #region CodeGenerator

        public static IServiceCollection AddCodeGenerator<TEntity>(this IServiceCollection services,
            Type managerType, Type codeGeneratorType,
            params Type[] additionalTypeArguments)
            where TEntity : class
        {
            return AddCodeGenerator(services, typeof(TEntity), managerType, codeGeneratorType, additionalTypeArguments);
        }

        public static IServiceCollection AddCodeGenerator<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType, Type codeGeneratorType,
            params Type[] additionalTypeArguments)
            where TEntity : class
            where TSubEntity : class
        {
            return AddCodeGenerator(services, typeof(TEntity), typeof(TSubEntity), managerType, codeGeneratorType, additionalTypeArguments);
        }

        public static IServiceCollection AddCodeGenerator(this IServiceCollection services,
            Type entityType, Type managerType, Type codeGeneratorType,
            params Type[] additionalTypeArguments)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }
            if (managerType == null)
            {
                throw new ArgumentNullException(nameof(managerType));
            }
            if (codeGeneratorType == null)
            {
                throw new ArgumentNullException(nameof(codeGeneratorType));
            }

            var managerTypeArguments = new Type[] { entityType }.Concat(additionalTypeArguments).ToArray();
            var managerService = managerType.MakeGenericType(managerTypeArguments);
            var service = typeof(IEntityCodeGenerator<,>).MakeGenericType(entityType, managerService);
            var implementationType = codeGeneratorType.MakeGenericType(entityType, managerService);

            services.TryAddScoped(service, implementationType);

            return services;
        }

        public static IServiceCollection AddCodeGenerator(this IServiceCollection services,
            Type entityType, Type subEntityType, Type managerType, Type codeGeneratorType,
            params Type[] additionalTypeArguments)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }
            if (subEntityType == null)
            {
                throw new ArgumentNullException(nameof(subEntityType));
            }
            if (managerType == null)
            {
                throw new ArgumentNullException(nameof(managerType));
            }
            if (codeGeneratorType == null)
            {
                throw new ArgumentNullException(nameof(codeGeneratorType));
            }

            var managerTypeArguments = new Type[] { entityType, subEntityType }.Concat(additionalTypeArguments).ToArray();
            var managerService = managerType.MakeGenericType(managerTypeArguments);
            var service = typeof(IEntityCodeGenerator<,>).MakeGenericType(entityType, managerService);
            var implementationType = codeGeneratorType.MakeGenericType(entityType, managerService);

            services.TryAddScoped(service, implementationType);

            return services;
        }

        #endregion

        #region Store

        public static IServiceCollection AddStore(this IServiceCollection services,
            Type entityType,
            Type storeServiceType,
            Type storeImplementationType, Type storeWithKeyTypeImplementationType, Type[] storeImplementationTypeArguments,
            Type keyType, int keyTypeIndex,
            params Type[] additionalTypeArguments)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }
            if (storeServiceType == null)
            {
                throw new ArgumentNullException(nameof(storeServiceType));
            }
            if (storeImplementationType == null)
            {
                throw new ArgumentNullException(nameof(storeImplementationType));
            }
            if (storeWithKeyTypeImplementationType == null)
            {
                throw new ArgumentNullException(nameof(storeWithKeyTypeImplementationType));
            }
            if (storeImplementationTypeArguments == null)
            {
                throw new ArgumentNullException(nameof(storeImplementationTypeArguments));
            }

            var serviceTypeArguments = new Type[] { entityType }.Concat(additionalTypeArguments).ToArray();
            var service = storeServiceType.MakeGenericType(serviceTypeArguments);
            var storeWithKeyTypeImplementationTypeArguments = storeImplementationTypeArguments.ToList();
            storeWithKeyTypeImplementationTypeArguments.Insert(keyTypeIndex, keyType);
            var implementationType = keyType == null ?
                storeImplementationType.MakeGenericType(storeImplementationTypeArguments) :
                storeWithKeyTypeImplementationType.MakeGenericType(storeWithKeyTypeImplementationTypeArguments.ToArray());

            services.TryAddScoped(service, implementationType);

            return services;
        }

        #endregion

        #region Accessor

        public static IServiceCollection AddAccessor(this IServiceCollection services,
            Type entityType,
            Type accessorServiceType,
            Type accessorImplementationType, Type accessorWithKeyTypeImplementationType, Type[] accessorImplementationTypeArguments,
            Type keyType, int keyTypeIndex,
            params Type[] additionalTypeArguments)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }
            if (accessorServiceType == null)
            {
                throw new ArgumentNullException(nameof(accessorServiceType));
            }
            if (accessorImplementationType == null)
            {
                throw new ArgumentNullException(nameof(accessorImplementationType));
            }
            if (accessorWithKeyTypeImplementationType == null)
            {
                throw new ArgumentNullException(nameof(accessorWithKeyTypeImplementationType));
            }
            if (accessorImplementationTypeArguments == null)
            {
                throw new ArgumentNullException(nameof(accessorImplementationTypeArguments));
            }

            var serviceTypeArguments = new Type[] { entityType }.Concat(additionalTypeArguments).ToArray();
            var service = accessorServiceType.MakeGenericType(serviceTypeArguments);
            var accessorWithKeyTypeImplementationTypeArguments = accessorImplementationTypeArguments.ToList();
            accessorWithKeyTypeImplementationTypeArguments.Insert(keyTypeIndex, keyType);
            var implementationType = keyType == null ?
                accessorImplementationType.MakeGenericType(accessorImplementationTypeArguments) :
                accessorWithKeyTypeImplementationType.MakeGenericType(accessorWithKeyTypeImplementationTypeArguments.ToArray());

            services.TryAddScoped(service, implementationType);

            return services;
        }

        #endregion

        #region Entity

        public static IServiceCollection AddEntity<TEntity>(this IServiceCollection services,
            Type managerType, Type validatorType, Type lookupNormalizerType, Type codeGeneratorType,
            params Type[] additionalTypeArguments)
            where TEntity : class
        {
            return AddEntity(services, typeof(TEntity), managerType, validatorType, lookupNormalizerType, codeGeneratorType, additionalTypeArguments);
        }

        public static IServiceCollection AddEntity<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType, Type validatorType, Type lookupNormalizerType, Type codeGeneratorType,
            params Type[] additionalTypeArguments)
            where TEntity : class
            where TSubEntity : class
        {
            return AddEntity(services, typeof(TEntity), typeof(TSubEntity), managerType, validatorType, lookupNormalizerType, codeGeneratorType, additionalTypeArguments);
        }

        public static IServiceCollection AddEntity(this IServiceCollection services,
            Type entityType, Type managerType, Type validatorType, Type lookupNormalizerType, Type codeGeneratorType,
            params Type[] additionalTypeArguments)
        {
            services = AddManager(services, entityType, managerType, additionalTypeArguments);

            if (validatorType != null)
            {
                services = AddValidator(services, entityType, managerType, validatorType, additionalTypeArguments);
            }

            if (lookupNormalizerType != null)
            {
                services = AddLookupNormalizer(services, entityType, lookupNormalizerType);
            }

            if (codeGeneratorType != null)
            {
                services = AddCodeGenerator(services, entityType, managerType, codeGeneratorType, additionalTypeArguments);
            }

            return services;
        }

        public static IServiceCollection AddEntity(this IServiceCollection services,
            Type entityType, Type subEntityType, Type managerType, Type validatorType, Type lookupNormalizerType, Type codeGeneratorType,
            params Type[] additionalTypeArguments)
        {
            services = AddManager(services, entityType, subEntityType, managerType, additionalTypeArguments);

            if (validatorType != null)
            {
                services = AddValidator(services, entityType, subEntityType, managerType, validatorType, additionalTypeArguments);
            }

            if (lookupNormalizerType != null)
            {
                services = AddLookupNormalizer(services, entityType, lookupNormalizerType);
            }

            if (codeGeneratorType != null)
            {
                services = AddCodeGenerator(services, entityType, subEntityType, managerType, codeGeneratorType, additionalTypeArguments);
            }

            return services;
        }

        #endregion
    }
}