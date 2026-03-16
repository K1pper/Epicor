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
}
catch
{
    IsError = true;
    ErrorMessage = "Could not load MANDATA user codes";
    return;
}

