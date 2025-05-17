using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ASC.WEB.Data; // namespace chứa interface và class cache ops

namespace ASC.WEB.Controllers
{
    [Route("test")]
    public class TestController : Controller
    {
        private readonly IMasterDataCacheOperations _cacheOperations;

        public TestController(IMasterDataCacheOperations cacheOperations)
        {
            _cacheOperations = cacheOperations;
        }

        [HttpGet("create-cache")]
        public async Task<IActionResult> CreateCache()
        {
            try
            {
                await _cacheOperations.CreateMasterDataCacheAsync();
                return Content("MasterDataCache đã được tạo trong Redis thành công.");
            }
            catch (System.Exception ex)
            {
                // Trả lỗi nếu có
                return Content($"Lỗi khi tạo cache: {ex.Message}");
            }
        }
    }
}