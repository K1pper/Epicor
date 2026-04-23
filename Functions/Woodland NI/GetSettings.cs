////////////////////////////////////////////////////////////////////////
// Mandata Shipping Functions
////////////////////////////////////////////////////////////////////////
// Function to retrieve Mandata setting from the MANDATA User codes
// Version 1.0
// 
// Written by Peter Roden - GHA Solutions - 12/01/2026
////////////////////////////////////////////////////////////////////////

var mandataUDCodes = Db.UDCodes
  .Where(r => r.Company == Session.CompanyID)
  .Where(r => r.CodeTypeID == "MANDATA")
  .ToList();

if (mandataUDCodes == null)
{
  IsError = true;
  ErrorMessage = "Cannot find MANDATA user code";
  return;
}

var test = mandataUDCodes
  .Where(r => r.CodeID == "Test")
  .Where(r => r.CodeDesc.ToLower() == "true")
  .Any();


try
{
  BaseURL = test ? mandataUDCodes.Where(r => r.CodeID == "TBaseURL").Select(r => r.CodeDesc).FirstOrDefault() : mandataUDCodes.Where(r => r.CodeID == "BaseURL").Select(r => r.CodeDesc).FirstOrDefault();
  Company = test ? mandataUDCodes.Where(r => r.CodeID == "TCompany").Select(r => r.CodeDesc).FirstOrDefault() : mandataUDCodes.Where(r => r.CodeID == "Company").Select(r => r.CodeDesc).FirstOrDefault();
  Depot = test ? mandataUDCodes.Where(r => r.CodeID == "TDepot").Select(r => r.CodeDesc).FirstOrDefault() : mandataUDCodes.Where(r => r.CodeID == "Depot").Select(r => r.CodeDesc).FirstOrDefault();
  Password = test ? mandataUDCodes.Where(r => r.CodeID == "TPassword").Select(r => r.CodeDesc).FirstOrDefault() : mandataUDCodes.Where(r => r.CodeID == "Password").Select(r => r.CodeDesc).FirstOrDefault();
  UserName = test ? mandataUDCodes.Where(r => r.CodeID == "TUserName").Select(r => r.CodeDesc).FirstOrDefault() : mandataUDCodes.Where(r => r.CodeID == "UserName").Select(r => r.CodeDesc).FirstOrDefault();
  Account = test ? mandataUDCodes.Where(r => r.CodeID == "TAccount").Select(r => r.CodeDesc).FirstOrDefault() : mandataUDCodes.Where(r => r.CodeID == "Account").Select(r => r.CodeDesc).FirstOrDefault();
  LastPollDate = test ? mandataUDCodes.Where(r => r.CodeID == "TLastPoll").Select(r => r.CodeDesc).FirstOrDefault() : mandataUDCodes.Where(r => r.CodeID == "LastPoll").Select(r => r.CodeDesc).FirstOrDefault();
  MapKey = mandataUDCodes.Where(r => r.CodeID == "MapKey").Select(r => r.CodeDesc).FirstOrDefault();

  if (BaseURL == "-") BaseURL = "";
  if (Company == "-") Company = "";
  if (Depot == "-") Depot = "";
  if (Password == "-") Password = "";
  if (UserName == "-") UserName = "";
  if (Account == "-") Account = "";
  if (LastPollDate == "-") LastPollDate = DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss");
  if (MapKey == "-") MapKey = "";

  var input = "2026-03-24T12:41:13";

  if (DateTime.TryParse(LastPollDate, out var dt))
  {
    LastPollDate = dt.AddMinutes(-2).ToString("yyyy-MM-ddTHH:mm:ss");
  }
}
catch
{
  IsError = true;
  ErrorMessage = "Could not load MANDATA user codes";
  return;
}

