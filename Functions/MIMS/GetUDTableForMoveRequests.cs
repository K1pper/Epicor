udTable = string.Empty;

var tableUDCode = (from row in Db.UDCodes where row.Company == Session.CompanyID && row.CodeTypeID == "GHA_MIMS" && row.CodeID == "MatQueueTb" select row).FirstOrDefault();
  
if(tableUDCode != null)
{
  udTable = tableUDCode.CodeDesc;
}

if(string.IsNullOrEmpty(udTable)) throw new BLException("A UD table must be chosen for the process, review system parameters");

var zdataTable = (from row in Db.ZDataTable where row.SystemCode == "Ice" && row.DataTableID == udTable select row).FirstOrDefault();

if(zdataTable == null) throw new BLException(string.Format("Table {0} could not be found, please check system parameters", udTable));