using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoreBot.Helpers
{
    public class OracleHelper
    {
        public static List<string> getOracleDBBranches(IConfiguration configuration)
        {
            try
            {
                List<string> lstDbrepos = new List<string>();
                string connString = configuration["OracleConnectionString"];
                string data = string.Empty;
                using (OracleConnection connection = new OracleConnection(connString))
                {
                    using (OracleCommand command = connection.CreateCommand())
                    {
                        connection.Open();
                        command.BindByName = true;
                        command.CommandText = "WITH CTE AS (SELECT SUBSTR(RULES.UTILS.GET_APP_VERSION,INSTR(RULES.UTILS.GET_APP_VERSION, '-') + 1,INSTR(RULES.UTILS.GET_APP_VERSION, '-', -1)- INSTR(RULES.UTILS.GET_APP_VERSION, '-')" +
                            " - 1) + SNO AS RELEASE FROM DUAL CROSS JOIN(SELECT 0 SNO FROM DUAL UNION SELECT 1 SNO FROM DUAL UNION SELECT 2 AS SNO FROM DUAL )" +
                            " SNO) SELECT 'ICM-DEV-9-' || RELEASE || '-0'   FROM CTE  WHERE INSTR(RULES.UTILS.GET_APP_VERSION @PMPRD1.iht.com, RELEASE)= 0";
                        OracleDataReader reader = command.ExecuteReader();
                        int i = 0;
                        while (reader.Read())
                        {
                            lstDbrepos.Add(reader.GetString(i));
                        }
                        reader.Dispose();
                    }
                }
                return lstDbrepos;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public static List<string> getVersionsfromInfluxDb(IConfiguration configuration, string env)
        {
            List<string> lstversions = new List<string>();
            try
            {
                var url = configuration["ETLInfluxQuery"];
                url = url.Replace("EnvVal", env);
                var client = new HttpClient();
                client.BaseAddress = new Uri(url);
                HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                var res = (JObject)JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                var values = res["results"].Children().First();
                if (values.Count() > 1)
                {
                    lstversions.Add("Skip");
                    values = values["series"].Children().First()["values"];
                    foreach (var val in values)
                    {
                        if (val.Children().Last().ToString() != "NA")
                            lstversions.Add(val.Children().Last().ToString());
                    }
                    lstversions = lstversions.Distinct().ToList();
                    return lstversions;
                }
                else return null;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
