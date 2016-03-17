using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
#if DOTNET5_4
using System.Reflection;
#endif

namespace PhantomNet.Entities
{
    // Foundation
    public abstract partial class EntityManagerBase<TEntity, TSubEntity, TEntityManager> :
        EntityManagerBase<TEntity, TEntityManager>
        where TEntity : class
        where TSubEntity : class
        where TEntityManager : EntityManagerBase<TEntity, TSubEntity, TEntityManager>
    {
        #region Constructors

        protected EntityManagerBase(
            IDisposable store,
            IEnumerable<IEntityValidator<TEntity, TSubEntity, TEntityManager>> entityValidators,
            ILookupNormalizer keyNormalizer,
            IEntityCodeGenerator<TEntity, TEntityManager> codeGenerator,
            EntityErrorDescriber errors,
            ILogger<EntityManagerBase<TEntity, TSubEntity, TEntityManager>> logger)
            : base(store, entityValidators, keyNormalizer, codeGenerator, errors, logger)
        {
            if (entityValidators != null)
            {
                foreach (var validator in entityValidators)
                {
                    EntityValidators.Add(validator);
                }
            }
        }

        #endregion

        #region Properties

        private IList<IEntityValidator<TEntity, TSubEntity, TEntityManager>> EntityValidators { get; }
            = new List<IEntityValidator<TEntity, TSubEntity, TEntityManager>>();

        protected virtual IQueryable<TSubEntity> SubEntities
        {
            get
            {
                if (SupportsQueryableEntityWithSubEntity)
                {
                    return QueryableStoreWithSubEntity.SubEntities;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        #endregion

        #region Helpers

        protected override async Task PrepareEntityForCreatingAsync(TEntity entity)
        {
            await base.PrepareEntityForCreatingAsync(entity);

            if (SupportsScopedNameBasedEntity)
            {
                await NormalizeEntityNameAsync(entity);
            }
        }

        protected override async Task PrepareEntityForUpdatingAsync(TEntity entity)
        {
            await base.PrepareEntityForUpdatingAsync(entity);

            if (SupportsScopedNameBasedEntity)
            {
                await NormalizeEntityNameAsync(entity);
            }
        }

        protected override async Task<EntityResult> ValidateEntityInternalAsync(TEntity entity)
        {
            var result = await base.ValidateEntityInternalAsync(entity);

            if (SupportsMasterDetailsEntity)
            {
                var details = await MasterDetailsStore.GetDetailsAsync(entity, CancellationToken);
                var tasks = new List<Task<EntityResult>>(details.Count);
                foreach (var detail in details)
                {
                    tasks.Add(ValidateSubEntityInternalAsync(detail));
                }
                await Task.WhenAll(tasks);

                var errors = new List<EntityError>();
                foreach (var task in tasks)
                {
                    errors.AddRange(task.Result.Errors);
                }

                if (errors.Count > 0)
                {
                    result = EntityResult.Failed(result.Errors.Concat(errors).ToArray());
                }
            }

            return result;
        }

        protected virtual async Task<EntityResult> ValidateSubEntityInternalAsync(TSubEntity subEntity)
        {
            var errors = new List<EntityError>();
            foreach (var validator in EntityValidators)
            {
                var result = await validator.ValidateAsync((TEntityManager)this, subEntity);
                if (!result.Succeeded)
                {
                    errors.AddRange(result.Errors);
                }
            }
            if (errors.Count > 0)
            {
                Logger.LogWarning(0, "SubEntity {subEntityId} validation failed: {errors}.", await GetSubEntityIdAsync(subEntity), string.Join(", ", errors.Select(e => e.Code)));
                return EntityResult.Failed(errors.ToArray());
            }
            return EntityResult.Success;
        }

        #endregion
    }

    // Entity
    partial class EntityManagerBase<TEntity, TSubEntity, TEntityManager>
    {
        #region Properties

        protected virtual bool SupportsReadOnlyEntityWithSubEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IReadOnlyEntityStore<TEntity, TSubEntity>;
            }
        }

        protected virtual IReadOnlyEntityStore<TEntity> ReadOnlyEntityWithSubEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IReadOnlyEntityStore<TEntity, TSubEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Resources.StoreNotIReadOnlyEntityWithSubEntityStore);
                }

                return store;
            }
        }

        protected virtual bool SupportsEntityWithSubEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IEntityStore<TEntity, TSubEntity>;
            }
        }

        protected virtual IEntityStore<TEntity, TSubEntity> EntityWithSubEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IEntityStore<TEntity, TSubEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Resources.StoreNotIEntityWithSubEntityStore);
                }

                return store;
            }
        }

        #endregion

        #region Public Operations

        protected virtual Task<string> GetSubEntityIdAsync(TSubEntity subEntity)
        {
            ThrowIfDisposed();
            if (subEntity == null)
            {
                throw new ArgumentNullException(nameof(subEntity));
            }

            return EntityWithSubEntityStore.GetIdAsync(subEntity, CancellationToken);
        }

        #endregion
    }

    // QueryableEntity
    partial class EntityManagerBase<TEntity, TSubEntity, TEntityManager>
    {
        #region Properties

        protected virtual bool SupportsQueryableEntityWithSubEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IQueryableEntityStore<TEntity, TSubEntity>;
            }
        }

        protected virtual IQueryableEntityStore<TEntity, TSubEntity> QueryableStoreWithSubEntity
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IQueryableEntityStore<TEntity, TSubEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Resources.StoreNotIQueryableEntityWithSubEntityStore);
                }

                return store;
            }
        }

        #endregion
    }

    // GroupedEntity
    partial class EntityManagerBase<TEntity, TSubEntity, TEntityManager>
    {
        #region Properties

        protected virtual bool SupportsGroupedEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IGroupedEntityStore<TEntity, TSubEntity>;
            }
        }

        protected virtual IGroupedEntityStore<TEntity, TSubEntity> GroupedEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IGroupedEntityStore<TEntity, TSubEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Resources.StoreNotIGroupedEntityStore);
                }

                return store;
            }
        }

        #endregion

        #region Public Operations

        protected virtual Task<EntityQueryResult<TEntity>> SearchEntitiesAsync(TSubEntity group, string search, int? pageNumber, int? pageSize, string sort, bool reverse)
        {
            ThrowIfDisposed();
            return SearchEntitiesInternalAsync(GroupedEntityStore.FilterByGroup(Entities, group), search, pageNumber, pageSize, sort, reverse);
        }

        #endregion
    }

    // ScopedNameBasedEntity
    partial class EntityManagerBase<TEntity, TSubEntity, TEntityManager>
    {
        #region Properties

        protected virtual bool SupportsScopedNameBasedEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IScopedNameBasedEntityStore<TEntity, TSubEntity>;
            }
        }

        protected virtual IScopedNameBasedEntityStore<TEntity, TSubEntity> ScopedNameBasedStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IScopedNameBasedEntityStore<TEntity, TSubEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Resources.StoreNotIScopedNameBasedEntityStore);
                }

                return store;
            }
        }

        #endregion

        #region Public Operations

        protected virtual async Task<TEntity> FindEntityByNameAsync(string name, TSubEntity scope)
        {
            ThrowIfDisposed();
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var entity = await ScopedNameBasedStore.FindByNameAsync(NormalizeEntityKey(name), scope, CancellationToken);

            if (entity != null && SupportsEagerLoadingEntity)
            {
                await EagerLoadingEntityStore.EagerLoadAsync(entity, CancellationToken);
            }

            return entity;
        }

        protected override Task<string> GetEntityNameAsync(TEntity entity)
        {
            if (!SupportsScopedNameBasedEntity && SupportsNameBasedEntity)
            {
                return base.GetEntityNameAsync(entity);
            }

            ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return ScopedNameBasedStore.GetNameAsync(entity, CancellationToken);
        }

        protected override async Task<EntityResult> SetEntityNameAsync(TEntity entity, string name)
        {
            if (!SupportsScopedNameBasedEntity && SupportsNameBasedEntity)
            {
                return await base.SetEntityNameAsync(entity, name);
            }

            ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await ScopedNameBasedStore.SetNameAsync(entity, name, CancellationToken);
            return await UpdateEntityInternalAsync(entity);
        }

        protected virtual Task<TSubEntity> GetEntityScope(TEntity entity)
        {
            ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return ScopedNameBasedStore.GetScopeAsync(entity, CancellationToken);
        }

        protected virtual async Task<EntityResult> SetEntityScopeAsync(TEntity entity, TSubEntity scope)
        {
            ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await ScopedNameBasedStore.SetScopeAsync(entity, scope, CancellationToken);
            return await UpdateEntityInternalAsync(entity);
        }

        #endregion

        #region Helpers

        protected override async Task NormalizeEntityNameAsync(TEntity entity)
        {
            if (!SupportsScopedNameBasedEntity && SupportsNameBasedEntity)
            {
                await base.NormalizeEntityNameAsync(entity);
                return;
            }

            var normalizedName = NormalizeEntityKey(await GetEntityNameAsync(entity));
            await ScopedNameBasedStore.SetNormalizedNameAsync(entity, normalizedName, CancellationToken);
        }

        #endregion
    }

    // MasterDetailsEntity
    partial class EntityManagerBase<TEntity, TSubEntity, TEntityManager>
    {
        #region Properties

        protected virtual bool SupportsMasterDetailsEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IMasterDetailsEntityStore<TEntity, TSubEntity>;
            }
        }

        protected virtual IMasterDetailsEntityStore<TEntity, TSubEntity> MasterDetailsStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IMasterDetailsEntityStore<TEntity, TSubEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Resources.StoreNotIMasterDetailsEntityStore);
                }

                return store;
            }
        }

        #endregion
    }

    // Foundation
    public abstract partial class EntityManagerBase<TEntity, TEntityManager>
        where TEntity : class
        where TEntityManager : EntityManagerBase<TEntity, TEntityManager>
    {
        #region Constructors

        protected EntityManagerBase(
            IDisposable store,
            IEnumerable<IEntityValidator<TEntity, TEntityManager>> entityValidators,
            ILookupNormalizer keyNormalizer,
            IEntityCodeGenerator<TEntity, TEntityManager> codeGenerator,
            EntityErrorDescriber errors,
            ILogger<EntityManagerBase<TEntity, TEntityManager>> logger)
        {
            if (!(this is TEntityManager))
            {
                throw new NotSupportedException(string.Format(Resources.GenericClassNotMatched, nameof(TEntityManager)));
            }
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            Store = store;
            KeyNormalizer = keyNormalizer;
            CodeGenerator = codeGenerator;
            ErrorDescriber = errors;
            Logger = logger;

            if (entityValidators != null)
            {
                foreach (var validator in entityValidators)
                {
                    EntityValidators.Add(validator);
                }
            }
        }

        #endregion

        #region Properties

        private IList<IEntityValidator<TEntity, TEntityManager>> EntityValidators { get; }
            = new List<IEntityValidator<TEntity, TEntityManager>>();

        private ILookupNormalizer KeyNormalizer { get; set; }

        private IEntityCodeGenerator<TEntity, TEntityManager> CodeGenerator { get; set; }

        private EntityErrorDescriber ErrorDescriber { get; set; }

        protected virtual CancellationToken CancellationToken => CancellationToken.None;

        protected ILogger Logger { get; set; }

        protected IDisposable Store { get; }

        protected virtual IQueryable<TEntity> Entities
        {
            get
            {
                if (SupportsQueryableEntity)
                {
                    return QueryableEntityStore.Entities;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        protected virtual bool SupportsEagerLoadingEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IEagerLoadingEntityStore<TEntity>;
            }
        }

        protected virtual IEagerLoadingEntityStore<TEntity> EagerLoadingEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IEagerLoadingEntityStore<TEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Resources.StoreNotIEagerLoadingEntityStore);
                }

                return store;
            }
        }

        #endregion

        #region Helpers

        protected virtual string NormalizeEntityKey(string key)
        {
            return (KeyNormalizer == null) ? key : KeyNormalizer.Normalize(key);
        }

        protected virtual async Task PrepareEntityForCreateValidationAsync(TEntity entity)
        {
            if (SupportsCodeBasedEntity)
            {
                if (await GetEntityCodeAsync(entity) == null && CodeGenerator != null)
                {
                    await UpdateEntityCodeAsync(entity, await GenerateEntityCodeAsync(entity));
                }
            }
        }

        protected virtual async Task PrepareEntityForCreatingAsync(TEntity entity)
        {
            if (SupportsCodeBasedEntity)
            {
                await NormalizeEntityCodeAsync(entity);
            }

            if (SupportsNameBasedEntity)
            {
                await NormalizeEntityNameAsync(entity);
            }

            if (SupportsTimeTrackedEntity)
            {
                var date = DateTime.Now;
                await UpdateEntityDataCreateDateAsync(entity, date);
                await UpdateEntityDataLastModifyDateAsync(entity, date);
            }
            await Task.FromResult(0);
        }

        protected virtual async Task PrepareEntityForUpdatingAsync(TEntity entity)
        {
            if (SupportsCodeBasedEntity)
            {
                await NormalizeEntityCodeAsync(entity);
            }

            if (SupportsNameBasedEntity)
            {
                await NormalizeEntityNameAsync(entity);
            }

            if (SupportsTimeTrackedEntity)
            {
                await UpdateEntityDataLastModifyDateAsync(entity);
            }
            await Task.FromResult(0);
        }

        protected virtual async Task<EntityResult> ValidateEntityInternalAsync(TEntity entity)
        {
            var errors = new List<EntityError>();
            foreach (var validator in EntityValidators)
            {
                var result = await validator.ValidateAsync((TEntityManager)this, entity);
                if (!result.Succeeded)
                {
                    errors.AddRange(result.Errors);
                }
            }
            if (errors.Count > 0)
            {
                Logger.LogWarning(0, "Entity {entityId} validation failed: {errors}.", await GetEntityIdAsync(entity), string.Join(", ", errors.Select(e => e.Code)));
                return EntityResult.Failed(errors.ToArray());
            }
            return EntityResult.Success;
        }

        #endregion

        #region IDisposable Support

        private bool _disposed = false;

        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Store.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }

    // Entity
    partial class EntityManagerBase<TEntity, TEntityManager>
    {
        #region Properties

        protected virtual bool SupportsReadOnlyEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IReadOnlyEntityStore<TEntity>;
            }
        }

        protected virtual IReadOnlyEntityStore<TEntity> ReadOnlyEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IReadOnlyEntityStore<TEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Resources.StoreNotIReadOnlyEntityStore);
                }

                return store;
            }
        }

        protected virtual bool SupportsEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IEntityStore<TEntity>;
            }
        }

        protected virtual IEntityStore<TEntity> EntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IEntityStore<TEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Resources.StoreNotIEntityStore);
                }

                return store;
            }
        }

        #endregion

        #region Public Operations

        protected virtual async Task<EntityResult> CreateEntityAsync(TEntity entity)
        {
            ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await PrepareEntityForCreateValidationAsync(entity);

            var result = await ValidateEntityInternalAsync(entity);
            if (!result.Succeeded)
            {
                return result;
            }

            await PrepareEntityForCreatingAsync(entity);

            result = await EntityStore.CreateAsync(entity, CancellationToken);
            return result;
        }

        protected virtual Task<EntityResult> UpdateEntityAsync(TEntity entity)
        {
            ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return UpdateEntityInternalAsync(entity);
        }

        protected virtual Task<EntityResult> DeleteEntityAsync(TEntity entity)
        {
            ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return EntityStore.DeleteAsync(entity, CancellationToken);
        }

        protected virtual async Task<TEntity> FindEntityByIdAsync(string id)
        {
            ThrowIfDisposed();
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var entity = await EntityStore.FindByIdAsync<TEntity>(id, CancellationToken);

            if (entity != null && SupportsEagerLoadingEntity)
            {
                await EagerLoadingEntityStore.EagerLoadAsync(entity, CancellationToken);
            }

            return entity;
        }

        protected virtual Task<string> GetEntityIdAsync(TEntity entity)
        {
            ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return EntityStore.GetIdAsync(entity, CancellationToken);
        }

        protected virtual Task<EntityQueryResult<TEntity>> SearchEntitiesAsync(string search, int? pageNumber, int? pageSize, string sort, bool reverse)
        {
            ThrowIfDisposed();
            return SearchEntitiesInternalAsync(Entities, search, pageNumber, pageSize, sort, reverse);
        }

        #endregion

        #region Helpers

        protected virtual async Task<EntityResult> UpdateEntityInternalAsync(TEntity entity)
        {
            var result = await ValidateEntityInternalAsync(entity);
            if (!result.Succeeded)
            {
                return result;
            }

            await PrepareEntityForUpdatingAsync(entity);

            return await EntityStore.UpdateAsync(entity, CancellationToken);
        }

        protected virtual async Task<EntityQueryResult<TEntity>> SearchEntitiesInternalAsync(IQueryable<TEntity> models, string search, int? pageNumber, int? pageSize, string sort, bool reverse)
        {
            if (models == null)
            {
                throw new ArgumentNullException(nameof(models));
            }

            var result = new EntityQueryResult<TEntity>();
            var offset = ((pageNumber - 1) * pageSize) ?? 0;
            var limit = pageSize ?? int.MaxValue;

            result.TotalCount = await ReadOnlyEntityStore.CountAsync(models, CancellationToken);

            if (!string.IsNullOrWhiteSpace(search))
            {
                models = ReadOnlyEntityStore.Filter(models, NormalizeEntityKey(search));
            }
            result.FilterredCount = await ReadOnlyEntityStore.CountAsync(models, CancellationToken);

            models = ReadOnlyEntityStore.PreSort(models);
            if (string.IsNullOrWhiteSpace(sort))
            {
                models = ReadOnlyEntityStore.DefaultSort(models);
            }
            else
            {
                if (sort.StartsWith("-"))
                {
                    sort = sort.TrimStart('-');
                    reverse = !reverse;
                }

                var param = Expression.Parameter(typeof(TEntity));
                var propertyInfo = typeof(TEntity).GetProperty(sort);
                var propertyExpression = Expression.Lambda(Expression.Property(param, propertyInfo), param);
                models = (IOrderedQueryable<TEntity>)models.Provider.CreateQuery(Expression.Call(
                    typeof(Queryable),
                    models.Expression.Type == typeof(IOrderedQueryable<TEntity>) ?
                        (reverse ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy)) :
                        (reverse ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy)),
                    new Type[] { typeof(TEntity), propertyInfo.PropertyType },
                    models.Expression,
                    propertyExpression
                ));
            }

            result.Results = models.Skip(offset).Take(limit);

            if (SupportsEagerLoadingEntity)
            {
                // Loading all entities parallelly will throw AggregateException exception.
                foreach (var entity in result.Results)
                {
                    await EagerLoadingEntityStore.EagerLoadAsync(entity, CancellationToken);
                }
            }

            return result;
        }

        #endregion
    }

    // QueryableEntity
    partial class EntityManagerBase<TEntity, TEntityManager>
    {
        #region Properties

        protected virtual bool SupportsQueryableEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IQueryableEntityStore<TEntity>;
            }
        }

        protected virtual IQueryableEntityStore<TEntity> QueryableEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IQueryableEntityStore<TEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Resources.StoreNotIQueryableEntityStore);
                }

                return store;
            }
        }

        #endregion
    }

    // TimeTrackedEntity
    partial class EntityManagerBase<TEntity, TEntityManager>
    {
        #region Properties

        protected virtual bool SupportsTimeTrackedEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is ITimeTrackedEntityStore<TEntity>;
            }
        }

        protected virtual ITimeTrackedEntityStore<TEntity> TimeTrackedEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as ITimeTrackedEntityStore<TEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Resources.StoreNotITimeTrackedEntityStore);
                }

                return store;
            }
        }

        #endregion

        #region Public Operations

        protected virtual async Task<TEntity> FindLatestEntityAsync()
        {
            ThrowIfDisposed();
            var entity = await TimeTrackedEntityStore.FindLatestAsync(CancellationToken);

            if (entity != null && SupportsEagerLoadingEntity)
            {
                await EagerLoadingEntityStore.EagerLoadAsync(entity, CancellationToken);
            }

            return entity;
        }

        #endregion

        #region Helpers

        protected virtual Task UpdateEntityDataCreateDateAsync(TEntity entity, DateTime date)
        {
            return TimeTrackedEntityStore.SetDataCreateDateAsync(entity, date, CancellationToken);
        }

        protected virtual Task UpdateEntityDataLastModifyDateAsync(TEntity entity, DateTime? date = null)
        {
            return TimeTrackedEntityStore.SetDataLastModifyDateAsync(entity, date ?? DateTime.Now, CancellationToken);
        }

        #endregion
    }

    // ReadOnlyCodeBasedEntity
    partial class EntityManagerBase<TEntity, TEntityManager>
    {
        #region Properties

        protected virtual bool SupportsReadOnlyCodeBasedEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IReadOnlyCodeBasedEntityStore<TEntity>;
            }
        }

        protected virtual IReadOnlyCodeBasedEntityStore<TEntity> ReadOnlyCodeBasedEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IReadOnlyCodeBasedEntityStore<TEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Resources.StoreNotIReadOnlyCodeBasedEntityStore);
                }

                return store;
            }
        }

        #endregion

        #region Public Operations

        protected virtual async Task<TEntity> FindEntityByCodeAsync(string code)
        {
            ThrowIfDisposed();
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            var entity = await ReadOnlyCodeBasedEntityStore.FindByCodeAsync(NormalizeEntityKey(code), CancellationToken);

            if (entity != null && SupportsEagerLoadingEntity)
            {
                await EagerLoadingEntityStore.EagerLoadAsync(entity, CancellationToken);
            }

            return entity;
        }

        #endregion
    }

    // CodeBasedEntity
    partial class EntityManagerBase<TEntity, TEntityManager>
    {
        #region Properties

        protected virtual bool SupportsCodeBasedEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is ICodeBasedEntityStore<TEntity>;
            }
        }

        protected virtual ICodeBasedEntityStore<TEntity> CodeBasedEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as ICodeBasedEntityStore<TEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Resources.StoreNotICodeBasedEntityStore);
                }

                return store;
            }
        }

        #endregion

        #region Public Operations

        protected virtual Task<string> GetEntityCodeAsync(TEntity entity)
        {
            ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return CodeBasedEntityStore.GetCodeAsync(entity, CancellationToken);
        }

        protected virtual async Task<EntityResult> SetEntityCodeAsync(TEntity entity, string code)
        {
            ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await CodeBasedEntityStore.SetCodeAsync(entity, code, CancellationToken);
            return await UpdateEntityInternalAsync(entity);
        }

        #endregion

        #region Helpers

        protected virtual async Task NormalizeEntityCodeAsync(TEntity entity)
        {
            var normalizedCode = NormalizeEntityKey(await GetEntityCodeAsync(entity));
            await CodeBasedEntityStore.SetCodeAsync(entity, normalizedCode, CancellationToken);
        }

        protected virtual Task<string> GenerateEntityCodeAsync(TEntity entity)
        {
            if (CodeGenerator == null)
            {
                throw new InvalidOperationException($"{nameof(CodeGenerator)} is null.");
            }

            return CodeGenerator.GenerateCodeAsync((TEntityManager)this, entity, CancellationToken);
        }

        protected virtual Task UpdateEntityCodeAsync(TEntity entity, string code)
        {
            return CodeBasedEntityStore.SetCodeAsync(entity, code, CancellationToken);
        }

        #endregion
    }

    // NameBasedEntity
    partial class EntityManagerBase<TEntity, TEntityManager>
    {
        #region Properties

        protected virtual bool SupportsNameBasedEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is INameBasedEntityStore<TEntity>;
            }
        }

        protected virtual INameBasedEntityStore<TEntity> NameBasedEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as INameBasedEntityStore<TEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Resources.StoreNotINameBasedEntityStore);
                }

                return store;
            }
        }

        #endregion

        #region Public Operations

        protected virtual async Task<TEntity> FindEntityByNameAsync(string name)
        {
            ThrowIfDisposed();
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var entity = await NameBasedEntityStore.FindByNameAsync(NormalizeEntityKey(name), CancellationToken);

            if (entity != null && SupportsEagerLoadingEntity)
            {
                await EagerLoadingEntityStore.EagerLoadAsync(entity, CancellationToken);
            }

            return entity;
        }

        protected virtual Task<string> GetEntityNameAsync(TEntity entity)
        {
            ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return NameBasedEntityStore.GetNameAsync(entity, CancellationToken);
        }

        protected virtual async Task<EntityResult> SetEntityNameAsync(TEntity entity, string name)
        {
            ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await NameBasedEntityStore.SetNameAsync(entity, name, CancellationToken);
            return await UpdateEntityInternalAsync(entity);
        }

        #endregion

        #region Helpers

        protected virtual async Task NormalizeEntityNameAsync(TEntity entity)
        {
            var normalizedName = NormalizeEntityKey(await GetEntityNameAsync(entity));
            await NameBasedEntityStore.SetNormalizedNameAsync(entity, normalizedName, CancellationToken);
        }

        #endregion
    }
}
