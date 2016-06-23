using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace PhantomNet.Entities.Mvc
{
    public abstract class EntityApiControllerBase<TModel, TModelSearchDescriptor, TModelManager, TErrorDescriber>
        : ApiControllerBase<TModel, TModelManager, TErrorDescriber>
        where TModel : class
        where TModelSearchDescriptor : class, IEntitySearchDescriptor<TModel>, new()
        where TModelManager : IDisposable
        where TErrorDescriber : class
    {
        public EntityApiControllerBase(TModelManager manager, TErrorDescriber errorDescriber, IStringLocalizer localizer)
            : base(manager, errorDescriber)
        {
            Localizer = localizer;
        }

        protected IStringLocalizer Localizer { get; }

        #region Public Operations

        [HttpGet("resources")]
        public virtual dynamic Resources()
        {
            return new {
                CreateError = Localizer["CreateError"].ToString(),
                UpdateError = Localizer["UpdateError"].ToString(),
                DeleteConfirmation = Localizer["DeleteConfirmation"].ToString(),
                DeleteError = Localizer["DeleteError"].ToString()
            };
        }

        [HttpGet]
        public virtual Task<IEnumerable<TModel>> Get(string token, string search, int? pageNumber, int? pageSize, string sort, bool reverse)
        {
            var searchDescriptor = new TModelSearchDescriptor() {
                SearchText = search,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortExpression = sort,
                SortReverse = reverse
            };

            return GetModels(token, searchDescriptor, returnedModel => PreProcessReturnedModel(returnedModel));
        }

        [HttpGet("{id}")]
        public virtual Task<TModel> Get(string token, string id)
        {
            return GetModel(token, id, returnedModel => PreProcessReturnedModel(returnedModel));
        }

        [HttpPost]
        public virtual Task<dynamic> Post([FromBody]TModel model)
        {
            return PostModel(model, returnedModel => PreProcessReturnedModel(returnedModel));
        }

        [HttpPut("{id}")]
        public virtual Task<dynamic> Put(string id, [FromBody]TModel model)
        {
            return PutModel(id, model,
                describeModelNotFoundError: () => DescribeModelNotFoundError(model),
                updateModel: oldModel => UpdateModel(oldModel, model));
        }

        [HttpDelete("{id}")]
        public virtual Task<dynamic> Delete(string id)
        {
            return DeleteModel(id);
        }

        #endregion

        #region Helpers

        protected abstract GenericError DescribeModelNotFoundError(TModel model);

        protected abstract void UpdateModel(TModel oldModel, TModel newModel);

        protected virtual void PreProcessReturnedModel(TModel model) { }

        #endregion
    }
}
