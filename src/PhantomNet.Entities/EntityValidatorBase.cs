﻿using PhantomNet.Entities.Properties;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public abstract class EntityValidatorBase<TEntity, TSubEntity, TEntityManager>
        : EntityValidatorBase<TEntity, TEntityManager>,
          IEntityValidator<TEntity, TSubEntity>
        where TEntity : class
        where TSubEntity : class
        where TEntityManager : class
    {
        public virtual async Task<GenericResult> ValidateAsync(object manager, TSubEntity subEntity)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            if (!typeof(TEntityManager).IsAssignableFrom(manager.GetType()))
            {
                throw new NotSupportedException(string.Format(
                    Strings.UnsupportedEntityManager,
                    GetType().Name,
                    typeof(TEntityManager).Name,
                    manager.GetType().Name));
            }
            if (subEntity == null)
            {
                throw new ArgumentNullException(nameof(subEntity));
            }

            var errors = new List<GenericError>();

            await ValidateInternalAsync((TEntityManager)manager, subEntity, errors);

            if (errors.Count > 0)
            {
                return GenericResult.Failed(errors.ToArray());
            }

            return GenericResult.Success;
        }

        protected virtual Task ValidateInternalAsync(TEntityManager manager, TSubEntity subEntity, List<GenericError> errors)
        {
            throw new InvalidOperationException(string.Format(
                Strings.ValidatorNeverValidatesEntity,
                GetType().Name,
                typeof(TSubEntity).Name));
        }
    }

    public abstract class EntityValidatorBase<TEntity, TEntityManager>
        : IEntityValidator<TEntity>
        where TEntity : class
        where TEntityManager : class
    {
        public virtual async Task<GenericResult> ValidateAsync(object manager, TEntity entity)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            if (!typeof(TEntityManager).IsAssignableFrom(manager.GetType()))
            {
                throw new NotSupportedException(string.Format(
                    Strings.UnsupportedEntityManager,
                    GetType().Name,
                    typeof(TEntityManager).Name,
                    manager.GetType().Name));
            }
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var errors = new List<GenericError>();

            await ValidateInternalAsync((TEntityManager)manager, entity, errors);

            if (errors.Count > 0)
            {
                return GenericResult.Failed(errors.ToArray());
            }

            return GenericResult.Success;
        }

        protected abstract Task ValidateInternalAsync(TEntityManager manager, TEntity entity, List<GenericError> errors);
    }
}
