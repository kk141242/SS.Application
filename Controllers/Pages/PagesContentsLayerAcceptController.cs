﻿using System;
using System.Collections.Generic;
using System.Web.Http;
using SiteServer.Plugin;
using SS.Application.Core;
using SS.Application.Core.Model;
using SS.Application.Core.Provider;
using SS.Application.Core.Utils;

namespace SS.Application.Controllers.Pages
{
    [RoutePrefix("pages/contentsLayerAccept")]
    public class PagesContentsLayerAcceptController : ApiController
    {
        private const string Route = "";

        [HttpGet, Route(Route)]
        public IHttpActionResult GetConfig()
        {
            try
            {
                var request = Context.GetCurrentRequest();
                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, ApplicationUtils.PluginId)) return Unauthorized();

                var contentIdList = TranslateUtils.StringCollectionToIntList(request.GetQueryString("contentIds"));

                var dataInfoList = new List<DataInfo>();
                foreach (var contentId in contentIdList)
                {
                    var contentInfo = DataDao.GetDataInfo(contentId);
                    if (contentInfo == null || contentInfo.State != DataState.New.Value) continue;

                    dataInfoList.Add(contentInfo);
                }

                var departmentInfoList = DepartmentManager.GetDepartmentInfoList(siteId);

                return Ok(new
                {
                    Value = dataInfoList,
                    DepartmentInfoList = departmentInfoList
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route(Route)]
        public IHttpActionResult Submit()
        {
            try
            {
                var request = Context.GetCurrentRequest();
                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, ApplicationUtils.PluginId)) return Unauthorized();

                var contentIdList = TranslateUtils.StringCollectionToIntList(request.GetPostString("contentIds"));
                var departmentId = request.GetPostInt("departmentId");

                foreach (var contentId in contentIdList)
                {
                    DataDao.UpdateStateAndDepartmentId(siteId, contentId, DataState.Accepted, departmentId);

                    LogManager.Accept(siteId, contentId, request.AdminId, departmentId);
                }

                return Ok(new
                {
                    Value = contentIdList
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
