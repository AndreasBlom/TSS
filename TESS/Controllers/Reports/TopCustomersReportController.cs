﻿using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using TietoCRM.Models;

namespace TietoCRM.Controllers.Reports
{
    public class TopCustomersReportController : Controller
    {
        // GET: TopCustomers
        public ActionResult Index()
        {
            ViewData.Add("Areas", view_Sector.getAllAreas().Where(a => System.Web.HttpContext.Current.GetUser().IfSameArea(a)));
            ViewData.Add("Title", "Top Customers Report");

            return View();
        }

        public String GetUsers()
        {
            String area = Request["area"];

            return (new JavaScriptSerializer()).Serialize(view_User.getAllUsers().Where(u => u.IfSameArea(area)));
        }

        public ActionResult Pdf()
        {
            String user = Request["user"];
            String area = Request["area"];
            String year = Request["year"];
            List<Dictionary<String, Object>> contracts = GenerateTopCustomers(user, area, year, 10);
            ViewData.Add("Contracts", contracts);

            this.ViewData["Title"] = "Top Customers Report";

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"\"";
            pdf.RotativaOptions.CustomSwitches += " --header-center \"Top Customers Report\"";

            return pdf;
        }

        public String TopCustomers()
        {
            String user = Request["user"];
            String area = Request["area"];
            String year = Request["year"];
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(GenerateTopCustomers(user, area, year, 10)) + "}";
        }

        /// <summary>
        /// Creats a list with customers which has most total spent. 
        /// </summary>
        /// <param name="user">Select customers by this user</param>
        /// <param name="area">Select customers which are in this Area</param>
        /// <param name="year">Which year to select from</param>
        /// <param name="ammount">How many results to return</param>
        /// <returns></returns>
        public static List<Dictionary<String, Object>> GenerateTopCustomers(String user, String area, String year, int ammount)
        {
            List<view_Customer> customers;
            if (area == "*")
                customers = view_Customer.getAllCustomers();
            else if(user == "*")
            {
                customers = new List<view_Customer>();
                List<view_User> users = view_User.getAllUsers().Where(u => u.IfSameArea(area)).ToList();
                foreach(view_User vUser in users)
                {
                    customers.AddRange(view_Customer.getAllCustomers(vUser.Sign));
                }
            }
            else
                customers = view_Customer.getAllCustomers(user);


            List<Dictionary<String, Object>> rows = new List<Dictionary<String, Object>>();
            foreach (view_Customer customer in customers)
            {
                Dictionary<String, Object> dict = new Dictionary<string, object>();
                CustomerStatistics stats = new CustomerStatistics(customer, true);

                try
                {
                    dict.Add("customer", customer.Customer);
                    dict.Add("amount", Convert.ToInt32(stats.GetTotalSpent(int.Parse(year), area)));
                    dict.Add("representative", customer.GetReprensentativesAsString());
                    dict.Add("customer_type", customer.Customer_type);
                    dict.Add("county", customer.County.ToString());

                    rows.Add(dict);
                }
                catch
                {

                }
            }
            return rows.OrderByDescending(d => d["amount"]).ToList().GetRange(0,Math.Min(ammount,rows.Count));
        }
    }
}