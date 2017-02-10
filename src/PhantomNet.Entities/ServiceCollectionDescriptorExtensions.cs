using System;
using System.Linq;
using PhantomNet.Entities;

namespace Microsoft.Extensions.DependencyInjection.Extensions
{
    public static class EntitiesServiceCollectionDescriptorExtensions
    {
        #region Manager

        public static IServiceCollection AddManager<TEntity>(this IServiceCollection services,
            Type managerType,
            params Type[] managerAdditionalTypeArguments)
            where TEntity : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var managerTypeArguments = managerAdditionalTypeArguments.ToList();
            managerTypeArguments.Insert(0, typeof(TEntity));
            var service = managerType.MakeGenericType(managerTypeArguments.ToArray());

            services.TryAddScoped(service);

            return services;
        }

        public static IServiceCollection AddManager<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType,
            params Type[] managerAdditionalTypeArguments)
            where TEntity : class
            where TSubEntity : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var managerTypeArguments = managerAdditionalTypeArguments.ToList();
            managerTypeArguments.Insert(0, typeof(TSubEntity));
            managerTypeArguments.Insert(0, typeof(TEntity));
            var service = managerType.MakeGenericType(managerTypeArguments.ToArray());

            services.TryAddScoped(service);

            return services;
        }

        #endregion

        #region Validator

        public static IServiceCollection AddValidator<TEntity>(this IServiceCollection services,
            Type managerType, Type validatorType,
            params Type[] managerAdditionalTypeArguments)
            where TEntity : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var managerTypeArguments = managerAdditionalTypeArguments.ToList();
            managerTypeArguments.Insert(0, typeof(TEntity));
            var managerService = managerType.MakeGenericType(managerTypeArguments.ToArray());
            var service = typeof(IEntityValidator<,>).MakeGenericType(typeof(TEntity), managerService);
            var implementationType = validatorType.MakeGenericType(typeof(TEntity), managerService);

            services.TryAddScoped(service, implementationType);

            return services;
        }

        public static IServiceCollection AddValidator<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType, Type validatorType,
            params Type[] managerAdditionalTypeArguments)
            where TEntity : class
            where TSubEntity : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var managerTypeArguments = managerAdditionalTypeArguments.ToList();
            managerTypeArguments.Insert(0, typeof(TSubEntity));
            managerTypeArguments.Insert(0, typeof(TEntity));
            var managerService = managerType.MakeGenericType(managerTypeArguments.ToArray());
            var service = typeof(IEntityValidator<,,>).MakeGenericType(typeof(TEntity), typeof(TSubEntity), managerService);
            var implementationType = validatorType.MakeGenericType(typeof(TEntity), managerService);

            services.TryAddScoped(service, implementationType);

            return services;
        }

        #endregion

        #region LookupNormalizer

        public static IServiceCollection AddLookupNormalizer<TEntity>(this IServiceCollection services,
            Type lookupNormalizerType)
            where TEntity : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var service = typeof(ILookupNormalizer<>).MakeGenericType(typeof(TEntity));
            var implementationType = lookupNormalizerType.MakeGenericType(typeof(TEntity));

            services.TryAddScoped(service, implementationType);

            return services;
        }

        #endregion

        #region CodeGenerator

        public static IServiceCollection AddCodeGenerator<TEntity>(this IServiceCollection services,
            Type managerType, Type codeGeneratorType,
            params Type[] managerAdditionalTypeArguments)
            where TEntity : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var managerTypeArguments = managerAdditionalTypeArguments.ToList();
            managerTypeArguments.Insert(0, typeof(TEntity));
            var managerService = managerType.MakeGenericType(managerTypeArguments.ToArray());
            var service = typeof(IEntityCodeGenerator<,>).MakeGenericType(typeof(TEntity), managerService);
            var implementationType = codeGeneratorType.MakeGenericType(typeof(TEntity), managerService);

            services.TryAddScoped(service, implementationType);

            return services;
        }

        public static IServiceCollection AddCodeGenerator<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType, Type codeGeneratorType,
            params Type[] managerAdditionalTypeArguments)
            where TEntity : class
            where TSubEntity : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var managerTypeArguments = managerAdditionalTypeArguments.ToList();
            managerTypeArguments.Insert(0, typeof(TSubEntity));
            managerTypeArguments.Insert(0, typeof(TEntity));
            var managerService = managerType.MakeGenericType(managerTypeArguments.ToArray());
            var service = typeof(IEntityCodeGenerator<,>).MakeGenericType(typeof(TEntity), managerService);
            var implementationType = codeGeneratorType.MakeGenericType(typeof(TEntity), managerService);

            services.TryAddScoped(service, implementationType);

            return services;
        }

        #endregion

        #region Entity

        public static IServiceCollection AddEntity<TEntity>(this IServiceCollection services,
            Type managerType,
            params Type[] managerAdditionalTypeArguments)
            where TEntity : class
        {
            return AddEntity<TEntity>(services, managerType, null, null, null, managerAdditionalTypeArguments);
        }

        public static IServiceCollection AddEntity<TEntity>(this IServiceCollection services,
            Type managerType, Type validatorType,
            params Type[] managerAdditionalTypeArguments)
            where TEntity : class
        {
            return AddEntity<TEntity>(services, managerType, validatorType, null, null, managerAdditionalTypeArguments);
        }

        public static IServiceCollection AddEntity<TEntity>(this IServiceCollection services,
            Type managerType, Type validatorType, Type lookupNormalizerType,
            params Type[] managerAdditionalTypeArguments)
            where TEntity : class
        {
            return AddEntity<TEntity>(services, managerType, validatorType, lookupNormalizerType, null, managerAdditionalTypeArguments);
        }

        public static IServiceCollection AddEntity<TEntity>(this IServiceCollection services,
            Type managerType, Type validatorType, Type lookupNormalizerType, Type codeGeneratorType,
            params Type[] managerAdditionalTypeArguments)
            where TEntity : class
        {
            services = AddManager<TEntity>(services, managerType, managerAdditionalTypeArguments);

            if (validatorType != null)
            {
                services = AddValidator<TEntity>(services, managerType, validatorType, managerAdditionalTypeArguments);
            }

            if (lookupNormalizerType != null)
            {
                services = AddLookupNormalizer<TEntity>(services, lookupNormalizerType);
            }

            if (codeGeneratorType != null)
            {
                services = AddCodeGenerator<TEntity>(services, managerType, codeGeneratorType, managerAdditionalTypeArguments);
            }

            return services;
        }

        public static IServiceCollection AddEntity<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType,
            params Type[] managerAdditionalTypeArguments)
            where TEntity : class
            where TSubEntity : class
        {
            return AddEntity<TEntity, TSubEntity>(services, managerType, null, null, null, managerAdditionalTypeArguments);
        }

        public static IServiceCollection AddEntity<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType, Type validatorType,
            params Type[] managerAdditionalTypeArguments)
            where TEntity : class
            where TSubEntity : class
        {
            return AddEntity<TEntity, TSubEntity>(services, managerType, validatorType, null, null, managerAdditionalTypeArguments);
        }

        public static IServiceCollection AddEntity<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType, Type validatorType, Type lookupNormalizerType,
            params Type[] managerAdditionalTypeArguments)
            where TEntity : class
            where TSubEntity : class
        {
            return AddEntity<TEntity, TSubEntity>(services, managerType, validatorType, lookupNormalizerType, null, managerAdditionalTypeArguments);
        }

        public static IServiceCollection AddEntity<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType, Type validatorType, Type lookupNormalizerType, Type codeGeneratorType,
            params Type[] managerAdditionalTypeArguments)
            where TEntity : class
            where TSubEntity : class
        {
            services = AddManager<TEntity, TSubEntity>(services, managerType, managerAdditionalTypeArguments);

            if (validatorType != null)
            {
                services = AddValidator<TEntity, TSubEntity>(services, managerType, validatorType, managerAdditionalTypeArguments);
            }

            if (lookupNormalizerType != null)
            {
                services = AddLookupNormalizer<TEntity>(services, lookupNormalizerType);
            }

            if (codeGeneratorType != null)
            {
                services = AddCodeGenerator<TEntity, TSubEntity>(services, managerType, codeGeneratorType, managerAdditionalTypeArguments);
            }

            return services;
        }

        #endregion
    }
}
