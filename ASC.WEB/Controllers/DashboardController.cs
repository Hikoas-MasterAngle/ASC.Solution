using ASC.Business.Interfaces;
using ASC.Model.BaseTypes;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.WEB.Areas.ServiceRequests.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.WEB.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class DashboardController : Controller
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;

        public DashboardController(IServiceRequestOperations operations)
        {
            _serviceRequestOperations = operations;
        }

        public async Task<IActionResult> Dashboard()
        {
            var status = new List<string>
            {
                Status.New.ToString(),
                Status.InProgress.ToString(),
                Status.Initiated.ToString(),
                Status.RequestForInformation.ToString()
            };

            var email = HttpContext.User.GetCurrentUserDetails().Email;
            List<ServiceRequest> serviceRequests = new List<ServiceRequest>();

            if (HttpContext.User.IsInRole(Roles.Admin.ToString()))
            {
                // ✅ Không lọc theo RequestedDate
                serviceRequests = await _serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(
                    null, status);
            }
            else if (HttpContext.User.IsInRole(Roles.Engineer.ToString()))
            {
                serviceRequests = await _serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(
                    null, status, serviceEngineerEmail: email);
            }
            else
            {
                serviceRequests = await _serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(
                    null, null, email: email);
            }

            return View(new DashboardViewModel
            {
                ServiceRequests = serviceRequests.OrderByDescending(p => p.RequestedDate).ToList()
            });
        }
    }
}
