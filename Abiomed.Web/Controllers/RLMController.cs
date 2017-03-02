/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * RLMController.cs: RLM Controller for getting initial view
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

using System.Web.Mvc;

namespace Abiomed.Web
{
    public class RLMController : Controller
    {
        // GET: RLM
        public ActionResult Index()
        {
            return View();
        }
    }
}