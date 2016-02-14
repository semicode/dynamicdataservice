using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Configuration;
using System.Threading.Tasks;
using System.Dynamic;
using System.Collections;
using System.IO;
using System.Text;

namespace DynamicDataService.Controllers {
    public class GetStoredProcController : ApiController {
        // GET: api/GetStoredProc

        [HttpGet]
        public HttpResponseMessage GetResults([FromUri]string procName) {
            string stroredProcedurName = procName;

            HttpResponseMessage response = Request.CreateResponse();
            response.Content = new PushStreamContent(
            async (outputStream, httpContent, transportContext) => {
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultCon"].ConnectionString;
                var asyncConnectionString = new SqlConnectionStringBuilder(connectionString) {
                    AsynchronousProcessing = true
                }.ToString();
                using (SqlConnection connection = new SqlConnection(asyncConnectionString)) {
                    connection.Open();
                    try {
                        using (var cmd = connection.CreateCommand()) {
                            cmd.CommandText = stroredProcedurName;
                            cmd.CommandType = CommandType.StoredProcedure;
                            using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) {

                                try {

                                    while (await reader.ReadAsync()) {
                                        var names = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();


                                        var expando = new ExpandoObject() as IDictionary<string, object>;

                                        for (var i = 0; i < names.Count; i++) {

                                            expando[names[i]] = await reader.IsDBNullAsync(i) ? null : await reader.GetFieldValueAsync<object>(i);

                                        }

                                        var str = await Newtonsoft.Json.JsonConvert.SerializeObjectAsync(expando);

                                        var buffer = UTF8Encoding.UTF8.GetBytes(str);

                                        await outputStream.WriteAsync(buffer, 0, buffer.Length);



                                    }

                                } finally {
                                    // Close output stream as we are done
                                    outputStream.Close();
                                }

                                ;
                            }
                        }
                    } finally {
                        connection.Close();
                    }

                }
            });
            return response;
        }

        // GET: api/GetStoredProc/5
        public string Get(int id) {


            return "value";
        }

        // POST: api/GetStoredProc
        public void Post([FromBody]string value) {
        }

        // PUT: api/GetStoredProc/5
        public void Put(int id, [FromBody]string value) {
        }

        // DELETE: api/GetStoredProc/5
        public void Delete(int id) {
        }
    }
}
