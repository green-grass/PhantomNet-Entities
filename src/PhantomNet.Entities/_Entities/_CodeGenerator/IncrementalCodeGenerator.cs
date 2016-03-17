using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.OptionsModel;

namespace PhantomNet.Entities
{
    public class IncrementalCodeGenerator<TEntity, TEntityManager> : IEntityCodeGenerator<TEntity, TEntityManager>
        where TEntity : class
        where TEntityManager : class,
                               ITimeTrackedEntityManager<TEntity>,
                               ICodeBasedEntityManager<TEntity>
    {
        public IncrementalCodeGenerator(IOptions<IncrementalCodeGeneratorOptions> optionsAccessor)
        {
            Prefixes = optionsAccessor?.Value?.Prefixes ?? new Dictionary<Type, string>();
        }

        public IDictionary<Type, string> Prefixes { get; }

        public async Task<string> GenerateCodeAsync(TEntityManager manager, TEntity entity, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var prefix = Prefixes[typeof(TEntity)] ?? string.Empty;
            var latestEntity = await manager.FindLatestAsync();
            if (latestEntity == null)
            {
                return $"{prefix}1";
            }

            var latestCode = await manager.GetCodeAsync(latestEntity);
            var latestNumber = int.Parse(latestCode.Substring(prefix.Length));
            return $"{prefix}{latestNumber + 1}";
        }
    }
}
