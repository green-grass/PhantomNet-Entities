using System;
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
            var service = TryMakeGenericType(managerType, managerTypeArguments);

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
            var service = TryMakeGenericType(managerType, managerTypeArguments);

            services.TryAddScoped(service);

            return services;
        }

        #endregion

        #region Validator

        public static IServiceCollection AddValidator<TEntity>(this IServiceCollection services,
            Type validatorType,
            params Type[] additionalTypeArguments)
            where TEntity : class
        {
            return AddValidator(services, typeof(TEntity), validatorType, additionalTypeArguments);
        }

        public static IServiceCollection AddValidator<TEntity, TSubEntity>(this IServiceCollection services,
            Type validatorType,
            params Type[] additionalTypeArguments)
            where TEntity : class
            where TSubEntity : class
        {
            return AddValidator(services, typeof(TEntity), typeof(TSubEntity), validatorType, additionalTypeArguments);
        }

        public static IServiceCollection AddValidator(this IServiceCollection services,
            Type entityType, Type validatorType,
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
            if (validatorType == null)
            {
                throw new ArgumentNullException(nameof(validatorType));
            }

            var validatorTypeArguments = new Type[] { entityType }.Concat(additionalTypeArguments).ToList();
            // Remove module marker
            if (validatorType.GetTypeInfo().GenericTypeParameters.Count() == validatorTypeArguments.Count - 1)
            {
                validatorTypeArguments.RemoveAt(validatorTypeArguments.Count - 1);
            }
            var service = TryMakeGenericType(typeof(IEntityValidator<>), entityType);
            var implementationType = TryMakeGenericType(validatorType, validatorTypeArguments.ToArray());

            services.TryAddScoped(service, implementationType);

            return services;
        }

        public static IServiceCollection AddValidator(this IServiceCollection services,
            Type entityType, Type subEntityType, Type validatorType,
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
            if (validatorType == null)
            {
                throw new ArgumentNullException(nameof(validatorType));
            }

            var validatorTypeArguments = new Type[] { entityType, subEntityType }.Concat(additionalTypeArguments).ToList();
            // Remove module marker
            if (validatorType.GetTypeInfo().GenericTypeParameters.Count() == validatorTypeArguments.Count - 1)
            {
                validatorTypeArguments.RemoveAt(validatorTypeArguments.Count - 1);
            }
            var service = TryMakeGenericType(typeof(IEntityValidator<,>), entityType, subEntityType);
            var implementationType = TryMakeGenericType(validatorType, validatorTypeArguments.ToArray());

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

            var service = TryMakeGenericType(typeof(ILookupNormalizer<>), entityType);
            var implementationType = TryMakeGenericType(lookupNormalizerType, entityType);

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
            var managerService = TryMakeGenericType(managerType, managerTypeArguments);
            var service = TryMakeGenericType(typeof(IEntityCodeGenerator<,>), entityType, managerService);
            var implementationType = TryMakeGenericType(codeGeneratorType, entityType, managerService);

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
            var managerService = TryMakeGenericType(managerType, managerTypeArguments);
            var service = TryMakeGenericType(typeof(IEntityCodeGenerator<,>), entityType, managerService);
            var implementationType = TryMakeGenericType(codeGeneratorType, entityType, managerService);

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
            var service = TryMakeGenericType(storeServiceType, serviceTypeArguments);
            var storeWithKeyTypeImplementationTypeArguments = storeImplementationTypeArguments.ToList();
            storeWithKeyTypeImplementationTypeArguments.Insert(keyTypeIndex, keyType);
            var implementationType = keyType == null ?
                TryMakeGenericType(storeImplementationType, storeImplementationTypeArguments) :
                TryMakeGenericType(storeWithKeyTypeImplementationType, storeWithKeyTypeImplementationTypeArguments.ToArray());

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
            var service = TryMakeGenericType(accessorServiceType, serviceTypeArguments);
            var accessorWithKeyTypeImplementationTypeArguments = accessorImplementationTypeArguments.ToList();
            accessorWithKeyTypeImplementationTypeArguments.Insert(keyTypeIndex, keyType);
            var implementationType = keyType == null ?
                TryMakeGenericType(accessorImplementationType, accessorImplementationTypeArguments) :
                TryMakeGenericType(accessorWithKeyTypeImplementationType, accessorWithKeyTypeImplementationTypeArguments.ToArray());

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
                services = AddValidator(services, entityType, validatorType, additionalTypeArguments);
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
                services = AddValidator(services, entityType, subEntityType, validatorType, additionalTypeArguments);
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

        #region Helpers

        private static Type TryMakeGenericType(Type genericType, params Type[] typeArguments)
        {
            if (!genericType.GetTypeInfo().IsGenericTypeDefinition)
            {
                return genericType;
            }

            try
            {
                return genericType.MakeGenericType(typeArguments);
            }
            catch (ArgumentException e)
            {
                if (e.ParamName == "instantiation" &&
                    e.Message.StartsWith("The number of generic arguments provided doesn't equal the arity of the generic type definition."))
                {
                    throw new ArgumentException($"The number of generic arguments ({string.Join(", ", typeArguments.Select(x => x.Name))}) doesn't equal the arity of the generic type definition ({genericType.Name}).", e);
                }
                throw e;
            }
        }

        #endregion
    }
}
