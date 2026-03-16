success = false;

try
{
    var isCodeTypeId = Db.UDCodeType
      .Where(r => r.Company == this.Session.CompanyID)
      .Where(r => r.CodeTypeID == CodeTypeID)
      .Any();

    if (!isCodeTypeId)
    {
        this.CallService<Ice.Contracts.UserCodesSvcContract>(userCodesSvc =>
        {
            var userCodesTs = new UserCodesTableset();
            userCodesSvc.GetNewUDCodeType(ref userCodesTs);
            userCodesTs.UDCodeType[0].CodeTypeID = CodeTypeID;
            userCodesTs.UDCodeType[0].CodeTypeDesc = CodeTypeDesc;
            userCodesSvc.Update(ref userCodesTs);
        });
    }
    success = true;
}
catch (Exception ex)
{
    success = false;
}
