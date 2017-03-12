using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public static class NameBasedEntityValidatorExtensions
    {
        public static async Task ValidateNameAsync<TEntity, TEntityManager, TEntityAccessor>(this IEntityValidator<TEntity> validator,
            TEntityManager manager, TEntityAccessor entityAccessor, TEntity entity, List<GenericError> errors,
            Func<string, GenericError> invalidName, Func<string, GenericError> duplicateName)
            where TEntity: class
            where TEntityManager : class, INameBasedEntityManager<TEntity>
            where TEntityAccessor : IEntityAccessor<TEntity>,
                                    INameBasedEntityAccessor<TEntity>
        {
            var name = entityAccessor.GetName(entity);
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(invalidName(name));
            }
            else
            {
                var owner = await manager.FindByNameAsync(name);
                if (owner != null &&
                    !string.Equals(entityAccessor.GetId(owner), entityAccessor.GetId(entity)))
                {
                    errors.Add(duplicateName(name));
                }
            }
        }
    }
}
