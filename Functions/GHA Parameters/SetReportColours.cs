
try
{
    // find corresponding parameters within UDCodes table
    IQueryable<UDCodes> reportCodes = Db.UDCodes
      .Where(r => r.Company == this.Session.CompanyID)
      .Where(r => r.CodeTypeID == "GHAReports");

    // set all colours to new values
    reportCodes.FirstOrDefault(r => r.CodeID == "ColBorders").CodeDesc = borders;
    reportCodes.FirstOrDefault(r => r.CodeID == "ColPrimary").CodeDesc = primary;
    reportCodes.FirstOrDefault(r => r.CodeID == "ColRowEven").CodeDesc = even;
    reportCodes.FirstOrDefault(r => r.CodeID == "ColRowOdd").CodeDesc = odd;
    reportCodes.FirstOrDefault(r => r.CodeID == "ColSecond").CodeDesc = secondary;
    reportCodes.FirstOrDefault(r => r.CodeID == "ColTxt").CodeDesc = text;
    reportCodes.FirstOrDefault(r => r.CodeID == "ColTxtHdr").CodeDesc = header;
    reportCodes.FirstOrDefault(r => r.CodeID == "ColTxtTitl").CodeDesc = title;

    Db.SaveChanges();
}
catch (Exception ex)
{
    output = $"{output}\nfailed to save colours: {ex.Message}";
}