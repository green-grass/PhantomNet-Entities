using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PhantomNet.Entities
{
    // Foundation
    public abstract partial class EntityManagerBase<TEntity, TSubEntity, TEntityManager>
        : EntityManagerBase<TEntity, TEntityManager>
        where TEntity : class
        where TSubEntity : class
        where TEntityManager : EntityManagerBase<TEntity, TSubEntity, TEntityManager>
    {
        #region Constructors

        protected EntityManagerBase(
            IDisposable store, object entityAccessor,
            IEnumerable<IEntityValidator<TEntity, TSubEntity, TEntityManager>> entityValidators,
            ILookupNormalizer keyNormalizer,
            IEntityCodeGenerator<TEntity, TEntityManager> codeGenerator,
            EntityErrorDescriber errors,
            ILogger<EntityManagerBase<TEntity, TSubEntity, TEntityManager>> logger)
            : base(store, entityAccessor, entityValidators, keyNormalizer, codeGenerator, errors, logger)
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
                NormalizeEntityName(entity);
            }
        }

        protected override async Task PrepareEntityForUpdatingAsync(TEntity entity)
        {
            await base.PrepareEntityForUpdatingAsync(entity);

            if (SupportsScopedNameBasedEntity)
            {
                NormalizeEntityName(entity);
            }
        }

        protected override async Task<EntityResult> ValidateEntityInternalAsync(TEntity entity)
        {
            var result = await base.ValidateEntityInternalAsync(entity);

            if (SupportsMasterDetailsEntity)
            {
                var details = MasterDetailsEntityAccessor.GetDetails(entity);
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
                Logger.LogWarning(0, "SubEntity {subEntityId} validation failed: {errors}.", SupportsEntityWithSubEntity ? EntityWithSubEntityAccessor.GetId(subEntity) : null, string.Join(", ", errors.Select(e => e.Code)));
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

        protected virtual bool SupportsEntityWithSubEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IEntityStore<TEntity, TSubEntity> && Accessor is IEntityAccessor<TEntity, TSubEntity>;
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

        protected virtual IEntityAccessor<TEntity, TSubEntity> EntityWithSubEntityAccessor
        {
            get
            {
                var accessor = Accessor as IEntityAccessor<TEntity, TSubEntity>;
                if (accessor == null)
                {
                    // TODO:: Message
                    throw new NotSupportedException();
                }

                return accessor;
            }
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
                return Store is IScopedNameBasedEntityStore<TEntity, TSubEntity> && Accessor is IScopedNameBasedEntityAccessor<TEntity, TSubEntity>;
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

        protected virtual IScopedNameBasedEntityAccessor<TEntity, TSubEntity> ScopedNameBasedEntityAccessor
        {
            get
            {
                var accessor = Accessor as IScopedNameBasedEntityAccessor<TEntity, TSubEntity>;
                if (accessor == null)
                {
                    // TODO:: Message
                    throw new NotSupportedException();
                }

                return accessor;
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
            await TryEeagerLoadingEntity(entity);
            return entity;
        }

        #endregion

        #region Helpers

        protected override void NormalizeEntityName(TEntity entity)
        {
            if (!SupportsScopedNameBasedEntity && SupportsNameBasedEntity)
            {
                base.NormalizeEntityName(entity);
                return;
            }

            var normalizedName = NormalizeEntityKey(ScopedNameBasedEntityAccessor.GetName(entity));
            ScopedNameBasedEntityAccessor.SetNormalizedName(entity, normalizedName);
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
                return Store is IMasterDetailsEntityStore<TEntity, TSubEntity> && Accessor is IMasterDetailsEntityAccessor<TEntity, TSubEntity>;
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

        protected virtual IMasterDetailsEntityAccessor<TEntity, TSubEntity> MasterDetailsEntityAccessor
        {
            get
            {
                var accessor = Accessor as IMasterDetailsEntityAccessor<TEntity, TSubEntity>;
                if (accessor == null)
                {
                    // TODO:: Message
                    throw new NotSupportedException();
                }

                return accessor;
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
            IDisposable store, object entityAccessor,
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
            Accessor = entityAccessor;
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

        protected object Accessor { get; }

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
                if (CodeBasedEntityAccessor.GetCode(entity) == null && CodeGenerator != null)
                {
                    CodeBasedEntityAccessor.SetCode(entity, await GenerateEntityCodeAsync(entity));
                }
            }
        }

        protected virtual async Task PrepareEntityForCreatingAsync(TEntity entity)
        {
            if (SupportsCodeBasedEntity)
            {
                NormalizeEntityCode(entity);
            }

            if (SupportsNameBasedEntity)
            {
                NormalizeEntityName(entity);
            }

            if (SupportsTimeTrackedEntity)
            {
                var date = DateTime.Now;
                TimeTrackedEntityAccessor.SetDataCreateDate(entity, date);
                TimeTrackedEntityAccessor.SetDataLastModifyDate(entity, date);
            }

            await Task.FromResult(0);
        }

        protected virtual async Task PrepareEntityForUpdatingAsync(TEntity entity)
        {
            if (SupportsCodeBasedEntity)
            {
                NormalizeEntityCode(entity);
            }

            if (SupportsNameBasedEntity)
            {
                NormalizeEntityName(entity);
            }

            if (SupportsTimeTrackedEntity)
            {
                TimeTrackedEntityAccessor.SetDataLastModifyDate(entity, DateTime.Now);
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
                Logger.LogWarning(0, "Entity {entityId} validation failed: {errors}.", SupportsEntity ? EntityAccessor.GetId(entity) : null, string.Join(", ", errors.Select(e => e.Code)));
                return EntityResult.Failed(errors.ToArray());
            }
            return EntityResult.Success;
        }

        protected virtual async Task TryEeagerLoadingEntities(IEnumerable<TEntity> entities)
        {
            if (SupportsEagerLoadingEntity)
            {
                // Loading all entities parallelly will throw AggregateException exception.
                foreach (var entity in entities)
                {
                    await EagerLoadingEntityStore.EagerLoadAsync(entity, CancellationToken);
                }
            }
        }

        protected virtual async Task TryEeagerLoadingEntity(TEntity entity)
        {
            if (entity != null && SupportsEagerLoadingEntity)
            {
                await EagerLoadingEntityStore.EagerLoadAsync(entity, CancellationToken);
            }
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

        protected virtual bool SupportsEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IEntityStore<TEntity> && Accessor is IEntityAccessor<TEntity>;
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

        protected virtual IEntityAccessor<TEntity> EntityAccessor
        {
            get
            {
                var accessor = Accessor as IEntityAccessor<TEntity>;
                if (accessor == null)
                {
                    // TODO:: Message
                    throw new NotSupportedException();
                }

                return accessor;
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
            await TryEeagerLoadingEntity(entity);
            return entity;
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

            result.TotalCount = await EntityStore.CountAsync(models, CancellationToken);

            if (!string.IsNullOrWhiteSpace(search))
            {
                models = EntityStore.Filter(models, NormalizeEntityKey(search));
            }
            result.FilterredCount = await EntityStore.CountAsync(models, CancellationToken);

            models = EntityStore.PreSort(models);
            if (string.IsNullOrWhiteSpace(sort))
            {
                models = EntityStore.DefaultSort(models);
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
            await TryEeagerLoadingEntities(result.Results);
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

    // ActivatableEntity
    partial class EntityManagerBase<TEntity, TEntityManager>
    {
        #region Properties

        protected virtual bool SupportsActivatableEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IActivatableEntityStore<TEntity>;
            }
        }

        protected virtual IActivatableEntityStore<TEntity> ActivatableEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IActivatableEntityStore<TEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Resources.StoreNotIActivatableEntityStore);
                }

                return store;
            }
        }

        #endregion

        #region Public Operations

        protected virtual Task<EntityQueryResult<TEntity>> SearchEntitiesAsync(bool? isActive, string search, int? pageNumber, int? pageSize, string sort, bool reverse)
        {
            ThrowIfDisposed();
            return SearchEntitiesInternalAsync(ActivatableEntityStore.FilterByIsActive(Entities, isActive), search, pageNumber, pageSize, sort, reverse);
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
                return Store is ITimeTrackedEntityStore<TEntity> && Accessor is ITimeTrackedEntityAccessor<TEntity>;
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

        protected virtual ITimeTrackedEntityAccessor<TEntity> TimeTrackedEntityAccessor
        {
            get
            {
                var accessor = Accessor as ITimeTrackedEntityAccessor<TEntity>;
                if (accessor == null)
                {
                    // TODO:: Message
                    throw new NotSupportedException();
                }

                return accessor;
            }
        }

        #endregion

        #region Public Operations

        protected virtual async Task<TEntity> FindLatestEntityAsync()
        {
            ThrowIfDisposed();
            var entity = await TimeTrackedEntityStore.FindLatestAsync(CancellationToken);
            await TryEeagerLoadingEntity(entity);
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
                return Store is ICodeBasedEntityStore<TEntity> && Accessor is ICodeBasedEntityAccessor<TEntity>;
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

        protected virtual ICodeBasedEntityAccessor<TEntity> CodeBasedEntityAccessor
        {
            get
            {
                var accessor = Accessor as ICodeBasedEntityAccessor<TEntity>;
                if (accessor == null)
                {
                    // TODO:: Message
                    throw new NotSupportedException();
                }

                return accessor;
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

            var entity = await CodeBasedEntityStore.FindByCodeAsync(NormalizeEntityKey(code), CancellationToken);
            await TryEeagerLoadingEntity(entity);
            return entity;
        }

        #endregion

        #region Helpers

        protected virtual void NormalizeEntityCode(TEntity entity)
        {
            var normalizedCode = NormalizeEntityKey(CodeBasedEntityAccessor.GetCode(entity));
            CodeBasedEntityAccessor.SetCode(entity, normalizedCode);
        }

        protected virtual Task<string> GenerateEntityCodeAsync(TEntity entity)
        {
            if (CodeGenerator == null)
            {
                throw new InvalidOperationException($"{nameof(CodeGenerator)} is null.");
            }

            return CodeGenerator.GenerateCodeAsync((TEntityManager)this, entity, CancellationToken);
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
                return Store is INameBasedEntityStore<TEntity> && Accessor is INameBasedEntityAccessor<TEntity>;
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

        protected virtual INameBasedEntityAccessor<TEntity> NameBasedEntityAccessor
        {
            get
            {
                var accessor = Accessor as INameBasedEntityAccessor<TEntity>;
                if (accessor == null)
                {
                    // TODO:: Message
                    throw new NotSupportedException();
                }

                return accessor;
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
            await TryEeagerLoadingEntity(entity);
            return entity;
        }

        #endregion

        #region Helpers

        protected virtual void NormalizeEntityName(TEntity entity)
        {
            var normalizedName = NormalizeEntityKey(NameBasedEntityAccessor.GetName(entity));
            NameBasedEntityAccessor.SetNormalizedName(entity, normalizedName);
        }

        #endregion
    }
}
