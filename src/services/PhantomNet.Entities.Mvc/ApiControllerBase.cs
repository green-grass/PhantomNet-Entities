using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PhantomNet.Entities.Mvc
{
    // Foundation
    public abstract partial class ApiControllerBase<TModel, TSubModel, TModelManager, TErrorDescriber>
        : ApiControllerBase<TModel, TModelManager, TErrorDescriber>
        where TModel : class
        where TSubModel : class
        where TModelManager : IDisposable
        where TErrorDescriber : class
    {
        public ApiControllerBase(TModelManager manager, TErrorDescriber errorDescriber)
            : base(manager, errorDescriber)
        { }
    }

    // GroupedEntity
    partial class ApiControllerBase<TModel, TSubModel, TModelManager, TErrorDescriber>
    {
        #region Properties

        protected virtual bool SupportsGroupedEntity
        {
            get
            {
                ThrowIfDisposed();
                return Manager is IGroupedEntityManager<TModel, TSubModel>;
            }
        }

        protected virtual IGroupedEntityManager<TModel, TSubModel> GroupedEntityManager
        {
            get
            {
                ThrowIfDisposed();
                var manager = Manager as IGroupedEntityManager<TModel, TSubModel>;
                if (manager == null)
                {
                    throw new NotSupportedException(Resources.ManagerNotIGroupedEntityManager);
                }

                return manager;
            }
        }

        #endregion

        #region Public Operations

        protected virtual Task<IEnumerable<TModel>> GetModels(string token, TSubModel group, string search, int? pageNumber, int? pageSize, string sort, bool reverse)
        {
            return GetModels(token, group, search, pageNumber, pageSize, sort, reverse, null);
        }

        protected virtual async Task<IEnumerable<TModel>> GetModels(string token, TSubModel group, string search, int? pageNumber, int? pageSize, string sort, bool reverse,
            Action<TModel> preProcessReturnedModel)
        {
            var result = await GroupedEntityManager.SearchAsync(group, search, pageNumber, pageSize, sort, reverse);
            Response.Headers["total-count"] = result.TotalCount.ToString();
            Response.Headers["filtered-count"] = result.FilterredCount.ToString();
            Response.Headers["token"] = token;
            if (preProcessReturnedModel != null)
            {
                foreach (var model in result.Results)
                {
                    preProcessReturnedModel(model);
                }
            }
            return result.Results;
        }

        #endregion
    }

    // Foundation
    public abstract partial class ApiControllerBase<TModel, TModelManager, TErrorDescriber> : Controller
        where TModel : class
        where TModelManager : IDisposable
        where TErrorDescriber : class
    {
        public ApiControllerBase(TModelManager manager, TErrorDescriber errorDescriber)
        {
            Manager = manager;
            ErrorDescriber = errorDescriber;
        }

        #region Properties

        protected virtual TModelManager Manager { get; }

        protected virtual TErrorDescriber ErrorDescriber { get; }

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

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Manager.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion
    }

    // Entity
    partial class ApiControllerBase<TModel, TModelManager, TErrorDescriber>
    {
        #region Properties

        protected virtual bool SupportsEntity
        {
            get
            {
                ThrowIfDisposed();
                return Manager is IEntityManager<TModel>;
            }
        }

        protected virtual IEntityManager<TModel> EntityManager
        {
            get
            {
                ThrowIfDisposed();
                var manager = Manager as IEntityManager<TModel>;
                if (manager == null)
                {
                    throw new NotSupportedException(Resources.ManagerNotIEntityManager);
                }

                return manager;
            }
        }

        #endregion

        #region Public Operations

        protected virtual Task<IEnumerable<TModel>> GetModels(string token, string search, int? pageNumber, int? pageSize, string sort, bool reverse)
        {
            return GetModels(token, search, pageNumber, pageSize, sort, reverse, null);
        }

        protected virtual async Task<IEnumerable<TModel>> GetModels(string token, string search, int? pageNumber, int? pageSize, string sort, bool reverse,
            Action<TModel> preProcessReturnedModel)
        {
            var result = await EntityManager.SearchAsync(search, pageNumber, pageSize, sort, reverse);
            Response.Headers["total-count"] = result.TotalCount.ToString();
            Response.Headers["filtered-count"] = result.FilterredCount.ToString();
            Response.Headers["token"] = token;
            if (preProcessReturnedModel != null)
            {
                foreach (var model in result.Results)
                {
                    preProcessReturnedModel(model);
                }
            }
            return result.Results;
        }

        protected virtual Task<TModel> GetModel(string id)
        {
            return GetModel(id, null);
        }

        protected virtual async Task<TModel> GetModel(string id, Action<TModel> preProcessReturnedModel)
        {
            ThrowIfDisposed();
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = await EntityManager.FindByIdAsync(id);
            if (preProcessReturnedModel != null)
            {
                preProcessReturnedModel(result);
            }
            return result;
        }

        protected virtual Task<dynamic> PostModel(TModel model)
        {
            return PostModel(model, null);
        }

        protected virtual async Task<dynamic> PostModel(TModel model, Action<TModel> preProcessReturnedModel)
        {
            try
            {
                var result = await EntityManager.CreateAsync(model);
                if (result.Succeeded)
                {
                    if (preProcessReturnedModel != null)
                    {
                        preProcessReturnedModel(model);
                    }
                    return new {
                        Result = result,
                        Model = model
                    };
                }
                else
                {
                    return new {
                        Result = result,
                        Model = default(TModel)
                    };
                }
            }
            catch (Exception e)
            {
                return new {
                    Result = GenericResult.Failed(new GenericError { Description = e.Message })
                };
            }
        }

        protected virtual async Task<dynamic> PutModel(string id, [FromBody]TModel model,
            Func<GenericError> describeModelNotFoundError,
            Action<TModel> updateModel)
        {
            try
            {
                var oldModel = await EntityManager.FindByIdAsync(id);
                if (model == null)
                {
                    return new {
                        Result = GenericResult.Failed(describeModelNotFoundError())
                    };
                }

                updateModel(oldModel);

                var result = await EntityManager.UpdateAsync(oldModel);
                return new {
                    Result = result
                };
            }
            catch (Exception e)
            {
                return new {
                    Result = GenericResult.Failed(new GenericError { Description = e.Message })
                };
            }
        }

        protected virtual async Task<dynamic> DeleteModel(string id)
        {
            try
            {
                var model = await EntityManager.FindByIdAsync(id);
                if (model == null)
                {
                    return new {
                        Result = GenericResult.Success
                    };
                }

                var result = await EntityManager.DeleteAsync(model);
                return new {
                    Result = result
                };
            }
            catch (Exception e)
            {
                return new {
                    Result = GenericResult.Failed(new GenericError { Description = e.Message })
                };
            }
        }

        #endregion
    }
}
