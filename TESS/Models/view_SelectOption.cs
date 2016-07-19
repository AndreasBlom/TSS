﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;

namespace TietoCRM.Models
{
    public class view_SelectOption : SelectOptionsBaseClass
    {
        private int _id;
        public int _ID { get; set; }

        private String model;
        public String Model { get; set; }

        private String property;
        public String Property { get; set; }

        private String value;
        public String Value { get; set; }

        private String text;
        public String Text { get; set; }

        public view_SelectOption(Boolean init = true) : base("SelectOption", init)
        {
            if(init)
                this.initTable();
        }
        public view_SelectOption() : base("SelectOption", false)
        {
        }
        
        protected override void initTable()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = @"   IF NOT EXISTS 
                                    (   SELECT  1
                                        FROM    " + databasePrefix + @"SelectOption 
                                        WHERE   Model = @Model 
                                        AND     Property = @Property
                                        AND     Value = @Value
                                    )
                                    BEGIN
                                        INSERT INTO " + databasePrefix + @"SelectOption (Model, Property, Value, Text)
                                        VALUES(@Model, @Property, @Value, @Text)
                                    END";
                foreach (String view in this.GetAllViews())
                {
                    SqlCommand commandView = new SqlCommand(query, connection);
                    commandView.Prepare();
                    commandView.Parameters.AddWithValue("@Model", this.GetType().Name.ToString());
                    commandView.Parameters.AddWithValue("@Property", "Model");
                    commandView.Parameters.AddWithValue("@Value", view);
                    String text = view.Replace("view_", "");
                    commandView.Parameters.AddWithValue("@Text", text);
                    commandView.ExecuteNonQuery();
                }

                SqlCommand command = new SqlCommand(query, connection);
                command.Prepare();
                command.Parameters.AddWithValue("@Model", this.GetType().Name.ToString());
                command.Parameters.AddWithValue("@Property", "Property");
                command.Parameters.AddWithValue("@Value", "Property");
                command.Parameters.AddWithValue("@Text", "Property");
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets all information.
        /// </summary>
        /// <returns>A lsit of users.</returns>
        public static List<view_SelectOption> getAllSelectOptionsWhere(String condition)
        {
            List<view_SelectOption> list = new List<view_SelectOption>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT ID,Model,Property,Value,Text FROM " + databasePrefix + "SelectOption WHERE " + condition;

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_SelectOption k = new view_SelectOption(false);
                        int i = 0;
                        while (reader.FieldCount > i)
                        {
                            k.SetValue(k.GetType().GetProperties()[i].Name, reader.GetValue(i));
                            i++;
                        }
                        list.Add(k);
                    }
                }
            }
            sw.Stop();
            Debug.Print(sw.Elapsed.TotalSeconds.ToString());
            return list;
        }
        public static List<view_SelectOption> getAllSelectOptions()
        {
            List<view_SelectOption> list = new List<view_SelectOption>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT ID,Model,Property,Value,Text FROM " + databasePrefix + "SelectOption";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_SelectOption k = new view_SelectOption(false);
                        int i = 0;
                        while (reader.FieldCount > i)
                        {
                            var prop = k.GetType().GetProperties()[i];
                            k.SetValue(prop.Name, reader.GetValue(i));
                            i++;
                        }
                        list.Add(k);
                    }
                }
            }
            return list;
        }

        private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }

        /// <summary>
        /// Return all models beginning with view_ 
        /// </summary>
        /// <returns>List of strings with view names</returns>
        public List<String> GetAllViews()
        {
            Type[] allClasses = this.GetTypesInNamespace(Assembly.GetExecutingAssembly(), "TietoCRM.Models");
            List<String> allViews = new List<String>();
            foreach(Type cT in allClasses)
            {
                if(cT.Name.StartsWith("view_"))
                {
                    allViews.Add(cT.Name);
                }
            }
            return allViews;
        }

    }
}