﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;

namespace TietoCRM.Controllers.List_Management
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            GlobalVariables.checkIfAuthorized("CustomerContract");
            ViewBag.Properties = typeof(TietoCRM.Models.view_User).GetProperties();
            //this.ViewData.Add("Properties", typeof(TietoCRM.Models.view_User).GetProperties());
            this.ViewData["title"] = "User";

            return View();
        }

        public String UserJsonData()
        {
            this.Response.ContentType = "text/plain";
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_User.getAllUsers()) + "}";
        }

        /// <summary>
        /// Saves a user
        /// </summary>
        /// <returns></returns>
        public string SaveUser()
        {
            try
            {
                string sign = Request.Form["sign"];
                string json = Request.Form["json"];

                Dictionary<string, object> variables = null;

                try
                {
                    variables = (Dictionary<string, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<string, dynamic>));
                }
                catch
                {
                    return "0";
                }

                view_User user = new view_User();
                user.Select("Sign = " + sign);

                using (var scope = TransactionHelper.CreateTransactionScope())
                {
                    var auditLog = new view_AuditLog();

                    auditLog.LogUserChanges(user, variables);

                    foreach (KeyValuePair<string, object> variable in variables)
                    {
                        if (variable.Key != "Sign")
                        {
                            user.SetValue(variable.Key, variable.Value);
                        }

                    }

                    user.Update("Sign = " + sign);

                    scope.Complete();
                }

                return "1";
            }
            catch(Exception e)
            {
                return "-1";
            }
        }

        public String InsertUser()
        {
            try
            {
                String json = Request.Form["json"];
                view_User a = null;
                try
                {
                    a = (view_User)(new JavaScriptSerializer()).Deserialize(json, typeof(view_User));
                }
                catch (Exception e)
                {
                    return "0";
                }

                List<view_User> services = view_User.getAllUsers();

                using(var scope = TransactionHelper.CreateTransactionScope())
                {
                    a.Insert();

                    //Insert went well. Write to AuditLog
                    new view_AuditLog().Write("C", "view_User", a.Sign, a.Name);

                    scope.Complete();
                }

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String DeleteUser()
        {
            try
            {
                String sign = Request.Form["sign"];
                view_User a = new view_User();
                //a.Select("Article_number = " + value);

                using (var scope = TransactionHelper.CreateTransactionScope())
                {
                    a.Delete("Sign = " + sign);

                    //Delete went well. Write to AuditLog
                    new view_AuditLog().Write("D", "view_User", sign);

                    scope.Complete();
                }

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }
    }
}