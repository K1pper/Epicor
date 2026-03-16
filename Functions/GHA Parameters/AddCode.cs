success = false;

try
{
    var isCodeId = Db.UDCodes
      .Where(r => r.Company == this.Session.CompanyID)
      .Where(r => r.CodeTypeID == CodeTypeID)
      .Where(r => r.CodeID == CodeID)
      .Any();

    if (!isCodeId)
    {
        this.CallService<Ice.Contracts.UserCodesSvcContract>(userCodesSvc =>
        {
            var userCodesTs = userCodesSvc.GetByID(CodeTypeID);
            userCodesSvc.GetNewUDCodes(ref userCodesTs, CodeTypeID);
            var added = userCodesTs.UDCodes.Where(r => r.Added()).FirstOrDefault();
            added.CodeID = CodeID;
            added.CodeDesc = Description;
            added.LongDesc = LongDescription;
            added.IsActive = Active;

            userCodesSvc.Update(ref userCodesTs);
        });
    }
    success = true;
}
catch (Exception ex)
{
    Ice.Diagnostics.Log.WriteEntry($"ERROR GHA Parameters: AddCode - {ex.Message}");
    if (ex.InnerException != null)
    {
        Ice.Diagnostics.Log.WriteEntry($"ERROR GHA Parameters: AddCode - {ex.InnerException.Message}");
    }
    success = false;
}