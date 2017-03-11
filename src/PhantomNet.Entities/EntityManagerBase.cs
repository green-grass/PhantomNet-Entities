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
            IDisposable store,
            object entityAccessor,
            IEnumerable<IEntityValidator<TEntity, TSubEntity, TEntityManager>> entityValidators,
            ILogger<EntityManagerBase<TEntity, TSubEntity, TEntityManager>> logger)
            : base(store, entityAccessor, entityValidators, logger)
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

        protected override async Task PrepareEntityForValidation(TEntity entity)
        {
            await base.PrepareEntityForValidation(entity);

            if (SupportsScopedNameBasedEntity)
            {
                var scopeId = ScopedNameBasedEntityAccessor.GetGroupId(entity);
                var scope = await EntityWithSubEntityStore.FindByIdAsync<TSubEntity>(scopeId, CancellationToken);
                ScopedNameBasedEntityAccessor.SetGroup(entity, scope);
            }
        }

        protected override async Task PrepareEntityForCreating(TEntity entity)
        {
            await base.PrepareEntityForCreating(entity);

            if (SupportsScopedNameBasedEntity)
            {
                NormalizeEntityName(entity);
            }
        }

        protected override async Task PrepareEntityForUpdating(TEntity entity)
        {
            await base.PrepareEntityForUpdating(entity);

            if (SupportsScopedNameBasedEntity)
            {
                NormalizeEntityName(entity);
            }
        }

        protected override async Task<GenericResult> ValidateEntityInternal(TEntity entity)
        {
            var result = await base.ValidateEntityInternal(entity);

            if (SupportsMasterDetailsEntity)
            {
                var details = MasterDetailsEntityAccessor.GetDetails(entity);
                var tasks = new List<Task<GenericResult>>(details.Count);
                foreach (var detail in details)
                {
                    tasks.Add(ValidateSubEntityInternal(detail));
                }
                await Task.WhenAll(tasks);

                var errors = new List<GenericError>();
                foreach (var task in tasks)
                {
                    errors.AddRange(task.Result.Errors);
                }

                if (errors.Count > 0)
                {
                    result = GenericResult.Failed(result.Errors.Concat(errors).ToArray());
                }
            }

            return result;
        }

        protected virtual async Task<GenericResult> ValidateSubEntityInternal(TSubEntity subEntity)
        {
            var errors = new List<GenericError>();
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
                return GenericResult.Failed(errors.ToArray());
            }
            return GenericResult.Success;
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
                    throw new NotSupportedException(Strings.StoreNotIEntityWithSubEntityStore);
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
                    throw new NotSupportedException(Strings.StoreNotIQueryableEntityWithSubEntityStore);
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
                return Store is IGroupedEntityStore<TEntity, TSubEntity> && Accessor is IGroupedEntityAccessor<TEntity, TSubEntity>;
            }
        }

        protected virtual IGroupedEntityStore<TEntity, TSubEntity> GroupedStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IGroupedEntityStore<TEntity, TSubEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Strings.StoreNotIGroupedEntityStore);
                }

                return store;
            }
        }

        protected virtual IGroupedEntityAccessor<TEntity, TSubEntity> GroupedEntityAccessor
        {
            get
            {
                var accessor = Accessor as IGroupedEntityAccessor<TEntity, TSubEntity>;
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

        protected virtual Task<IEnumerable<TSubEntity>> GetAllEntityGroupsAsync()
        {
            ThrowIfDisposed();
            return GroupedStore.GetAllGroupsAsync(CancellationToken);
        }

        protected virtual Task<IEnumerable<TSubEntity>> GetEntityGroupsWithEntitiesAsync()
        {
            ThrowIfDisposed();
            return GroupedStore.GetGroupsWithEntitiesAsync(CancellationToken);
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
                    throw new NotSupportedException(Strings.StoreNotIScopedNameBasedEntityStore);
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

        protected virtual Task<TEntity> FindEntityByNameAsync(string name, TSubEntity scope)
        {
            ThrowIfDisposed();
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return ScopedNameBasedStore.FindByNameAsync(NormalizeEntityName(name), scope, CancellationToken);
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

            var normalizedName = NormalizeEntityName(ScopedNameBasedEntityAccessor.GetName(entity));
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
                return Store is IMasterDetailsEntityStore<TEntity, TSubEntity> &&
                    (Accessor is IMasterDetailsEntityAccessor<TEntity, TSubEntity> || HasDefaultMasterDetailsEntityAccessor);
            }
        }

        protected virtual bool HasDefaultMasterDetailsEntityAccessor
            => typeof(TEntity).IsAssignableFrom(typeof(IDetailsWiseEntity<TSubEntity>));

        protected virtual IMasterDetailsEntityStore<TEntity, TSubEntity> MasterDetailsStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IMasterDetailsEntityStore<TEntity, TSubEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Strings.StoreNotIMasterDetailsEntityStore);
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
                    if (HasDefaultMasterDetailsEntityAccessor)
                    {
                        return new DefaultMasterDetailsEntityAccessor<TEntity, TSubEntity>();
                    }

                    // TODO:: Message
                    throw new NotSupportedException();
                }

                return accessor;
            }
        }

        #endregion
    }

    // Foundation
    public abstract partial class EntityManagerBase<TEntity, TEntityManager> : IDisposable
        where TEntity : class
        where TEntityManager : EntityManagerBase<TEntity, TEntityManager>
    {
        #region Constructors

        protected EntityManagerBase(
            IDisposable store,
            object entityAccessor,
            IEnumerable<IEntityValidator<TEntity, TEntityManager>> entityValidators,
            ILogger<EntityManagerBase<TEntity, TEntityManager>> logger)
        {
            if (!(this is TEntityManager))
            {
                throw new NotSupportedException(Strings.FormatGenericClassNotMatched(nameof(TEntityManager)));
            }
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            Store = store;
            Accessor = entityAccessor;
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

        protected virtual CancellationToken CancellationToken => CancellationToken.None;

        protected ILogger Logger { get; }

        protected IDisposable Store { get; }

        protected object Accessor { get; }

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
                    throw new NotSupportedException(Strings.StoreNotIEagerLoadingEntityStore);
                }

                return store;
            }
        }

        protected virtual bool SupportsExplicitLoadingEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IExplicitLoadingEntityStore<TEntity>;
            }
        }

        protected virtual IExplicitLoadingEntityStore<TEntity> ExplicitLoadingEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IExplicitLoadingEntityStore<TEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Strings.StoreNotIExplicitLoadingEntityStore);
                }

                return store;
            }
        }

        protected virtual bool SupportsFilterableEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is IFilterableEntityStore<TEntity>;
            }
        }

        protected virtual IFilterableEntityStore<TEntity> FilterableEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as IFilterableEntityStore<TEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Strings.StoreNotIFilterableEntityStore);
                }

                return store;
            }
        }

        #endregion

        #region Helpers

        protected virtual async Task PrepareEntityForValidation(TEntity entity)
        {
            if (SupportsCodeBasedEntity)
            {
                if (CodeBasedEntityAccessor.GetCode(entity) == null && CodeGenerator != null)
                {
                    CodeBasedEntityAccessor.SetCode(entity, await GenerateEntityCode(entity));
                }
            }
        }

        protected virtual Task PrepareEntityForCreating(TEntity entity)
        {
            PrepareEntityForSaving(entity);

            if (SupportsTimeTrackedEntity)
            {
                var date = DateTime.Now;
                TimeTrackedEntityAccessor.SetDataCreateDate(entity, date);
                TimeTrackedEntityAccessor.SetDataLastModifyDate(entity, date);
            }

            return Task.FromResult(0);
        }

        protected virtual Task PrepareEntityForUpdating(TEntity entity)
        {
            PrepareEntityForSaving(entity);

            if (SupportsTimeTrackedEntity)
            {
                TimeTrackedEntityAccessor.SetDataLastModifyDate(entity, DateTime.Now);
            }

            return Task.FromResult(0);
        }

        protected virtual Task PrepareEntityForSaving(TEntity entity)
        {
            if (SupportsCodeBasedEntity)
            {
                NormalizeEntityCode(entity);
            }

            if (SupportsNameBasedEntity)
            {
                NormalizeEntityName(entity);
            }

            if (SupportsTaggedEntity)
            {
                NormalizeEntityTags(entity);
            }

            return Task.FromResult(0);
        }

        protected virtual async Task<GenericResult> ValidateEntityInternal(TEntity entity)
        {
            var errors = new List<GenericError>();
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
                return GenericResult.Failed(errors.ToArray());
            }
            return GenericResult.Success;
        }

        protected virtual Task<EntityQueryResult<TEntity>> SearchEntitiesInternal(IEntitySearchDescriptor<TEntity> searchDescriptor)
        {
            ThrowIfDisposed();
            if (SupportsQueryableEntity)
            {
                return SearchEntitiesInternal(QueryableEntityStore.Entities, searchDescriptor);
            }

            throw new NotImplementedException();
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
                    throw new NotSupportedException(Strings.StoreNotIEntityStore);
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

        protected virtual async Task<GenericResult> CreateEntityAsync(TEntity entity)
        {
            ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await PrepareEntityForValidation(entity);

            var result = await ValidateEntityInternal(entity);
            if (!result.Succeeded)
            {
                return result;
            }

            await PrepareEntityForCreating(entity);

            result = await EntityStore.CreateAsync(entity, CancellationToken);
            return result;
        }

        protected virtual Task<GenericResult> UpdateEntityAsync(TEntity entity)
        {
            ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return UpdateEntityInternal(entity);
        }

        protected virtual Task<GenericResult> DeleteEntityAsync(TEntity entity)
        {
            ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return EntityStore.DeleteAsync(entity, CancellationToken);
        }

        protected virtual Task<TEntity> FindEntityByIdAsync(string id)
        {
            ThrowIfDisposed();
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return EntityStore.FindByIdAsync<TEntity>(id, CancellationToken);
        }

        protected virtual Task<EntityQueryResult<TEntity>> SearchEntitiesAsync(IEntitySearchDescriptor<TEntity> searchDescriptor)
        {
            return SearchEntitiesInternal(searchDescriptor);
        }

        #endregion

        #region Helpers

        protected virtual async Task<GenericResult> UpdateEntityInternal(TEntity entity)
        {
            await PrepareEntityForValidation(entity);

            var result = await ValidateEntityInternal(entity);
            if (!result.Succeeded)
            {
                return result;
            }

            await PrepareEntityForUpdating(entity);

            return await EntityStore.UpdateAsync(entity, CancellationToken);
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
                    throw new NotSupportedException(Strings.StoreNotIQueryableEntityStore);
                }

                return store;
            }
        }

        #endregion

        #region Helpers

        protected virtual async Task<EntityQueryResult<TEntity>> SearchEntitiesInternal(IQueryable<TEntity> entities, IEntitySearchDescriptor<TEntity> searchDescriptor)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            if (SupportsEagerLoadingEntity)
            {
                entities = EagerLoadingEntityStore.EagerLoad(entities);
            }

            var result = new EntityQueryResult<TEntity>();

            entities = PreFilter(entities, searchDescriptor);
            result.TotalCount = await Count(entities);

            entities = Filter(entities, searchDescriptor);
            result.FilteredCount = await Count(entities);

            entities = Sort(entities, searchDescriptor);

            result.Results = Page(entities, searchDescriptor);

            if (SupportsExplicitLoadingEntity)
            {
                result.Results = result.Results.ToList().AsQueryable();
                await ExplicitLoadingEntityStore.ExplicitLoadAsync(result.Results, CancellationToken);
            }

            return result;
        }

        protected virtual IQueryable<TEntity> PreFilter(IQueryable<TEntity> entities, IEntitySearchDescriptor<TEntity> searchDescriptor)
        {
            entities = searchDescriptor?.PreFilter(entities) ?? entities;
            if (SupportsFilterableEntity)
            {
                entities = FilterableEntityStore.PreFilter(entities, searchDescriptor);
            }

            return entities;
        }

        protected virtual IQueryable<TEntity> Filter(IQueryable<TEntity> entities, IEntitySearchDescriptor<TEntity> searchDescriptor)
        {
            entities = searchDescriptor?.Filter(entities) ?? entities;
            if (SupportsFilterableEntity)
            {
                entities = FilterableEntityStore.Filter(entities, searchDescriptor);
            }

            return entities;
        }

        protected virtual async Task<int> Count(IQueryable<TEntity> entities)
        {
            if (SupportsEntity)
            {
                return await EntityStore.CountAsync(entities, CancellationToken);
            }

            return entities.Count();
        }

        protected virtual IQueryable<TEntity> Sort(IQueryable<TEntity> entities, IEntitySearchDescriptor<TEntity> searchDescriptor)
        {
            // Pre-sort
            entities = searchDescriptor?.PreSort(entities) ?? entities;

            // Sort
            var sort = searchDescriptor?.SortExpression;
            var reverse = searchDescriptor?.SortReverse;
            if (string.IsNullOrWhiteSpace(sort))
            {
                entities = searchDescriptor?.DefaultSort(entities) ?? entities;
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
                entities = (IOrderedQueryable<TEntity>)entities.Provider.CreateQuery(Expression.Call(
                    typeof(Queryable),
                    entities.Expression.Type == typeof(IOrderedQueryable<TEntity>) ?
                        (reverse.Value ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy)) :
                        (reverse.Value ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy)),
                    new Type[] { typeof(TEntity), propertyInfo.PropertyType },
                    entities.Expression,
                    propertyExpression
                ));
            }

            return entities;
        }

        protected virtual IQueryable<TEntity> Page(IQueryable<TEntity> entities, IEntitySearchDescriptor<TEntity> searchDescriptor)
        {
            var offset = ((searchDescriptor?.PageNumber - 1) * searchDescriptor?.PageSize) ?? 0;
            var limit = searchDescriptor?.PageSize ?? int.MaxValue;
            return entities.Skip(offset).Take(limit);
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
                return Store is ITimeTrackedEntityStore<TEntity> &&
                    (Accessor is ITimeTrackedEntityAccessor<TEntity> || HasDefaultTimeTrackedEntityAccessor);
            }
        }

        protected virtual bool HasDefaultTimeTrackedEntityAccessor
            => typeof(TEntity).IsAssignableFrom(typeof(ITimeWiseEntity));

        protected virtual ITimeTrackedEntityStore<TEntity> TimeTrackedEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as ITimeTrackedEntityStore<TEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Strings.StoreNotITimeTrackedEntityStore);
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
                    if (HasDefaultTimeTrackedEntityAccessor)
                    {
                        return new DefaultTimeTrackedEntityAccessor<TEntity>();
                    }

                    // TODO:: Message
                    throw new NotSupportedException();
                }

                return accessor;
            }
        }

        #endregion

        #region Public Operations

        protected virtual Task<TEntity> FindLatestEntityAsync()
        {
            ThrowIfDisposed();
            return TimeTrackedEntityStore.FindLatestAsync(CancellationToken);
        }

        #endregion
    }

    // CodeBasedEntity
    partial class EntityManagerBase<TEntity, TEntityManager>
    {
        #region Properties

        protected ILookupNormalizer CodeNormalizer { get; set; }

        protected IEntityCodeGenerator<TEntity, TEntityManager> CodeGenerator { get; set; }

        protected virtual bool SupportsCodeBasedEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is ICodeBasedEntityStore<TEntity> &&
                    (Accessor is ICodeBasedEntityAccessor<TEntity> || HasDefaultCodeBasedEntityAccessor);
            }
        }

        protected virtual bool HasDefaultCodeBasedEntityAccessor
            => typeof(TEntity).IsAssignableFrom(typeof(ICodeWiseEntity));

        protected virtual ICodeBasedEntityStore<TEntity> CodeBasedEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as ICodeBasedEntityStore<TEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Strings.StoreNotICodeBasedEntityStore);
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
                    if (HasDefaultCodeBasedEntityAccessor)
                    {
                        return new DefaultCodeBasedEntityAccessor<TEntity>();
                    }

                    // TODO:: Message
                    throw new NotSupportedException();
                }

                return accessor;
            }
        }

        #endregion

        #region Public Operations

        protected virtual Task<TEntity> FindEntityByCodeAsync(string code)
        {
            ThrowIfDisposed();
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            return CodeBasedEntityStore.FindByCodeAsync(NormalizeEntityCode(code), CancellationToken);
        }

        #endregion

        #region Helpers

        protected virtual string NormalizeEntityCode(string code)
        {
            return (CodeNormalizer == null) ? code : CodeNormalizer.Normalize(code);
        }

        protected virtual void NormalizeEntityCode(TEntity entity)
        {
            var normalizedCode = NormalizeEntityCode(CodeBasedEntityAccessor.GetCode(entity));
            CodeBasedEntityAccessor.SetCode(entity, normalizedCode);
        }

        protected virtual Task<string> GenerateEntityCode(TEntity entity)
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

        protected ILookupNormalizer NameNormalizer { get; set; }

        protected virtual bool SupportsNameBasedEntity
        {
            get
            {
                ThrowIfDisposed();
                return Store is INameBasedEntityStore<TEntity> &&
                    (Accessor is INameBasedEntityAccessor<TEntity> || HasDefaultNameBasedEntityAccessor);
            }
        }

        protected virtual bool HasDefaultNameBasedEntityAccessor
            => typeof(TEntity).IsAssignableFrom(typeof(INameWiseEntity));

        protected virtual INameBasedEntityStore<TEntity> NameBasedEntityStore
        {
            get
            {
                ThrowIfDisposed();
                var store = Store as INameBasedEntityStore<TEntity>;
                if (store == null)
                {
                    throw new NotSupportedException(Strings.StoreNotINameBasedEntityStore);
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
                    if (HasDefaultNameBasedEntityAccessor)
                    {
                        return new DefaultNameBasedEntityAccessor<TEntity>();
                    }

                    // TODO:: Message
                    throw new NotSupportedException();
                }

                return accessor;
            }
        }

        #endregion

        #region Public Operations

        protected virtual Task<TEntity> FindEntityByNameAsync(string name)
        {
            ThrowIfDisposed();
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return NameBasedEntityStore.FindByNameAsync(NormalizeEntityName(name), CancellationToken);
        }

        #endregion

        #region Helpers

        protected virtual string NormalizeEntityName(string name)
        {
            return (NameNormalizer == null) ? name : NameNormalizer.Normalize(name);
        }

        protected virtual void NormalizeEntityName(TEntity entity)
        {
            var normalizedName = NormalizeEntityName(NameBasedEntityAccessor.GetName(entity));
            NameBasedEntityAccessor.SetNormalizedName(entity, normalizedName);
        }

        #endregion
    }

    // TaggedEntity
    partial class EntityManagerBase<TEntity, TEntityManager>
    {
        #region Properties

        protected virtual ITagProcessor TagProcessor { get; set; }

        protected virtual bool SupportsTaggedEntity
        {
            get
            {
                ThrowIfDisposed();
                return Accessor is ITaggedEntityAccessor<TEntity> || HasDefaultTaggedEntityAccessor;
            }
        }

        protected virtual bool HasDefaultTaggedEntityAccessor
            => typeof(TEntity).IsAssignableFrom(typeof(ITagsWiseEntity));

        protected virtual ITaggedEntityAccessor<TEntity> TaggedEntityAccessor
        {
            get
            {
                var accessor = Accessor as ITaggedEntityAccessor<TEntity>;
                if (accessor == null)
                {
                    if (HasDefaultTaggedEntityAccessor)
                    {
                        return new DefaultTaggedEntityAccessor<TEntity>();
                    }

                    // TODO:: Message
                    throw new NotSupportedException();
                }

                return accessor;
            }
        }

        #endregion

        #region Helpers

        protected virtual void NormalizeEntityTags(TEntity entity)
        {
            var tags = TaggedEntityAccessor.GetTags(entity);
            var normalizedTags = (TagProcessor == null) ? tags : TagProcessor.NormalizeTags(tags);
            TaggedEntityAccessor.SetTags(entity, normalizedTags);
        }

        #endregion
    }
}
