using AutoMapper;
using MiCake.AspNetCore.Controller;
using MiCake.Cord.Paging;
using MiCake.Core.DependencyInjection;
using MiCake.Identity;

namespace MiCakeTemplate.Api.Controllers
{
    public abstract class AppControllerBase<T> : MiCakeControllerBase where T : MiCakeControllerBase
    {
        protected ILogger Logger { get; set; }

        public IMapper Mapper => infrastructuresServices.Mapper;

        public int? CurrentUserId => infrastructuresServices.CurrentUser?.UserId as int?;

        protected ControllerInfrastructures infrastructuresServices;

        public AppControllerBase(ControllerInfrastructures infrastructures, ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger<T>();

            infrastructuresServices = infrastructures;
        }

        protected PagingQueryResult<TD> MapperPagingQueryResult<TS, TD>(PagingQueryResult<TS> source)
        {
            return new PagingQueryResult<TD>(source.CurrentPageNumber, source.TotalCount, Mapper.Map<List<TD>>(source.Data));
        }
    }

    // Use this composite class to reduce the number of services in the controller's constructor.
    [InjectService()]
    public class ControllerInfrastructures
    {
        public IMapper Mapper { get; set; }

        public ICurrentMiCakeUser CurrentUser { get; }

        public ControllerInfrastructures(IMapper mapper, ICurrentMiCakeUser currentUser)
        {
            Mapper = mapper;
            CurrentUser = currentUser;
        }
    }
}
