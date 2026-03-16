var liveurl = "api.ghahosted.com/mobileportal/api/v1/EpicorSvc/";
var url = "https://" + liveurl;

var debug = false;

/********************************************************************************************************************************************/
var udCodes = this.Db.UDCodes
  .Where(r => r.Company == this.Session.CompanyID)
  .Where(r => r.CodeTypeID == "GHAMFS")
  .Where(r => r.CodeID == "LicenceKey")
  .FirstOrDefault();

if (udCodes == null) return;

var key = udCodes.CodeDesc;

var licenseKey = new string(
  key.Select((x, i) => i > 0 && i % 4 == 0 ? new[] { '-', x } : new[] { x })
     .SelectMany(x => x)
     .ToArray()
);

var debugCode = this.Db.UDCodes
  .Where(r => r.Company == this.Session.CompanyID)
  .Where(r => r.CodeTypeID == "GHAMFS")
  .Where(r => r.CodeID == "Debug")
  .FirstOrDefault();

if (string.Compare(debugCode?.CodeDesc, "true", true) == 0)
{
    debug = true;
}

if (debug) Ice.Diagnostics.Log.WriteEntry("GHA: Call Field Service Function");

/********************************************************************************************************************************************/

DataSet versionResults = new DataSet();

CallService<Ice.Contracts.DynamicQuerySvcContract>
(
  bo => { versionResults = bo.ExecuteByID("GHA_MFS_Version", new QueryExecutionTableset()); }
);

var version = versionResults.Tables["Results"].Rows[0]["Calculated_Version"].ToString();

/********************************************************************************************************************************************/
string hostname = string.Empty;
string instance = string.Empty;
string testenvir = string.Empty;

var appServerURl = Session.AppServerURL;
if (appServerURl != null)
{
    var appServerUrl = new Uri(appServerURl);
    hostname = appServerUrl.Host;
    instance = appServerUrl.AbsolutePath.Replace("/", "");
}

var testEnvironment = this.Db.UDCodes
  .Where(r => r.Company == this.Session.CompanyID)
  .Where(r => r.CodeTypeID == "GHAMFS")
  .Where(r => r.CodeID == "TestEnvir")
  .Where(r => r.CodeDesc.ToLower() == "true")
  .FirstOrDefault();

if (testEnvironment != null)
{
    testenvir = "true";
}
var environment = this.Db.UDCodes
  .Where(r => r.Company == this.Session.CompanyID)
  .Where(r => r.CodeTypeID == "GHAMFS")
  .Where(r => r.CodeID == "Environmen")
  .FirstOrDefault();

if (environment != null)
{
    url = "https://" + environment.CodeDesc + liveurl;
}

var beta = this.Db.UDCodes
.Where(r => r.Company == this.Session.CompanyID)
.Where(r => r.CodeTypeID == "GHAMFS")
.Where(r => r.CodeID == "Beta")
.FirstOrDefault();

if (string.Compare(beta?.CodeDesc, "true", true) == 0)
{
    url = "https://beta" + liveurl;
}

var sync = string.Compare(callContextClient.ClientType, string.Empty, true) == 0 ? "sync" : "nosync";


/********************************************************************************************************************************************/

if (debug)
{
    StringBuilder debugMessage = new StringBuilder();
    debugMessage.AppendLine("GHA: Token = " + licenseKey);
    debugMessage.AppendLine("GHA: HostName = " + hostname);
    debugMessage.AppendLine("GHA: Instance = " + instance);
    debugMessage.AppendLine("GHA: TestEnvir = " + testenvir);
    debugMessage.AppendLine("GHA: URL = " + url + service);
    debugMessage.AppendLine("GHA: Version = " + version);
    debugMessage.AppendLine("GHA: Message data");
    debugMessage.AppendLine(data);
    Ice.Diagnostics.Log.WriteEntry(debugMessage.ToString());
}

HttpClient client = new HttpClient();

try
{
    var httpRequestMessage = new HttpRequestMessage
    {
        Method = HttpMethod.Post,
        RequestUri = new Uri(url + service),
        Headers = {
    { HttpRequestHeader.Accept.ToString(), "application/json" },
    { HttpRequestHeader.ContentType.ToString(), "application/json" },
    { "token", licenseKey },
    { "hostname", hostname },
    { "instance", instance },
    { "testenvir", testenvir },
    { sync, sync }
    },
        Content = new StringContent(data, Encoding.UTF8, "application/json")
    };

    var response = client.SendAsync(httpRequestMessage).Result;

    if (response.IsSuccessStatusCode)
    {
        var result = response.Content.ReadAsStringAsync().Result;
        var s = Newtonsoft.Json.JsonConvert.DeserializeObject(result);
        this.status = "S";
        this.error = result;
        if (debug)
        {
            StringBuilder debugMessage = new StringBuilder();
            debugMessage.AppendLine("GHA: Response");
            debugMessage.AppendLine(result);
            Ice.Diagnostics.Log.WriteEntry(debugMessage.ToString());
        }
    }
    else
    {
        var result = response.Content.ReadAsStringAsync().Result;
        var s = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(result);
        this.status = "X";
        this.error = s.title;
        if (debug)
        {
            StringBuilder debugMessage = new StringBuilder();
            debugMessage.AppendLine("GHA: Response");
            debugMessage.AppendLine(s.title);
            Ice.Diagnostics.Log.WriteEntry(debugMessage.ToString());
        }
    }
}
catch (Exception ex)
{
    Ice.Diagnostics.Log.WriteEntry("Call Field Service Exception: " + ex.Message);
    this.error = ex.Message;

    StringBuilder debugMessage = new StringBuilder();
    debugMessage.AppendLine("GHA:Call Field Service Error");
    debugMessage.AppendLine(this.error);
    Ice.Diagnostics.Log.WriteEntry(debugMessage.ToString());
}

