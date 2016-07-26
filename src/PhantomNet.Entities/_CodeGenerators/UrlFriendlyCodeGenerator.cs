using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace PhantomNet.Entities
{
    public class UrlFriendlyCodeGenerator<TEntity, TEntityManager> : IEntityCodeGenerator<TEntity, TEntityManager>
        where TEntity : class
        where TEntityManager : class
    {
        public UrlFriendlyCodeGenerator(INameBasedEntityAccessor<TEntity> entityNameAccessor, IOptions<UrlFriendlyCodeGeneratorOptions> optionsAccessor)
        {
            if (entityNameAccessor == null)
            {
                throw new ArgumentNullException(nameof(entityNameAccessor));
            }

            EntityNameAccessor = entityNameAccessor;
        }

        protected INameBasedEntityAccessor<TEntity> EntityNameAccessor { get; }

        public Task<string> GenerateCodeAsync(TEntityManager manager, TEntity entity, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var name = EntityNameAccessor.GetName(entity);
            return Task.FromResult(name.ToUrlFriendly());
        }
    }
}
