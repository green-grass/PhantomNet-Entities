using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace PhantomNet.Entities
{
    public class IncrementalCodeGenerator<TEntity, TEntityManager> : IEntityCodeGenerator<TEntity, TEntityManager>
        where TEntity : class
        where TEntityManager : class,
                               ITimeTrackedEntityManager<TEntity>,
                               ICodeBasedEntityManager<TEntity>
    {
        public IncrementalCodeGenerator(
            ICodeBasedEntityAccessor<TEntity> entityCodeAccessor,
            IOptions<IncrementalCodeGeneratorOptions<TEntity>> incrementalCodeGeneratorOptions)
        {
            if (entityCodeAccessor == null)
            {
                throw new ArgumentNullException(nameof(entityCodeAccessor));
            }

            if (incrementalCodeGeneratorOptions == null)
            {
                throw new ArgumentNullException(nameof(incrementalCodeGeneratorOptions));
            }

            EntityCodeAccessor = entityCodeAccessor;
            Prefix = incrementalCodeGeneratorOptions.Value.Prefix ?? string.Empty;
        }

        public string Prefix { get; }

        protected ICodeBasedEntityAccessor<TEntity> EntityCodeAccessor { get; }

        public async Task<string> GenerateCodeAsync(TEntityManager manager, TEntity entity, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var latestEntity = await manager.FindLatestAsync();
            if (latestEntity == null)
            {
                return $"{Prefix}1";
            }

            var latestCode = EntityCodeAccessor.GetCode(latestEntity);
            var latestNumber = int.Parse(latestCode.Substring(Prefix.Length));
            return $"{Prefix}{latestNumber + 1}";
        }
    }
}
