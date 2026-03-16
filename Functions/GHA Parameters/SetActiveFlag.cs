try
{
    var codeData = Db.UDCodes.Where(r => r.CodeTypeID == codeTypeID)
                             .FirstOrDefault(r => r.CodeID == codeID);
    if (codeData == null) return;

    codeData.IsActive = isActive;

    Db.SaveChanges();

}
catch (Exception ex)
{
    output = $"Failed to set active flag to: '{isActive}' for code ID: '{codeID}' on type: '{codeTypeID}'.\n\n{ex.Message}";
}