using Sample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sample.Controllers
{
    public class ExpenseController : Controller
    {
        public static contextPage _context = new contextPage();
        // GET: Expense
        [AllowAnonymous]
        public ActionResult Index()
        {

            if (Session["Username"] != null && !string.IsNullOrEmpty(Session["Username"].ToString()))
            {
                ViewBag.usersname = Session["Username"];
                return View();
            }
            else
            {
                return  RedirectToAction("Login", "Expense");
            }
        }
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Logout()
        {
            
            Session.Clear(); 
            Session.Abandon();
            return RedirectToAction("Login", "Expense"); 
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["LoginAccess"] == null || !"Y".Equals(Session["LoginAccess"].ToString()))
            {
                if (filterContext.ActionDescriptor.ActionName != "Login" && filterContext.Controller.ControllerContext.RouteData.Values["controller"].ToString() != "Expense")
                {
                    filterContext.Result = RedirectToAction("Login", "Expense");
                }
            }

            base.OnActionExecuting(filterContext);
        }


        [HttpPost]
        public JsonResult GetTableData(string dateValue,string types)
        {
            var data = _context.getMethod(dateValue, types);
            return Json(data);
        }
        //
        [HttpPost]
        public JsonResult TracksAmount(string dateValue)
        {
            var data = _context.TracksAmount(dateValue);
            return Json(data);
        }
        [HttpPost]
        public JsonResult SaveExpense(ExpenseModel formData)
        {
            Response response = _context.SaveExpense(formData);
            return Json(response);
        }
        [HttpPost]
        public JsonResult GetCategory()
        {
            Response response = _context.DistinctCategory();
            return Json(response);
        }
        [HttpPost]
        public JsonResult EditExpense(string rowValue)
        {
            Response response = _context.EditExpense(rowValue);
            return Json(response);
        }
        [HttpPost]
        public JsonResult DeleteExpense(string rowValue)
        {
            Response response = _context.DeleteExpense(rowValue);
            return Json(response);
        }
        [HttpPost]
        public JsonResult SaveRegister(ExpenseRegister formData)
        {
            Response response = _context.SaveRegister(formData);
            return Json(response);
        }
        [HttpPost]
        public JsonResult LoginUser(ExpenseRegister formData)
        {
            Response response = _context.LoginUser(formData);
            return Json(response);


        }
        [HttpPost]
        public JsonResult GetViewCategory()
        {
            Response response = _context.GetViewCategory();
            return Json(response);
        }
        [HttpPost]
        public JsonResult SaveNewCategory(CategoryAdd formData)
        {
            Response response = _context.SaveNewCategory(formData);
            return Json(response);
        }
        [HttpPost]
        public JsonResult DeleteCategory(string rowValue)
        {
            Response response = _context.DeleteCategory(rowValue);
            return Json(response);
        }

        [HttpGet]
        public ActionResult CSVExport()
        {
            return _context.ExportToCSV();
        }

        [HttpGet]
        public ActionResult PdfExport()
        {
            return _context.ExportToPDF();
        }

    }
}