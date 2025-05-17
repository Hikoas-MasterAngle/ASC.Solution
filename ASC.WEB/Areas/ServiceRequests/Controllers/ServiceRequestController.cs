using ASC.Business.Interfaces;
using ASC.Model.BaseTypes;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.WEB.Areas.ServiceRequests.Models;
using ASC.WEB.Controllers;
using ASC.WEB.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.WEB.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IMapper _mapper;
        private readonly IMasterDataCacheOperations _masterData;

        public ServiceRequestController(IServiceRequestOperations operations, IMapper mapper, IMasterDataCacheOperations masterData)
        {
            _serviceRequestOperations = operations;
            _mapper = mapper;
            _masterData = masterData;
        }

        private async Task LoadMasterDataToViewBagAsync()
        {
            var masterDataCache = await _masterData.GetMasterDataCacheAsync();

            ViewBag.VehicleTypes = masterDataCache?.Values?
                .Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString() && !p.IsDeleted)
                .ToList() ?? new List<MasterDataValue>();

            ViewBag.VehicleNames = masterDataCache?.Values?
                .Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString() && !p.IsDeleted)
                .ToList() ?? new List<MasterDataValue>();
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequest()
        {
            await LoadMasterDataToViewBagAsync();
            return View(new NewServiceRequestViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ServiceRequest(NewServiceRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var serviceRequest = _mapper.Map<ServiceRequest>(model);
                var currentUser = HttpContext.User.GetCurrentUserDetails()?.Email ?? "system";

                serviceRequest.RowKey = Guid.NewGuid().ToString();
                serviceRequest.PartitionKey = currentUser;

                serviceRequest.CreatedBy = currentUser;
                serviceRequest.UpdatedBy = currentUser;
                serviceRequest.CreatedDate = DateTime.UtcNow;
                serviceRequest.UpdatedDate = DateTime.UtcNow;

                serviceRequest.RequestedDate = model.RequestedDate ?? DateTime.UtcNow;
                serviceRequest.Status = Status.New.ToString();

                await _serviceRequestOperations.CreateServiceRequestAsync(serviceRequest);

                return RedirectToAction("Dashboard", "Dashboard", new { Area = "ServiceRequests" });
            }

            await LoadMasterDataToViewBagAsync();
            return View(model);
        }
    }
}
