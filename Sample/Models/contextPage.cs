
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Web.Mvc;
using System.Globalization;

namespace Sample.Models
{
    public class contextPage
    {
        private MySqlDataAccess _dataAccess;
        public contextPage()
        {
            _dataAccess = new MySqlDataAccess();
        }
        public Response getMethod(string dateValue, string types)
        {
            var response = new Response();
            var sumCount = string.Empty;
            var filterType = string.Empty;
            MySqlDataAccess dataAccess = new MySqlDataAccess();
            try
            {
                switch (types)
                {
                   
                    case "Filter by":
                        filterType = "initial";
                        break;
                    case "Filter by Date":
                        filterType = "date";
                        break;
                    case "Filter by Date Range":
                        filterType = "daterange";
                        break;
                    case "Filter by Month":
                        filterType = "month";
                        break;
                    case "Filter by Year":
                        filterType = "year";
                        break;
                }
            dataAccess.OpenDb();
                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.Append(" SELECT a.id, b.cad_name AS category, a.`DESCRIPTION`, a.Amount, a.Date, a.Addedby, a.AddedOn ");
                sqlQuery.Append(" FROM expensemaster a ");
                sqlQuery.Append(" LEFT JOIN category b ON a.categ_id = b.cad_id ");
                if (!string.IsNullOrEmpty(dateValue))
                {
                    if (filterType == "daterange" && dateValue.Contains(" - "))
                    {
                        string[] dateParts = dateValue.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                        if (dateParts.Length == 2)
                        {
                            DateTime startDate = DateTime.ParseExact(dateParts[0].Trim(), "MM-dd-yyyy", CultureInfo.InvariantCulture);
                            DateTime endDate = DateTime.ParseExact(dateParts[1].Trim(), "MM-dd-yyyy", CultureInfo.InvariantCulture);

                            sqlQuery.AppendFormat(" WHERE a.Date BETWEEN '{0}' AND '{1}' ",
                                                  startDate.ToString("yyyy-MM-dd"),
                                                  endDate.ToString("yyyy-MM-dd"));
                        }
                    }
                    else if (filterType == "initial" || filterType == "date")
                    {
                        string datesValue = dateValue;
                        if (!DateTime.TryParseExact(datesValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                        {
                            DateTime parsedDate = DateTime.ParseExact(datesValue, "MM-dd-yyyy", CultureInfo.InvariantCulture);
                            datesValue = parsedDate.ToString("yyyy-MM-dd");
                        }
                        sqlQuery.AppendFormat(" WHERE a.Date = '{0}' ", datesValue);
                    }
                    else if (filterType == "month")
                    {
                        if (DateTime.TryParseExact(dateValue, "MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime monthDate) ||
        DateTime.TryParseExact(dateValue, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out monthDate))
                        {
                            sqlQuery.AppendFormat(" WHERE YEAR(a.Date) = {0} AND MONTH(a.Date) = {1} ", monthDate.Year, monthDate.Month);
                        }

                    }
                    else if (filterType == "year")
                    {
                        if (int.TryParse(dateValue, out int year))
                        {
                            sqlQuery.AppendFormat(" WHERE YEAR(a.Date) = {0} ", year);
                        }
                    }
                }
                DataSet getTable = dataAccess.getDs(sqlQuery.ToString(), "query");
                if (getTable.HasData())
                {
                    response.Data["distinctTable"] = getTable.ToJSON();
                    HttpContext.Current.Session["ExpenseTable"] = getTable.Tables[0];
                    response.ErrNum = 0;
                    response.ErrMsg = "Success";
                }
                else
                {
                    response.ErrNum = 1;
                    response.ErrMsg = "No categories found.";
                }
            }
            catch (Exception ex)
            {
                response.ErrNum = 3;
                response.ErrMsg = "An error occurred: " + ex.Message;
            }
            finally
            {
                dataAccess.CloseDb();
            }

            return response;
        }
        //
        public Response TracksAmount(string dateValue)
        {
            var response = new Response();
            MySqlDataAccess dataAccess = new MySqlDataAccess();
            try
            {
                dataAccess.OpenDb();
                StringBuilder sqlQuery = new StringBuilder();
                if (!string.IsNullOrEmpty(dateValue))
                {
                    sqlQuery.AppendFormat(" SELECT SUM(amount) AS totalAmount from expensemaster WHERE  Date ='{0}' ", dateValue);
                    DataSet getTables = dataAccess.getDs(sqlQuery.ToString(), "query");
                    if (getTables.HasData())
                    {
                        response.Data["totalAmount"] = getTables.ToJSON();
                    }
                    else
                    {
                        response.Data["totalAmount"] = 0;
                    }

                }
            }
            catch (Exception ex)
            {
                response.ErrNum = 3;
                response.ErrMsg = "An error occurred: " + ex.Message;
            }
            finally
            {
                dataAccess.CloseDb();
            }

            return response;
        }


        public Response DistinctCategory()
        {
            var response = new Response();
            MySqlDataAccess dataAccess = new MySqlDataAccess();
            try
            {
                dataAccess.OpenDb();
                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.Append(" SELECT cad_id, cad_name FROM category ");
                DataSet getCategory = dataAccess.getDs(sqlQuery.ToString(), "query");
                if (getCategory.HasData())
                {
                    response.Data["distinctCategory"] = new
                    {
                        query = getCategory.Tables[0].AsEnumerable()
                        .Select(row => new
                        {
                            cad_id = row["cad_id"],
                            cad_name = row["cad_name"]
                        }).ToList()
                    };
                    response.ErrNum = 0;
                    response.ErrMsg = "Success";
                }
                else
                {
                    response.ErrNum = 1;
                    response.ErrMsg = "No categories found.";
                }
            }
            catch (Exception ex)
            {
                response.ErrNum = 3;
                response.ErrMsg = "An error occurred: " + ex.Message;
            }
            finally
            {
                dataAccess.CloseDb();
            }

            return response;
        }


        public Response SaveExpense(ExpenseModel formData)
        {
            var response = new Response();
            MySqlDataAccess dataAccess = new MySqlDataAccess();
            try
            {

                dataAccess.OpenDb();
                StringBuilder query = new StringBuilder();
                query.AppendFormat("INSERT INTO expensemaster (categ_id,DESCRIPTION, Amount, Date, Addedby, AddedOn) ");
                query.AppendFormat("VALUES ('{0}', '{1}', {2}, '{3}', 'Kalai', NOW())",
                    formData.Category, formData.Description, formData.Amount, formData.Date);
                bool result = dataAccess.ExecuteAddEdit(query.ToString());

                if (result)
                {
                    response.ErrNum = 0;
                    response.ErrMsg = "Expense saved successfully.";
                }
                else
                {
                    response.ErrNum = 2;
                    response.ErrMsg = "Error in saving expense.";
                }
            }
            catch (Exception ex)
            {
                response.ErrNum = 3;
                response.ErrMsg = "An error occurred: " + ex.Message;
            }
            finally
            {
                dataAccess.CloseDb();
            }

            return response;
        }

        public Response EditExpense(string rowValue)
        {
            var response = new Response();
            MySqlDataAccess dataAccess = new MySqlDataAccess();
            try
            {
                dataAccess.OpenDb();
                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.Append(" SELECT * FROM expensemaster ");
                sqlQuery.AppendFormat(" WHERE id = '{0}' ", rowValue);
                DataSet getRowdetails = dataAccess.getDs(sqlQuery.ToString(), "query");
                if (getRowdetails.HasData())
                {
                    response.Data["distinctTableValue"] = getRowdetails.ToJSON();
                    response.ErrNum = 0;
                    response.ErrMsg = "Success";
                }
                else
                {
                    response.ErrNum = 1;
                    response.ErrMsg = "Problem in getting record.";
                }
            }
            catch (Exception ex)
            {
                response.ErrNum = 3;
                response.ErrMsg = "An error occurred: " + ex.Message;
            }
            finally
            {
                dataAccess.CloseDb();
            }

            return response;
        }

        public Response DeleteExpense(string rowValue)
        {
            var response = new Response();
            MySqlDataAccess dataAccess = new MySqlDataAccess();
            try
            {
                dataAccess.OpenDb();
                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.Append(" DELETE  FROM expensemaster ");
                sqlQuery.AppendFormat(" WHERE id = '{0}' ", rowValue);
                int rowsAffected = dataAccess.ExecuteNonQuery(sqlQuery.ToString());
                if (rowsAffected > 0)
                {
                    response.ErrNum = 0;
                    response.ErrMsg = "Record deleted Successfully";
                }
                else
                {
                    response.ErrNum = 1;
                    response.ErrMsg = "Problem in delete record.";
                }
            }
            catch (Exception ex)
            {
                response.ErrNum = 3;
                response.ErrMsg = "An error occurred: " + ex.Message;
            }
            finally
            {
                dataAccess.CloseDb();
            }

            return response;
        }

        public Response SaveRegister(ExpenseRegister formData)
        {
            var response = new Response();
            MySqlDataAccess dataAccess = new MySqlDataAccess();
            try
            {

                dataAccess.OpenDb();
                StringBuilder query = new StringBuilder();
                query.AppendFormat("SELECT COUNT(*) FROM users WHERE username = '{0}'", formData.userName);
                int usernameExists = dataAccess.ExecuteNonQuery(query.ToString());
                if (usernameExists > 0)
                {
                    response.ErrNum = 3;
                    response.ErrMsg = "Username Already Exists.";
                }
                else
                {
                    query.Clear();
                    var passwords = string.Empty; ;
                    if (!string.IsNullOrEmpty(formData.passwordValue))
                    {
                        passwords = "MD5('" + formData.passwordValue + "')";
                    }

                    query.AppendFormat("INSERT INTO users (username,password, firstname, lastname, email, mobile) ");
                    query.AppendFormat("VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                        formData.userName, formData.passwordValue, formData.firstName, formData.lastName, formData.emailValue, formData.mobilevalue);
                    bool result = dataAccess.ExecuteAddEdit(query.ToString());

                    if (result)
                    {
                        response.ErrNum = 0;
                        response.ErrMsg = "Data saved successfully.";
                    }
                    else
                    {
                        response.ErrNum = 2;
                        response.ErrMsg = "Error in saving Data.";
                    }
                }

            }
            catch (Exception ex)
            {
                response.ErrNum = 4;
                response.ErrMsg = "An error occurred: " + ex.Message;
            }
            finally
            {
                dataAccess.CloseDb();
            }

            return response;
        }

        public Response LoginUser(ExpenseRegister formData)
        {
            var response = new Response();
            MySqlDataAccess dataAccess = new MySqlDataAccess();
            try
            {

                dataAccess.OpenDb();
                StringBuilder query = new StringBuilder(); var passwords = string.Empty;
                query.AppendFormat("SELECT username,password FROM users WHERE username = '{0}' AND password='{1}' ", formData.userName, formData.passwordValue);
                DataSet getTable = dataAccess.getDs(query.ToString(), "query");
                if (getTable.HasData())
                {
                    response.ErrNum = 0;
                    response.ErrMsg = "Login authentication successfully.";
                    HttpContext.Current.Session["LoginAccess"] = "Y";
                    HttpContext.Current.Session["Username"] = formData.userName;
                }
                else
                {
                    response.ErrNum = 1;
                    response.ErrMsg = "Invalid username or password.";
                    HttpContext.Current.Session["LoginAccess"] = "N";
                    HttpContext.Current.Session["Username"] = string.Empty;
                }
            }
            catch (Exception ex)
            {
                response.ErrNum = 4;
                response.ErrMsg = "An error occurred: " + ex.Message;
            }
            finally
            {
                dataAccess.CloseDb();
            }

            return response;
        }

        public Response GetViewCategory()
        {
            var response = new Response();
            MySqlDataAccess dataAccess = new MySqlDataAccess();
            try
            {
                dataAccess.OpenDb();
                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.Append(" SELECT * FROM category ");
                DataSet getCategory = dataAccess.getDs(sqlQuery.ToString(), "query");
                if (getCategory.HasData())
                {
                    response.Data["distinctTablecategory"] = getCategory.ToJSON();
                    response.ErrNum = 0;
                    response.ErrMsg = "Success";
                }
                else
                {
                    response.ErrNum = 1;
                    response.ErrMsg = "No categories found.";
                }
            }
            catch (Exception ex)
            {
                response.ErrNum = 3;
                response.ErrMsg = "An error occurred: " + ex.Message;
            }
            finally
            {
                dataAccess.CloseDb();
            }

            return response;
        }

        public Response SaveNewCategory(CategoryAdd formData)
        {
            var response = new Response();
            MySqlDataAccess dataAccess = new MySqlDataAccess();
            try
            {

                dataAccess.OpenDb();
                StringBuilder query = new StringBuilder();
                if (!string.IsNullOrEmpty(formData.categoryrowId))
                {
                    query.AppendFormat("UPDATE  category SET cad_name = '{0}', description = '{1}' where cad_id = '{2}' ", formData.categoryName, formData.categoryDescription, formData.categoryrowId);
                }
                else
                {
                    query.AppendFormat("INSERT INTO category (cad_name,description, addedby, addedon) ");
                    query.AppendFormat("Values ('{0}','{1}', 'Kalai', NOW()) ", formData.categoryName, formData.categoryDescription);
                }
                bool result = dataAccess.ExecuteAddEdit(query.ToString());

                if (result)
                {
                    response.ErrNum = 0;
                    response.ErrMsg = "Data saved successfully.";
                }
                else
                {
                    response.ErrNum = 2;
                    response.ErrMsg = "Error in saving Data.";
                }

            }
            catch (Exception ex)
            {
                response.ErrNum = 4;
                response.ErrMsg = "An error occurred: " + ex.Message;
            }
            finally
            {
                dataAccess.CloseDb();
            }

            return response;
        }

        public Response DeleteCategory(string rowValue)
        {
            var response = new Response();
            MySqlDataAccess dataAccess = new MySqlDataAccess();
            try
            {
                dataAccess.OpenDb();
                StringBuilder sqlQuery = new StringBuilder();
                sqlQuery.Append(" DELETE  FROM category ");
                sqlQuery.AppendFormat(" WHERE cad_id = '{0}' ", rowValue);
                int rowsAffected = dataAccess.ExecuteNonQuery(sqlQuery.ToString());
                if (rowsAffected > 0)
                {
                    response.ErrNum = 0;
                    response.ErrMsg = "Record deleted Successfully";
                }
                else
                {
                    response.ErrNum = 1;
                    response.ErrMsg = "Problem in delete record.";
                }
            }
            catch (Exception ex)
            {
                response.ErrNum = 3;
                response.ErrMsg = "An error occurred: " + ex.Message;
            }
            finally
            {
                dataAccess.CloseDb();
            }

            return response;
        }

        public FileResult ExportToCSV()
        {
            DataTable dt = HttpContext.Current.Session["ExpenseTable"] as DataTable;
            if (dt == null || dt.Rows.Count == 0)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            var columnNames = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                var fields = row.ItemArray.Select(field => "\"" + field.ToString().Replace("\"", "\"\"") + "\"");
                sb.AppendLine(string.Join(",", fields));
            }

            byte[] fileBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return new FileContentResult(fileBytes, "text/csv")
            {
                FileDownloadName = "ExpenseData.csv"
            };
        }

        public FileResult ExportToPDF()
        {
            DataTable dt = HttpContext.Current.Session["ExpenseTable"] as DataTable;
            if (dt == null || dt.Rows.Count == 0)
            {
                return null;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                Document doc = new Document();
                PdfWriter.GetInstance(doc, stream);
                doc.Open();

                PdfPTable table = new PdfPTable(dt.Columns.Count);
                table.WidthPercentage = 100;

                foreach (DataColumn column in dt.Columns)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(column.ColumnName));
                    cell.BackgroundColor = new BaseColor(200, 200, 200);
                    table.AddCell(cell);
                }

                foreach (DataRow row in dt.Rows)
                {
                    foreach (var cell in row.ItemArray)
                    {
                        table.AddCell(cell.ToString());
                    }
                }

                doc.Add(table);
                doc.Close();

                byte[] fileBytes = stream.ToArray();
                return new FileContentResult(fileBytes, "application/pdf")
                {
                    FileDownloadName = "ExpenseData.pdf"
                };
            }
        }
    }

    public class ExpenseModel
    {
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Date { get; set; }
    }
    public class ExpenseRegister
    {
        public string firstName { get; set; }
        public string userName { get; set; }
        public string lastName { get; set; }
        public string emailValue { get; set; }
        public string passwordValue { get; set; }
        public string mobilevalue { get; set; }
    }
    public class CategoryAdd
    {
        public string categoryName { get; set; }
        public string categoryDescription { get; set; }
        public string categoryrowId { get; set; }
    }
    public static class DataSetExtensions
    {
        // Convert DataSet to JSON
        public static string ToJSON(this DataSet ds)
        {
            return JsonConvert.SerializeObject(ds, Formatting.Indented);
        }

        // Check if DataSet has data (at least one table with rows)
        public static bool HasData(this DataSet ds)
        {
            return ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0;
        }
    }
}