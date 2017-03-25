using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PhantomNet.Entities.Mvc
{
    public abstract class ReadOnlyEntityApiControllerBase<TModel, TViewModel, TModelSearchDescriptor, TModelManager>
        : ApiControllerBase<TModel, TViewModel, TModelManager>
        where TModel : class
        where TViewModel : class
        where TModelSearchDescriptor : class, IEntitySearchDescriptor<TModel>, new()
        where TModelManager : IDisposable
    {
        public ReadOnlyEntityApiControllerBase(TModelManager manager)
            : base(manager)
        { }

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

            return GetModels(token, searchDescriptor);
        }
    }
}
