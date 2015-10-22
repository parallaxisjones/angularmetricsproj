using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PlayverseMetrics
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class AuthorizeUser : AuthorizeAttribute
    {
        private string errorSuffix { get; set; }
        private string loginPrefix = "You must log in to";
        private string permissionPrefix = "You do not have permission to";
        private bool userAuthenticated = true;

        public string[] Permissions { get; set; }
        public string Role { get; set; }

        public AuthorizeUser()
        {
            this.errorSuffix = String.Empty;
        }

        public AuthorizeUser(string errorSuffix)
        {
            this.errorSuffix = errorSuffix;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            userAuthenticated = true;
            string loginError = String.Format("{0} {1}", loginPrefix, errorSuffix);
            string permissionError = String.Format("{0} {1}", permissionPrefix, errorSuffix);

            if (!base.AuthorizeCore(httpContext))
            {
                userAuthenticated = false;
                //Messages.Instance.AddErrorMsg(loginError);
                return false;
            }

            // TODO: When we implement roles and permissions
            //UserModel userModel = new UserModel(httpContext.User);

            //if (Permissions != null && Permissions.Any())
            //{
            //    foreach (string permissionName in Permissions)
            //    {
            //        if (userModel.HasPermission(permissionName))
            //        {
            //            return true;
            //        }
            //    }

            //    Messages.Instance.AddErrorMsg(permissionError);
            //    return false;
            //}

            //if (!String.IsNullOrEmpty(Role))
            //{
            //    if (userModel.IsInRole(Role))
            //    {
            //        return true;
            //    }

            //    Messages.Instance.AddErrorMsg(permissionError);
            //    return false;
            //}

            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(
                    new
                    {
                        Controller = "Home",
                        Action = "Index"
                    })
                );

            //if (userAuthenticated)
            //{
            //    if (filterContext.HttpContext.Request.UrlReferrer != null)
            //    {
            //        filterContext.Result = new RedirectResult(filterContext.HttpContext.Request.UrlReferrer.LocalPath);
            //    }
            //    else
            //    {
            //        HomeModel homeModel = new HomeModel();
            //        filterContext.Result = new RedirectResult(homeModel.GetHomeUrl());
            //    }
            //}
            //else
            //{
            //    string returnURL = String.Empty;

            //    if (filterContext.HttpContext.Request.UrlReferrer != null && filterContext.HttpContext.Request.Form != null)
            //    {
            //        FormData.Instance.AddFormData(filterContext.HttpContext.Request.UrlReferrer.LocalPath, filterContext.HttpContext.Request.Form);
            //        returnURL = filterContext.HttpContext.Request.UrlReferrer.AbsoluteUri;
            //    }
            //    else
            //    {
            //        HomeModel homeModel = new HomeModel();

            //        returnURL = homeModel.GetHomeUrl();
            //    }

            //    UserModel userModel = new UserModel(filterContext.HttpContext.User);

            //    filterContext.Result = new RedirectResult(userModel.GetLoginUrl(returnURL));
            //}
        }
    }
}