using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Stream.ViewModels;
using Teknik.Controllers;

namespace Teknik.Areas.Stream.Controllers
{
    public class StreamController : DefaultController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Teknikam";
            StreamViewModel model = new StreamViewModel();

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ViewStream(int id)
        {
            if (Config.StreamConfig.Enabled)
            {
                if (id > 0 && id <= Config.StreamConfig.Sources.Count)
                {
                    // ID is valid, so let's get the stream
                    string source = Config.StreamConfig.Sources[id - 1];

                    System.IO.Stream stream = null;

                    //This controls how many bytes to read at a time and send to the client
                    int bytesToRead = 10000;

                    // Buffer to read bytes in chunk size specified above
                    byte[] buffer = new Byte[bytesToRead];


                    try
                    {
                        //Create a WebRequest to get the file
                        HttpWebRequest fileReq = (HttpWebRequest)HttpWebRequest.Create(source);

                        //Create a response for this request
                        HttpWebResponse fileResp = (HttpWebResponse)fileReq.GetResponse();

                        if (fileReq.ContentLength > 0)
                            fileResp.ContentLength = fileReq.ContentLength;

                        //Get the Stream returned from the response
                        stream = fileResp.GetResponseStream();

                        return File(stream, "image/jpeg");
                    }
                    catch (Exception ex)
                    {
                        return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
                    }
                }
                return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http403"));
        }
    }
}