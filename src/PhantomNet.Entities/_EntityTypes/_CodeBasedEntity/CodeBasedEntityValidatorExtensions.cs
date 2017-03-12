using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public static class CodeBasedEntityValidatorExtensions
    {
        public static async Task ValidateCodeAsync<TEntity, TEntityManager, TEntityAccessor>(this IEntityValidator<TEntity> validator,
            TEntityManager manager, TEntityAccessor entityAccessor, TEntity entity, List<GenericError> errors,
            Func<string, GenericError> invalidCode, Func<string, GenericError> duplicateCode)
            where TEntity: class
            where TEntityManager : class, ICodeBasedEntityManager<TEntity>
            where TEntityAccessor : IEntityAccessor<TEntity>,
                                    ICodeBasedEntityAccessor<TEntity>
        {
            var code = entityAccessor.GetCode(entity);
            if (string.IsNullOrWhiteSpace(code))
            {
                errors.Add(invalidCode(code));
            }
            else
            {
                var owner = await manager.FindByCodeAsync(code);
                if (owner != null &&
                    !string.Equals(entityAccessor.GetId(owner), entityAccessor.GetId(entity)))
                {
                    errors.Add(duplicateCode(code));
                }
            }
        }
    }
}
