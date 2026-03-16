string codeTypeId = string.Empty;

try
{
    var plant = Db.Plant
      .Where(r => r.Company == this.callContextClient.CurrentCompany)
      .Where(r => r.Plant1 == this.callContextClient.CurrentPlant)
      .FirstOrDefault();

    if (plant == null) return;

    codeTypeId = plant.UDField<string>("GHA_MFS_UserCodeType_c");

    UDCodes parameter = Db.UDCodes
      .Where(r => r.Company == this.Session.CompanyID)
      .Where(r => r.CodeTypeID == codeTypeId)
      .FirstOrDefault(r => r.CodeID == codeID);

    if (parameter == null) return;

    parameter.IsActive = isActive;
    Db.SaveChanges();
}
catch (Exception ex)
{
    output = $"Failed to set active flag to: '{isActive}' for code ID: '{codeID}' on type: '{codeTypeId}'.\n\n{ex.Message}";
}