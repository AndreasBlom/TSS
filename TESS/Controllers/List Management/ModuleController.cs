﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security.AntiXss;
using TietoCRM.Models;

namespace TietoCRM.Controllers
{
    public class ModuleController : Controller
    {
        // GET: Module
        public ActionResult Index()
        {
            string amount = this.Request.QueryString["amount"];

            if (amount == "" || amount == null)
                amount = "0";

            this.ViewData.Add("Properties", typeof(TietoCRM.Models.view_Module).GetProperties());

            this.ViewData.Add("ControllerName", "module");

            this.ViewData.Add("Systems", this.GetAllSystemNames());

            this.ViewData["Title"] = "Articles";
            //GlobalVariables.MostVisitedSites = this.getMostVisitedSite();

            return View();
        }


        public String ModuleData()
        {
            this.Response.ContentType = "text/plain";
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_Module.getAllModules()) + "}";
        }

        public String ClassificationData()
        {
            String System = Request.Form["System"];
            List<String> classificationList = new List<String>();
            String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {  
                connection.Open();
                String queryTextClassification = @"SELECT Procapita, Indelning FROM V_Procapita WHERE Procapita = @Procapita";
                command.CommandText = queryTextClassification;
                command.Prepare();

                command.Parameters.AddWithValue("@Procapita", System);
                command.ExecuteNonQuery();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        classificationList.Add(reader["indelning"].ToString());
                    }
                }
            }
            string temp = classificationList[0];
            classificationList.Remove(temp);
            classificationList.Add(temp);

            return (new JavaScriptSerializer()).Serialize(classificationList);
        }

        public String ModuleJsonData()
        {
            this.Response.ContentType = "text/plain";
            List<view_Module> l = view_Module.getAllModules();
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(l) + "}";
        }

        public List<String> GetAllSystemNames()
        {
            List<String> SystemList = new List<String>();
            String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                String queryTextClassification = @"SELECT DISTINCT Procapita FROM V_Procapita";
                command.CommandText = queryTextClassification;
                command.Prepare();

                command.ExecuteNonQuery();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SystemList.Add(reader["Procapita"].ToString());
                    }
                }
            }

            return SystemList;
        }


        public String InsertModule()
        {
            try
            {
                String json = Request.Form["json"];

                Dictionary<String, Object> variables = null;

                try
                {
                    variables = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
                }
                catch
                {
                    return "0";
                }

                view_Module module = new view_Module();
                try
                {
                    foreach (KeyValuePair<String, object> variable in variables)
                    {
                        module.SetValue(variable.Key, variable.Value);
                    }

                    module.Insert();

                    return "1";
                }
                catch (Exception ex)
                {
                    return "0";
                }
            }
            catch
            {
                return "-1";
            }
            
        }

        public String SaveModule()
        {
            try
            {
                String oldArtNr = Request.Form["oldArtNr"];

                String json = Request.Form["json"];

                Dictionary<String, Object> variables = null;

                try
                {
                    variables = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
                }
                catch
                {
                    return "0";
                }

                view_Module module = new view_Module();
                module.Select("Article_number = " + oldArtNr);
                try
                {
                    foreach (KeyValuePair<String, object> variable in variables)
                    {
                        module.SetValue(variable.Key, variable.Value);
                    }

                    module.Update("Article_number = " + oldArtNr);

                    return "1";
                }
                catch (Exception ex)
                {
                    return "0";
                }
            }
            catch
            {
                return "-1";
            }
            

        }
    }
}