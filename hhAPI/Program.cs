using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace hhAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            HhAPI hh = new HhAPI();
            hh.GetVacancies();
            Console.ReadLine();
        }
    }

    class HhAPI
    {
        public string token = "";//expired_at 2018-01-02T18:38:35+0300
        public string url = "https://api.hh.ru/";
        public string query = "vacancies?text=c%23+junior&area=1";//"vacancies"+ /*WebUtility.UrlEncode(*/"?"/*)*/+"area"+ WebUtility.UrlEncode("=")+"1"+ WebUtility.UrlEncode("&")+"text"+ WebUtility.UrlEncode("=")+"c"+ WebUtility.UrlEncode("#")+"%20junior";
        public string GetVacancies()
        {
            Console.WriteLine(url+query);
            try
            {
                string text = null;
                for (int page = 0; page < 10; page++)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + query + "&page=" + page.ToString());
                    request.UserAgent = "HH-User-Agent";
                    request.Method = WebRequestMethods.Http.Get;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream resStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(resStream, Encoding.UTF8);
                    text = reader.ReadToEnd();

                    File.WriteAllText(@"C:\C#\json.txt", text, Encoding.UTF8);
                    File.AppendAllText(@"C:\C#\vacancies.csv", DataTableToCSV(JsonToDataTable(text)), Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.ToString());
            }

            return null;
        }

        public static DataTable JsonToDataTable(string jsonContent)
        {
            RootObject root = JsonConvert.DeserializeObject<RootObject>(jsonContent);
            IList<PropertyInfo> properties = typeof(MyItem).GetProperties();
            DataTable table = new DataTable();
            foreach (PropertyInfo info in properties)
            {
                table.Columns.Add(info.Name);
            }
            foreach (MyItem item in root.items)
            {
                DataRow row = table.NewRow();
                foreach (PropertyInfo info in properties)
                {
                    string value = null;
                    //Console.WriteLine(info.PropertyType.ToString());
                    //Console.WriteLine(info.PropertyType);
                    //Console.WriteLine(item.salary.GetValue());
                    if (info.PropertyType.ToString().Substring(0, 5) == "hhAPI")
                    {
                        //Console.WriteLine("HH_API");
                        if (info.PropertyType == typeof(Salary))
                        {
                            if (item.salary != null)
                            {
                                value = item.salary.GetValue();
                                //Console.WriteLine(value);
                            }
                        }
                        if (info.PropertyType == typeof(Snippet))
                        {
                            if (item.snippet != null)
                            {
                                value = item.snippet.GetValue();
                            }
                        }
                        if (info.PropertyType == typeof(Area))
                        {
                            if (item.area != null)
                            {
                                value = item.area.GetValue();
                            }
                        }
                        if (info.PropertyType == typeof(Employer))
                        {
                            if (item.employer != null)
                            {
                                value = item.employer.GetValue();
                            }
                        }
                        if (info.PropertyType == typeof(Address))
                        {
                            if (item.address != null)
                            {
                                value = item.address.GetValue();
                            }
                        }
                        if (info.PropertyType == typeof(Type) && item.type != null)
                        {

                            value = item.type.GetValue();
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Common");
                        if (info.GetValue(item) != null)
                        {
                            var _value = info.GetValue(item).ToString();
                            value = _value.ToString();
                        }
                    }
                    row[info.Name] = value;
                }
                table.Rows.Add(row);
            }
            return table;
        }

        public static string DataTableToCSV(DataTable table)
        {
            StringBuilder csv = new StringBuilder();
            foreach (DataColumn column in table.Columns)
            {
                csv.Append(column.ColumnName.ToString() + ";");
            }
            csv.Replace(";", System.Environment.NewLine, csv.Length - 1, 1);
            foreach (DataRow row in table.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                csv.AppendLine(string.Join(";", fields));
            }
            return csv.ToString();
        }
    }
}




