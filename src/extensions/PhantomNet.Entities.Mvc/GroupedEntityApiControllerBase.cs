using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace PhantomNet.Entities.Mvc
{
    public abstract class GroupedEntityApiControllerBase<TModel, TModelGroup, TViewModel, TViewModelGroup, TKey, TModelSearchDescriptor, TModelManager, TErrorDescriber>
        : ApiControllerBase<TModel, TViewModel, TModelManager, TErrorDescriber>
        where TModel : class
        where TModelGroup : class
        where TViewModel : class
        where TViewModelGroup : class
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
        public virtual Task<IEnumerable<TViewModel>> Get(string token, TKey groupId, string search, int? pageNumber, int? pageSize, string sort, bool reverse)
        {
            var searchDescriptor = new TModelSearchDescriptor() {
                GroupId = groupId,
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

        protected abstract dynamic ProjectGroup(TModelGroup group);

        #endregion
    }
}
