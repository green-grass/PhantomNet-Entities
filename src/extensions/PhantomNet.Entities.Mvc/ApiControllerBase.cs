using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PhantomNet.Entities.Mvc.Properties;

namespace PhantomNet.Entities.Mvc
{
    // Foundation
    public abstract partial class ApiControllerBase<TModel, TViewModel, TModelManager, TErrorDescriber> : Controller
        where TModel : class
        where TViewModel : class
        where TModelManager : IDisposable
        where TErrorDescriber : class
    {
        public ApiControllerBase(TModelManager manager, TErrorDescriber errorDescriber)
        {
            Manager = manager;
            ErrorDescriber = errorDescriber;
        }

        #region Properties

        protected TModelManager Manager { get; }

        protected TErrorDescriber ErrorDescriber { get; }

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
    partial class ApiControllerBase<TModel, TViewModel, TModelManager, TErrorDescriber>
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
                    throw new NotSupportedException(Strings.ManagerNotIEntityManager);
                }

                return manager;
            }
        }

        #endregion

        #region Public Operations

        protected virtual Task<IEnumerable<TViewModel>> GetModels(string token, IEntitySearchDescriptor<TModel> searchDescriptor)
        {
            return GetModels(token, searchDescriptor, null);
        }

        protected virtual async Task<IEnumerable<TViewModel>> GetModels(string token, IEntitySearchDescriptor<TModel> searchDescriptor,
            Action<TViewModel> preProcessReturnedViewModel)
        {
            if (!string.IsNullOrWhiteSpace(searchDescriptor.SortExpression))
            {
                searchDescriptor.SortExpression = searchDescriptor.SortExpression.ToPascalCase();
            }

            var result = await EntityManager.SearchAsync(searchDescriptor);

            Response.Headers["total-count"] = result.TotalCount.ToString();
            Response.Headers["filtered-count"] = result.FilteredCount.ToString();
            Response.Headers["token"] = token;

            var viewModels = Mapper.Map<IEnumerable<TViewModel>>(result.Results);
            if (preProcessReturnedViewModel != null)
            {
                foreach (var viewModel in viewModels)
                {
                    preProcessReturnedViewModel(viewModel);
                }
            }
            return viewModels;
        }

        protected virtual Task<TViewModel> GetModel(string token, string id)
        {
            return GetModel(token, id, null);
        }

        protected virtual async Task<TViewModel> GetModel(string token, string id,
            Action<TViewModel> preProcessReturnedViewModel)
        {
            ThrowIfDisposed();
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var model = await EntityManager.FindByIdAsync(id);

            Response.Headers["token"] = token;

            var viewModel = Mapper.Map<TViewModel>(model);
            preProcessReturnedViewModel?.Invoke(viewModel);
            return viewModel;
        }

        protected virtual Task<dynamic> PostModel(TViewModel viewModel)
        {
            return PostModel(viewModel, null);
        }

        protected virtual async Task<dynamic> PostModel(TViewModel viewModel,
            Action<TViewModel> preProcessReturnedViewModel)
        {
            try
            {
                var model = Mapper.Map<TModel>(viewModel);
                var result = await EntityManager.CreateAsync(model);
                if (result.Succeeded)
                {
                    viewModel = Mapper.Map<TViewModel>(model);
                    preProcessReturnedViewModel?.Invoke(viewModel);
                    return new {
                        Result = result,
                        Model = viewModel
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
                var errors = new List<GenericError>();
                do
                {
                    errors.Add(new GenericError { Code = e.Source, Description = e.Message });
                    e = e.InnerException;
                } while (e != null);

                return new {
                    Result = GenericResult.Failed(errors.ToArray())
                };
            }
        }

        protected virtual async Task<dynamic> PutModel(string id, [FromBody]TViewModel viewModel,
            Func<TViewModel, GenericError> describeModelNotFoundError,
            Action<TModel, TViewModel> updateModel)
        {
            try
            {
                var oldModel = await EntityManager.FindByIdAsync(id);
                if (oldModel == null)
                {
                    return new {
                        Result = GenericResult.Failed(describeModelNotFoundError(viewModel))
                    };
                }

                updateModel.Invoke(oldModel, viewModel);

                var result = await EntityManager.UpdateAsync(oldModel);
                return new {
                    Result = result
                };
            }
            catch (Exception e)
            {
                var errors = new List<GenericError>();
                do
                {
                    errors.Add(new GenericError { Code = e.Source, Description = e.Message });
                    e = e.InnerException;
                } while (e != null);

                return new {
                    Result = GenericResult.Failed(errors.ToArray())
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
                var errors = new List<GenericError>();
                do
                {
                    errors.Add(new GenericError { Code = e.Source, Description = e.Message });
                    e = e.InnerException;
                } while (e != null);

                return new {
                    Result = GenericResult.Failed(errors.ToArray())
                };
            }
        }

        #endregion
    }
}
