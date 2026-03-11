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
  key.Select((x,i) => i > 0 && i % 4 == 0 ? new [] { '-', x } : new [] { x })
     .SelectMany(x => x)
     .ToArray()
);

var debugCode = this.Db.UDCodes
  .Where(r => r.Company == this.Session.CompanyID)
  .Where(r => r.CodeTypeID =="GHAMFS")
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
  bo => { versionResults = bo.ExecuteByID("GHA_MFS_Version",  new QueryExecutionTableset()); }
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
  
/********************************************************************************************************************************************/

if (debug)
{
  StringBuilder debugMessage = new StringBuilder();
  debugMessage.AppendLine("GHA: Token = " + licenseKey);
  debugMessage.AppendLine("GHA: HostName = " + hostname);
  debugMessage.AppendLine("GHA: Instance = " + instance);
  debugMessage.AppendLine("GHA: TestEnvir = " + testenvir);
  debugMessage.AppendLine("GHA: Version = " + version);
  debugMessage.AppendLine("GHA: URL = " + url + service);
  debugMessage.AppendLine("GHA: Message data");
  debugMessage.AppendLine(data);
  Ice.Diagnostics.Log.WriteEntry(debugMessage.ToString());
}

using (var client = new WebClient() { Encoding = Encoding.UTF8 } )
{
  try
  {
    client.Headers.Add("token", licenseKey);
    client.Headers.Add("hostname", hostname);
    client.Headers.Add("instance", instance);
    client.Headers.Add("testenvir", testenvir);
    client.Headers.Add("version", version);
    
    if (string.Compare(callContextClient.ClientType, string.Empty, true) == 0)
    {
      client.Headers.Add("sync", "sync");
    }
    
    
    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
    string response = client.UploadString(new Uri(url + service), "POST", data);
    
    if (debug) 
    {  
      StringBuilder debugMessage = new StringBuilder();
      debugMessage.AppendLine("GHA: Response");
      debugMessage.AppendLine(response);
      Ice.Diagnostics.Log.WriteEntry(debugMessage.ToString());
    }
    
    var result = JsonConvert.DeserializeObject<dynamic>(response);
    
    this.status = (bool)result.success ? "S" : "X";
    this.error = result.message.ToString();
  }
  catch (WebException we)
  {
    this.status = "X";
    using (StreamReader r = new StreamReader(we.Response.GetResponseStream()))
    {
        this.error = r.ReadToEnd();
    }
    
    StringBuilder debugMessage = new StringBuilder();
    debugMessage.AppendLine("GHA:Call Field Service Error");
    debugMessage.AppendLine(this.error);
    Ice.Diagnostics.Log.WriteEntry(debugMessage.ToString());
  }
}