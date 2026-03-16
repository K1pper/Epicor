try
{
    var plant = Db.Plant
      .Where(r => r.Company == this.callContextClient.CurrentCompany)
      .Where(r => r.Plant1 == this.callContextClient.CurrentPlant)
      .FirstOrDefault();

    if (plant == null) return;

    var codeTypeId = plant.UDField<string>("GHA_MFS_UserCodeType_c");

    UDCodes parameter = Db.UDCodes
      .Where(r => r.Company == this.Session.CompanyID)
      .Where(r => r.CodeTypeID == codeTypeId)
      .FirstOrDefault(r => r.CodeID == codeID);

    if (parameter == null) return;

    parameter.CodeDesc = codeDesc;
    Db.SaveChanges();
}
catch (Exception ex)
{
    output = $"{output}\nfailed for id '{codeID}' holding value '{codeDesc}': {ex.Message}";
}