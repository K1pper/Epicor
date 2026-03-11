// Go to Plant table and get GHA_MFS_UserCodeID_c field for the current plant
// Go to the UD code table & get default labour entry method and time entry method for user code ID
// Get current plant 

timeEntryMethodValue = "RT";
labourEntryMethodValue = "RT";

var site = this.Db.Plant.Where(r => r.Company == this.Session.CompanyID).Where(r => r.Plant1 == this.Session.PlantID).FirstOrDefault();

if (site == null) {
  return;
}

var userCodeType = site.UDField<string>("GHA_MFS_UserCodeType_c");

if (string.IsNullOrEmpty(userCodeType)) {
  return;
}

var labourEntryMethod = this.Db.UDCodes
    .Where(r => r.Company == this.Session.CompanyID)
    .Where(r => r.CodeTypeID == site.GHA_MFS_UserCodeType_c)
    .Where(r => r.CodeID.ToLower() == "DTEM".ToLower())
    .FirstOrDefault();

var timeEntryMethod = this.Db.UDCodes
    .Where(r => r.Company == this.Session.CompanyID)
    .Where(r => r.CodeTypeID == site.GHA_MFS_UserCodeType_c)
    .Where(r => r.CodeID.ToLower() == "DTrvTim".ToLower())
    .FirstOrDefault();

if (timeEntryMethod != null) {
  timeEntryMethodValue = timeEntryMethod.CodeDesc;
}

if (labourEntryMethod != null) {
  labourEntryMethodValue = labourEntryMethod.CodeDesc;
}