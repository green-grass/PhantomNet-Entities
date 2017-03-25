using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace PhantomNet.Entities.Mvc
{
    public abstract class EntityApiControllerBase<TModel, TViewModel, TModelSearchDescriptor, TModelManager>
        : ApiControllerBase<TModel, TViewModel, TModelManager>
        where TModel : class
        where TViewModel : class
        where TModelSearchDescriptor : class, IEntitySearchDescriptor<TModel>, new()
        where TModelManager : IDisposable
    {
        public EntityApiControllerBase(TModelManager manager, IStringLocalizer localizer)
            : base(manager)
        {
            Localizer = localizer;
        }

        protected IStringLocalizer Localizer { get; }

        #region Public Operations

        [HttpGet("resources")]
        public virtual dynamic Resources()
        {
            return new {
                CreateError = Localizer["CreateError"].Value,
                UpdateError = Localizer["UpdateError"].Value,
                DeleteConfirmation = Localizer["DeleteConfirmation"].Value,
                DeleteError = Localizer["DeleteError"].Value
            };
        }

        [HttpGet]
        public virtual Task<IEnumerable<TViewModel>> Get(string token, string search, int? pageNumber, int? pageSize, string sort, bool reverse)
        {
            var searchDescriptor = new TModelSearchDescriptor() {
                SearchText = search,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortExpression = sort,
                SortReverse = reverse
            };

            return GetModels(token, searchDescriptor, returnedViewModel => PreProcessReturnedViewModel(returnedViewModel));
        }

        [HttpGet("{id}")]
        public virtual Task<TViewModel> Get(string token, string id)
        {
            return GetModel(token, id, returnedViewModel => PreProcessReturnedViewModel(returnedViewModel));
        }

        [HttpPost]
        public virtual Task<dynamic> Post([FromBody]TViewModel viewModel)
        {
            return PostModel(viewModel, returnedViewModel => PreProcessReturnedViewModel(returnedViewModel));
        }

        [HttpPut("{id}")]
        public virtual Task<dynamic> Put(string id, [FromBody]TViewModel viewModel)
        {
            return PutModel(id, viewModel,
                describeModelNotFoundError: DescribeModelNotFoundError,
                updateModel: UpdateModel);
        }

        [HttpDelete("{id}")]
        public virtual Task<dynamic> Delete(string id)
        {
            return DeleteModel(id);
        }

        #endregion

        #region Helpers

        protected abstract GenericError DescribeModelNotFoundError(TViewModel viewModel);

        protected virtual void PreProcessReturnedViewModel(TViewModel viewModel) { }

        protected virtual void UpdateModel(TModel oldModel, TViewModel viewModel)
        {
            Mapper.Map(viewModel, oldModel);
        }

        #endregion
    }
}
