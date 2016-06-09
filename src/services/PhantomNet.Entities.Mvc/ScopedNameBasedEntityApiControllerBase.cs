using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PhantomNet.Entities.Mvc
{
    public abstract class ScopedNameBasedEntityApiControllerBase<TModel, TModelScope, TKey, TModelSearchDescriptor, TModelManager, TErrorDescriber>
        : ApiControllerBase<TModel, TModelManager, TErrorDescriber>
        where TModel : class
        where TModelScope : class
        where TKey : IEquatable<TKey>
        where TModelSearchDescriptor : class, IEntitySearchDescriptor<TModel>, IScopedNameBasedEntitySearchDescriptor<TKey>, new()
        where TModelManager : IScopedNameBasedEntityManager<TModel, TModelScope>
        where TErrorDescriber : class
    {
        public ScopedNameBasedEntityApiControllerBase(TModelManager manager, TErrorDescriber errorDescriber)
            : base(manager, errorDescriber)
        { }

        #region Public Operations

        [HttpGet("scopes")]
        public async Task<IEnumerable<dynamic>> Makes()
        {
            return (await Manager.GetAllScopesAsync()).Select(x => ProjectScope(x));
        }

        [HttpGet("scopes-with-entities")]
        public async Task<IEnumerable<dynamic>> MakesWithModels()
        {
            return (await Manager.GetScopesWithEntitiesAsync()).Select(x => ProjectScope(x));
        }

        [HttpGet]
        public virtual Task<IEnumerable<TModel>> Get(string token, TKey groupId, string search, int? pageNumber, int? pageSize, string sort, bool reverse)
        {
            var searchDescriptor = new TModelSearchDescriptor() {
                ScopeId = groupId,
                SearchText = search,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortExpression = sort,
                SortReverse = reverse
            };

            return GetModels(token, searchDescriptor, returnedModel => PreProcessReturnedModel(returnedModel));
        }

        [HttpGet("{id}")]
        public virtual Task<TModel> Get(string id)
        {
            return GetModel(id, returnedModel => PreProcessReturnedModel(returnedModel));
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

        protected abstract dynamic ProjectScope(TModelScope scope);

        #endregion
    }
}
