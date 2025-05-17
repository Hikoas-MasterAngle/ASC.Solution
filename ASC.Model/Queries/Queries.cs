using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ASC.Model.Models;
using ASC.Model.Queries;
using ASC.Utilities;
namespace ASC.Model.Queries
{
    public static class Queries
    {
        public static Expression<Func<ServiceRequest, bool>> GetDashboardQuery(
    DateTime? requestedDate,
    List<string> status = null,
    string email = "",
    string serviceEngineerEmail = "")
        {
            var query = (Expression<Func<ServiceRequest, bool>>)(u => true);

            if (requestedDate.HasValue)
            {
                query = query.And(u => u.RequestedDate == requestedDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.And(u => u.PartitionKey == email);
            }

            if (!string.IsNullOrWhiteSpace(serviceEngineerEmail))
            {
                query = query.And(u => u.ServiceEngineer == serviceEngineerEmail);
            }

            if (status != null && status.Count > 0)
            {
                Expression<Func<ServiceRequest, bool>> statusQuery = u => false;
                foreach (var state in status)
                {
                    string temp = state;
                    statusQuery = statusQuery.Or(u => u.Status == temp);
                }

                query = query.And(statusQuery);
            }

            return query;
        }

    }
}
