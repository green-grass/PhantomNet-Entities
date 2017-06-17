using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public static class ScopedNameBasedEntityValidatorExtensions
    {
        public static async Task ValidateNameAsync<TEntity, TSubEntity, TEntityManager, TEntityAccessor>(
            this IEntityValidator<TEntity, TSubEntity> validator,
            TEntityManager manager, TEntityAccessor entityAccessor, TEntity entity, List<GenericError> errors,
            Func<string, GenericError> invalidName, Func<string, GenericError> duplicateName)
            where TEntity : class
            where TSubEntity : class
            where TEntityManager : class, IScopedNameBasedEntityManager<TEntity, TSubEntity>
            where TEntityAccessor : IEntityAccessor<TEntity, TSubEntity>,
                                    IScopedNameBasedEntityAccessor<TEntity, TSubEntity>
        {
            var name = entityAccessor.GetName(entity);
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(invalidName(name));
            }
            else
            {
                var scope = entityAccessor.GetGroup(entity);
                var owner = await manager.FindByNameAsync(name, scope);
                if (owner != null &&
                    !string.Equals(entityAccessor.GetId(owner), entityAccessor.GetId(entity)))
                {
                    errors.Add(duplicateName(name));
                }
            }
        }
    }
}
