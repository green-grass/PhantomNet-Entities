using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace PhantomNet.Entities.Mvc
{
    public abstract class GroupedEntityApiControllerBase<TModel, TModelGroup, TKey, TModelSearchDescriptor, TModelManager, TErrorDescriber>
        : ApiControllerBase<TModel, TModelManager, TErrorDescriber>
        where TModel : class
        where TModelGroup : class
        where TKey : IEquatable<TKey>
        where TModelSearchDescriptor : class, IEntitySearchDescriptor<TModel>, IGroupedEntitySearchDescriptor<TKey>, new()
        where TModelManager : IGroupedEntityManager<TModel, TModelGroup>
        where TErrorDescriber : class
    {
        public GroupedEntityApiControllerBase(TModelManager manager, TErrorDescriber errorDescriber, IStringLocalizer localizer)
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

        [HttpGet("groups")]
        public async Task<IEnumerable<dynamic>> Groups()
        {
            return (await Manager.GetAllGroupsAsync()).Select(x => ProjectGroup(x));
        }

        [HttpGet("groups-with-entities")]
        public async Task<IEnumerable<dynamic>> GroupsWithModels()
        {
            return (await Manager.GetGroupsWithEntitiesAsync()).Select(x => ProjectGroup(x));
        }

        [HttpGet]
        public virtual Task<IEnumerable<TModel>> Get(string token, TKey groupId, string search, int? pageNumber, int? pageSize, string sort, bool reverse)
        {
            var searchDescriptor = new TModelSearchDescriptor() {
                GroupId = groupId,
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

        protected abstract dynamic ProjectGroup(TModelGroup group);

        #endregion
    }
}
