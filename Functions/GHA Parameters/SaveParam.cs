try
{
    UDCodes parameter = Db.UDCodes
      .Where(r => r.Company == this.Session.CompanyID)
      .Where(r => r.CodeTypeID == codeTypeID)
      .FirstOrDefault(r => r.CodeID == codeID);

    if (parameter == null) return;

    parameter.CodeDesc = longDesc;
    Db.SaveChanges();
}
catch (Exception ex)
{
    output = $"{output}\nfailed for id '{codeID}' holding value '{longDesc}': {ex.Message}";
}