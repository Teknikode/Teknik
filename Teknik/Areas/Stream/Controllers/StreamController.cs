using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Stream.ViewModels;
using Teknik.Attributes;
using Teknik.Controllers;
using Teknik.Filters;

namespace Teknik.Areas.Stream.Controllers
{
    [TeknikAuthorize]
    public class StreamController : DefaultController
    {
        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Teknikam";
            StreamViewModel model = new StreamViewModel();

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public FileStreamResult ViewStream(int id)
        {
            try
            {
                if (Config.StreamConfig.Enabled)
                {
                    if (id > 0 && id <= Config.StreamConfig.Sources.Count)
                    {
                        // ID is valid, so let's get the stream
                        string source = Config.StreamConfig.Sources[id - 1];
                        //Create a WebRequest to get the file
                        HttpWebRequest fileReq = (HttpWebRequest)HttpWebRequest.Create(source);

                        //Create a response for this request
                        HttpWebResponse fileResp = (HttpWebResponse)fileReq.GetResponse();

                        return File(fileResp.GetResponseStream(), fileResp.ContentType);
                    }
                }
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}