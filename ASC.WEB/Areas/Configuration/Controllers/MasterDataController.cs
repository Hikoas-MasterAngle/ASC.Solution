using ASC.Business.Interfaces;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.WEB.Areas.Configuration.Models;
using ASC.WEB.Controllers;
using ASC.WED.Areas.Configuration.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;


namespace ASC.Web.Areas.Configuration.Controllers
{
    [Area("Configuration")]
    [Authorize(Roles = "Admin")]
    // 1 reference
    public class MasterDataController : BaseController
    {
        private readonly IMasterDataOperations _masterData;
        private readonly IMapper _mapper;

        // 0 references
        public MasterDataController(IMasterDataOperations masterData, IMapper mapper)
        {
            _masterData = masterData;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> MasterKeys()
        {
            var masterKeys = await _masterData.GetAllMasterKeysAsync();
            var masterKeysViewModel = _mapper.Map<List<MasterDataKey>, List<MasterDataKeyViewModel>>(masterKeys);
            // Hold all Master Keys in session
            HttpContext.Session.SetSession("MasterKeys", masterKeysViewModel);
            return View(new MasterKeysViewModel
            {
                MasterKeys = masterKeysViewModel == null ? null : masterKeysViewModel.ToList(),
                IsEdit = false
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 0 references
        public async Task<IActionResult> MasterKeys(MasterKeysViewModel masterKeys)
        {
            masterKeys.MasterKeys = HttpContext.Session.GetSession<List<MasterDataKeyViewModel>>("MasterKeys");
            if (!ModelState.IsValid)
            {
                return View(masterKeys);
            }

            var masterKey = _mapper.Map<MasterDataKeyViewModel, MasterDataKey>(masterKeys.MasterKeyInContext);
            if (masterKeys.IsEdit)
            {
                // Update Master Key
                await _masterData.UpdateMasterKeyAsync(masterKeys.MasterKeyInContext.PartitionKey, masterKey);
                masterKey = _mapper.Map<MasterDataKeyViewModel, MasterDataKey>(masterKeys.MasterKeyInContext);
                masterKey.IsActive = masterKeys.MasterKeyInContext.IsActive;
                masterKey.CreatedBy = User.Identity?.Name ?? "System";
                masterKey.UpdatedBy = User.Identity?.Name ?? "System"; // thêm dòng này
            }
            else
            {
                // Insert Master Key
                masterKey.RowKey = Guid.NewGuid().ToString();
                // Tự sinh PartitionKey tăng dần (ví dụ: đếm số lượng key hiện có)
                var allKeys = await _masterData.GetAllMasterKeysAsync();
                var nextId = allKeys.Count + 1;
                masterKey.PartitionKey = masterKey.Name;
                masterKey.CreatedBy = User.Identity?.Name ?? "System"; // hoặc tên người dùng mặc định
                masterKey.UpdatedBy = User.Identity?.Name ?? "System"; // thêm dòng này
                await _masterData.InsertMasterKeyAsync(masterKey);
            }

            return RedirectToAction("MasterKeys");
        }
        [HttpGet]
        public async Task<IActionResult> MasterValues()
        {
            ViewBag.MasterKeys = await _masterData.GetAllMasterKeysAsync();
            return View(new MasterValuesViewModel
            {
                MasterValues = new List<MasterDataValueViewModel>(),
                IsEdit = false
            });
        }
        [HttpGet]
        public async Task<IActionResult> MasterValuesByKey(string key)
        {
            var masterValues = await _masterData.GetAllMasterValuesByKeyAsync(key);
            var masterValuesViewModel = masterValues.Select(x => new MasterDataValueViewModel
            {
                RowKey = x.RowKey ?? "",
                PartitionKey = x.PartitionKey ?? "",
                Name = x.Name ?? "",
                IsActive = x.IsActive // bool không cần check null
            }).ToList();

            return Json(new { data = masterValuesViewModel });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterValues(string submitType, MasterValuesViewModel model)
        {
            ViewBag.MasterKeys = await _masterData.GetAllMasterKeysAsync();

            if (submitType == "Reset")
                return RedirectToAction(nameof(MasterValues));

            if (!ModelState.IsValid)
            {
                model.MasterValues = await _masterData.GetAllMasterValuesByKeyAsync(model.MasterValueInContext.PartitionKey)
                    .ContinueWith(t => t.Result.Select(x => new MasterDataValueViewModel
                    {
                        RowKey = x.RowKey,
                        PartitionKey = x.PartitionKey,
                        Name = x.Name,
                        IsActive = x.IsActive
                    }).ToList());
                return View(model);
            }

            var entity = _mapper.Map<MasterDataValueViewModel, MasterDataValue>(model.MasterValueInContext);

            if (model.IsEdit)
            {
                await _masterData.UpdateMasterValueAsync(entity.PartitionKey, entity.RowKey, entity);
            }
            else
            {
                entity.RowKey = Guid.NewGuid().ToString();
                entity.CreatedBy = HttpContext.User.GetCurrentUserDetails().Name;
                entity.UpdatedBy = HttpContext.User.GetCurrentUserDetails().Name;
                await _masterData.InsertMasterValueAsync(entity);
            }

            return RedirectToAction(nameof(MasterValues));
        }

        private async Task<List<MasterDataValue>> ParseMasterDataExcel(IFormFile excelFile)
        {
            var masterValueList = new List<MasterDataValue>();

            // Kích hoạt license cá nhân
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var memoryStream = new MemoryStream())
            {
                await excelFile.CopyToAsync(memoryStream);
                using (var package = new ExcelPackage(memoryStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        //string GetCellString(int col) => worksheet.Cells[row, col].Value?.ToString()?.Trim();

                        //bool ParseBool(string input)
                        //{
                        //    return bool.TryParse(input, out var result) ? result : false;
                        //}

                        //DateTime? ParseDate(string input)
                        //{
                        //    return DateTime.TryParse(input, out var dt) ? dt : null;
                        //}

                        var masterDataValue = new MasterDataValue
                        {
                            RowKey = Guid.NewGuid().ToString(), // ✅ Auto
                            PartitionKey = worksheet.Cells[row, 1].Value.ToString(),
                            IsActive = Boolean.Parse(worksheet.Cells[row, 3].Value.ToString()),   // TRUE/FALSE
                            Name = worksheet.Cells[row, 2].Value.ToString(),
                            IsDeleted = false,     // TRUE/FALSE
                            CreatedDate = DateTime.Now,
                            CreatedBy = HttpContext.User.GetCurrentUserDetails().Name,

                        };

                        masterValueList.Add(masterDataValue);
                    }
                }
            }

            return masterValueList;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadExcel()
        {
            var files = Request.Form.Files;
            // Validations
            if (!files.Any())
            {
                return Json(new { Error = true, Text = "Upload a file" });
            }
            var excelFile = files.First();
            if (excelFile.Length <= 0)
            {
                return Json(new { Error = true, Text = "Upload a file" });
            }

            // Parse Excel Data
            var masterData = await ParseMasterDataExcel(excelFile);
            var result = await _masterData.UploadBulkMasterData(masterData);

            return Json(new { Success = result });
        }
    }
}
