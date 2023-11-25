using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using StarBlog.Contrib.CLRStats;
//https://github.com/itsvse/CLRStats
using StarBlog.Web.Extensions;

namespace StarBlog.Web.Apis.Common;

[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Common)]
public class DashboardController : ControllerBase
{
    //[HttpGet("[action]")]
    //public ApiResponse ClrStats()
    //    => ApiResponse.Ok(CLRStatsUtils.GetCurrentClrStats());

}