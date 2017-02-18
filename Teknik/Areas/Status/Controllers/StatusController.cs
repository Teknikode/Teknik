using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Status.ViewModels;
using Teknik.Attributes;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Models;
using Teknik.Utilities;

namespace Teknik.Areas.Status.Controllers
{
    [TeknikAuthorize]
    public class StatusController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Status Information - " + Config.Title;
            ViewBag.Description = "Current status information for the server and resources.";

            StatusViewModel model = new StatusViewModel();

            // Load initial status info

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetUsage()
        {
            try
            {
                float totalCPUValue = 0;
                float webCPUValue = 0;
                float dbCPUValue = 0;

                float totalMem = 0;
                float totalAvailMemValue = 0;
                float totalUsedMemValue = 0;
                float webMemValue = 0;
                float dbMemValue = 0;

                float bytesSent = 0;
                float bytesReceived = 0;

                // CPU
                using (PerformanceCounter totalCPU = new PerformanceCounter("Processor", "% Processor Time", "_Total", true))
                using (PerformanceCounter webCPU = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName, true))
                using (PerformanceCounter dbCPU = new PerformanceCounter("Process", "% Processor Time", Config.StatusConfig.DatabaseProcessName, true))
                // Memory
                using (PerformanceCounter totalAvailMem = new PerformanceCounter("Memory", "Available Bytes", true))
                using (PerformanceCounter webMem = new PerformanceCounter("Process", "Private Bytes", Process.GetCurrentProcess().ProcessName, true))
                using (PerformanceCounter dbMem = new PerformanceCounter("Process", "Private Bytes", Config.StatusConfig.DatabaseProcessName, true))
                // Network
                using (PerformanceCounter sentPerf = new PerformanceCounter("Network Interface", "Bytes Sent/sec", Config.StatusConfig.NetworkInterface, true))
                using (PerformanceCounter receivedPerf = new PerformanceCounter("Network Interface", "Bytes Received/sec", Config.StatusConfig.NetworkInterface, true))
                {
                    // CPU Sample
                    totalCPU.NextValue();
                    if (Config.StatusConfig.ShowWebStatus)
                    {
                        webCPU.NextValue();
                    }
                    if (Config.StatusConfig.ShowDatabaseStatus)
                    {
                        dbCPU.NextValue();
                    }

                    // Network Sample
                    sentPerf.NextValue();
                    receivedPerf.NextValue();

                    // Wait the sample time
                    Thread.Sleep(1000);

                    // CPU Values
                    totalCPUValue = totalCPU.NextValue();
                    if (Config.StatusConfig.ShowWebStatus)
                    {
                        webCPUValue = webCPU.NextValue();
                    }
                    if (Config.StatusConfig.ShowDatabaseStatus)
                    {
                        dbCPUValue = dbCPU.NextValue();
                    }

                    // Memory Values
                    totalMem = Config.StatusConfig.TotalMemory;
                    totalAvailMemValue = totalAvailMem.NextValue();
                    totalUsedMemValue = totalMem - totalAvailMemValue;
                    if (Config.StatusConfig.ShowWebStatus)
                    {
                        webMemValue = webMem.NextValue();
                    }
                    if (Config.StatusConfig.ShowDatabaseStatus)
                    {
                        dbMemValue = dbMem.NextValue();
                    }

                    // Network Values
                    bytesSent = sentPerf.NextValue();
                    bytesReceived = receivedPerf.NextValue();

                    // Return usage info
                    return Json(new { result = new {
                        cpu = new { total = totalCPUValue, web = webCPUValue, db = dbCPUValue },
                        memory = new { total = totalMem, totalAvail = totalAvailMemValue, totalUsed = totalUsedMemValue, webUsed = webMemValue, dbUsed = dbMemValue },
                        network = new { sent = bytesSent, received = bytesReceived }
                    } }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = new { message = ex.GetFullMessage(true) } }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}