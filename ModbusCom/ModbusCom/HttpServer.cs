using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Data;
using System.Linq;


namespace ModbusCom
{
    
    class HttpServer
    {
        private HttpListener listener = null;
        private DeviceInfoDB deviceInfoDB = null;
        private Dictionary<string, string> lastRecord = null;
        public HttpServer(Dictionary<string, string[]> columns = null)
        {
            listener = new HttpListener();
            listener.Prefixes.Add(AppConfig.ServiceURL);
            try
            {
                deviceInfoDB = new DeviceInfoDB(columns);
            }
            catch (Exception) { }
        }

        public void Start()
        {
            Console.WriteLine("Listening...");
            while (true)
            {
                listener.Start();
                var context = listener.GetContext();
                RequestHandler(context);
            }
        }

        public void RequestHandler(HttpListenerContext context)
        {            
            var requestType = AppConfig.MethodsTypes[context.Request.HttpMethod];
            string responseString = null;
            if (requestType == AppConfig.MethodsTypes[HttpMethod.Get.ToString()])
                responseString = HandleGetRequest();
            else if (requestType == AppConfig.MethodsTypes[HttpMethod.Post.ToString()])
                responseString = HandlePostRequest(context.Request);

            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            context.Response.ContentLength64 = buffer.Length;
            System.IO.Stream output = context.Response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();
            listener.Stop();
        }

        private string HandleGetRequest()
        {

            void MakeRow(ref StringBuilder stringBuilder, object[] values, bool header = false)
            {
                string cellFormat = "<td>{0}</td>";
                if (header)
                    cellFormat = "<th>{0}</th>";
                stringBuilder.Append("<tr>");
                foreach (var val in values) stringBuilder.Append(string.Format(cellFormat, val));
                stringBuilder.Append("</tr>");
            }

            string ReadHtmlFile()
            {
                string htmlCodeFormat = null;
                using (var web = new WebClient())
                {
                    htmlCodeFormat = web.DownloadString(AppConfig.TablePagePath);
                }
                return htmlCodeFormat;
            }

            string responseString = "<html><head><body>There is not measures.</body></head></html>";
            string htmlCodeFormat = ReadHtmlFile();
            StringBuilder stringBuilder = new StringBuilder();
            if (deviceInfoDB != null)
            {
                DataTable dataTable = deviceInfoDB.ReadData(AppConfig.DeviceRegTblName);
                var colNames = 
                    dataTable.Columns.Cast<DataColumn>().ToList().Select(
                        dataCol => dataCol.ColumnName).ToArray();
                MakeRow(ref stringBuilder, colNames, header: true);                
                
                foreach (DataRow row in dataTable.Rows)
                    MakeRow(ref stringBuilder, row.ItemArray);
                string tblName = AppConfig.DeviceRegTblName,
                    tblContent = stringBuilder.ToString();
                responseString = htmlCodeFormat.Replace("tblName", 
                    tblName).Replace("tblContent", tblContent);
            }
            else if (lastRecord != null)
            {
                MakeRow(ref stringBuilder, lastRecord.Keys.ToArray(), header: true);
                MakeRow(ref stringBuilder, lastRecord.Values.ToArray(), header: true);
                string tblName = "The last measure",
                    tblContent = stringBuilder.ToString();
                responseString = htmlCodeFormat.Replace("tblName", 
                    tblName).Replace("tblContent", tblContent);
            }
            return responseString;
        }

        private string HandlePostRequest(HttpListenerRequest request)
        {
            string responseString = "DataBase is not exists. Table keep only the last record.";
            int length = Convert.ToInt32(request.ContentLength64);
            byte[] buffer = new byte[length];
            request.InputStream.Read(buffer, 0, length);

            string requestContent = Encoding.UTF8.GetString(buffer);
            lastRecord = new Dictionary<string, string>();
            foreach (var nameVal in requestContent.Split("&"))
            {
                string[] pair = nameVal.Split("=");
                lastRecord[pair[0]] = pair[1];
            }
            if (deviceInfoDB != null)
            {
                deviceInfoDB.RecordData(AppConfig.DeviceRegTblName, lastRecord);
                responseString = "Successful.";
            }
            return responseString;
        }

        ~HttpServer()
        {
            if (listener.IsListening)
                listener.Stop();
        }
    }
}
