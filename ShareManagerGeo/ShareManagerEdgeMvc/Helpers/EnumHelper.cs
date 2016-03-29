using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShareManagerEdgeMvc.Models;

namespace ShareManagerEdgeMvc.Helpers
{
    public static class EnumHelper
    {


        public static List<SelectListItem> GetSelectList<TEnum>(string selectedItem)
        {
            List<SelectListItem> enumList = new List<SelectListItem>();
            foreach (TEnum data in Enum.GetValues(typeof(TEnum)))
            {
                string displayname;
                if (typeof(TEnum) == typeof(Status))
                {
                    displayname = StatusExt.AsDisplayString((Status)Enum.Parse(typeof(Status), data.ToString()));
                }
                else if (typeof(TEnum) == typeof(PermissionType))
                {
                    displayname = PermissionTypeExt.AsDisplayString((PermissionType)Enum.Parse(typeof(PermissionType), data.ToString()));
                }
                else if (typeof(TEnum) == typeof(RequestType))
                {
                    displayname = RequestTypeExt.AsDisplayString((RequestType)Enum.Parse(typeof(RequestType), data.ToString()));
                }
                else if (typeof(TEnum) == typeof(RequestStatus))
                {
                    displayname = RequestStatusExt.AsDisplayString((RequestStatus)Enum.Parse(typeof(RequestStatus), data.ToString()));
                }
                else if (typeof(TEnum) == typeof(RequestApprovalStatus))
                {
                    displayname = RequestApprovalStatusExt.AsDisplayString((RequestApprovalStatus)Enum.Parse(typeof(RequestApprovalStatus), data.ToString()));
                }
                else if (typeof(TEnum) == typeof(RequestAdPrincipalType))
                {
                    displayname = RequestAdPrincipalTypeExt.AsDisplayString((RequestAdPrincipalType)Enum.Parse(typeof(RequestAdPrincipalType), data.ToString()));
                }
                else
                {
                    // default to ENUM text value
                    displayname = data.ToString();
                }
                enumList.Add(new SelectListItem
                {
                    Text = displayname,
                    Value = ((int)Enum.Parse(typeof(TEnum), data.ToString())).ToString(),
                    Selected = selectedItem == data.ToString()
                });
                
            }

            return enumList;
        }

        public static List<SelectListItem> GetSelectList<TEnum>()
        {
            List<SelectListItem> enumList = new List<SelectListItem>();
            foreach (TEnum data in Enum.GetValues(typeof(TEnum)))
            {
                string displayname;
                if (typeof(TEnum) == typeof(Status))
                {
                    displayname = StatusExt.AsDisplayString((Status)Enum.Parse(typeof(Status), data.ToString()));
                }
                else if (typeof(TEnum) == typeof(PermissionType))
                {
                    displayname = PermissionTypeExt.AsDisplayString((PermissionType)Enum.Parse(typeof(PermissionType), data.ToString()));
                }
                else if (typeof(TEnum) == typeof(RequestType))
                {
                    displayname = RequestTypeExt.AsDisplayString((RequestType)Enum.Parse(typeof(RequestType), data.ToString()));
                }
                else if (typeof(TEnum) == typeof(RequestStatus))
                {
                    displayname = RequestStatusExt.AsDisplayString((RequestStatus)Enum.Parse(typeof(RequestStatus), data.ToString()));
                }
                else if (typeof(TEnum) == typeof(RequestApprovalStatus))
                {
                    displayname = RequestApprovalStatusExt.AsDisplayString((RequestApprovalStatus)Enum.Parse(typeof(RequestApprovalStatus), data.ToString()));
                }
                else
                {
                    // default to ENUM text value
                    displayname = data.ToString();
                }
                enumList.Add(new SelectListItem
                {
                    Text = displayname,
                    Value = ((int)Enum.Parse(typeof(TEnum), data.ToString())).ToString()
                    
                });

            }

            return enumList;
        }
    }
}