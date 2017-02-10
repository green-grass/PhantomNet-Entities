using System;
using PhantomNet.Entities;

namespace Microsoft.Extensions.DependencyInjection.Extensions
{
    public static class EntitiesServiceCollectionDescriptorExtensions
    {
        // Manager

        public static IServiceCollection AddManager<TEntity>(this IServiceCollection services,
            Type managerType)
            where TEntity : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var service = managerType.MakeGenericType(typeof(TEntity));

            services.TryAddScoped(service);

            return services;
        }
        public static IServiceCollection AddManager<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType)
            where TEntity : class
            where TSubEntity : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var service = managerType.MakeGenericType(typeof(TEntity), typeof(TSubEntity));

            services.TryAddScoped(service);

            return services;
        }

        // Validator

        public static IServiceCollection AddValidator<TEntity>(this IServiceCollection services,
            Type managerType, Type validatorType)
            where TEntity : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var managerService = managerType.MakeGenericType(typeof(TEntity));
            var service = typeof(IEntityValidator<,>).MakeGenericType(typeof(TEntity), managerService);
            var implementationType = validatorType.MakeGenericType(typeof(TEntity), managerService);

            services.TryAddScoped(service, implementationType);

            return services;
        }

        public static IServiceCollection AddValidator<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType, Type validatorType)
            where TEntity : class
            where TSubEntity : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var managerService = managerType.MakeGenericType(typeof(TEntity), typeof(TSubEntity));
            var service = typeof(IEntityValidator<,,>).MakeGenericType(typeof(TEntity), typeof(TSubEntity), managerService);
            var implementationType = validatorType.MakeGenericType(typeof(TEntity), managerService);

            services.TryAddScoped(service, implementationType);

            return services;
        }

        // LookupNormalizer

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

        // CodeGenerator

        public static IServiceCollection AddCodeGenerator<TEntity>(this IServiceCollection services,
            Type managerType, Type codeGeneratorType)
            where TEntity : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var managerService = managerType.MakeGenericType(typeof(TEntity));
            var service = typeof(IEntityCodeGenerator<,>).MakeGenericType(typeof(TEntity), managerService);
            var implementationType = codeGeneratorType.MakeGenericType(typeof(TEntity), managerService);

            services.TryAddScoped(service, implementationType);

            return services;
        }

        public static IServiceCollection AddCodeGenerator<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType, Type codeGeneratorType)
            where TEntity : class
            where TSubEntity : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var managerService = managerType.MakeGenericType(typeof(TEntity), typeof(TSubEntity));
            var service = typeof(IEntityCodeGenerator<,>).MakeGenericType(typeof(TEntity), managerService);
            var implementationType = codeGeneratorType.MakeGenericType(typeof(TEntity), managerService);

            services.TryAddScoped(service, implementationType);

            return services;
        }

        // Entity

        public static IServiceCollection AddEntity<TEntity>(this IServiceCollection services,
            Type managerType)
            where TEntity : class
        {
            return AddEntity<TEntity>(services, managerType, null, null, null);
        }

        public static IServiceCollection AddEntity<TEntity>(this IServiceCollection services,
            Type managerType, Type validatorType)
            where TEntity : class
        {
            return AddEntity<TEntity>(services, managerType, validatorType, null, null);
        }

        public static IServiceCollection AddEntity<TEntity>(this IServiceCollection services,
            Type managerType, Type validatorType, Type lookupNormalizerType)
            where TEntity : class
        {
            return AddEntity<TEntity>(services, managerType, validatorType, lookupNormalizerType, null);
        }

        public static IServiceCollection AddEntity<TEntity>(this IServiceCollection services,
            Type managerType, Type validatorType, Type lookupNormalizerType, Type codeGeneratorType)
            where TEntity : class
        {
            services = AddManager<TEntity>(services, managerType);

            if (validatorType != null)
            {
                services = AddValidator<TEntity>(services, managerType, validatorType);
            }

            if (lookupNormalizerType != null)
            {
                services = AddLookupNormalizer<TEntity>(services, lookupNormalizerType);
            }

            if (codeGeneratorType != null)
            {
                services = AddCodeGenerator<TEntity>(services, managerType, codeGeneratorType);
            }

            return services;
        }

        public static IServiceCollection AddEntity<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType)
            where TEntity : class
            where TSubEntity : class
        {
            return AddEntity<TEntity, TSubEntity>(services, managerType, null, null, null);
        }

        public static IServiceCollection AddEntity<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType, Type validatorType)
            where TEntity : class
            where TSubEntity : class
        {
            return AddEntity<TEntity, TSubEntity>(services, managerType, validatorType, null, null);
        }

        public static IServiceCollection AddEntity<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType, Type validatorType, Type lookupNormalizerType)
            where TEntity : class
            where TSubEntity : class
        {
            return AddEntity<TEntity, TSubEntity>(services, managerType, validatorType, lookupNormalizerType, null);
        }

        public static IServiceCollection AddEntity<TEntity, TSubEntity>(this IServiceCollection services,
            Type managerType, Type validatorType, Type lookupNormalizerType, Type codeGeneratorType)
            where TEntity : class
            where TSubEntity : class
        {
            services = AddManager<TEntity, TSubEntity>(services, managerType);

            if (validatorType != null)
            {
                services = AddValidator<TEntity, TSubEntity>(services, managerType, validatorType);
            }

            if (lookupNormalizerType != null)
            {
                services = AddLookupNormalizer<TEntity>(services, lookupNormalizerType);
            }

            if (codeGeneratorType != null)
            {
                services = AddCodeGenerator<TEntity, TSubEntity>(services, managerType, codeGeneratorType);
            }

            return services;
        }
    }
}
