//set the type field on the ecomtl from the new part
//see also dd on ecomtl to set type on check out / as a catch all
foreach( var ttEM in ds.ECOMtl.Where(x => (x.Added() || x.Updated()) && !string.IsNullOrEmpty(x.MtlPartNum)))
{
  if(ttEM.Updated())
  {
    var bitt = ttECOMtl.Where(x => x.SysRowID == ttEM.SysRowID && x.Unchanged()).FirstOrDefault();
    
    if(bitt == null || bitt.MtlPartNum.Equals(ttEM.MtlPartNum, StringComparison.OrdinalIgnoreCase)) continue;
  }
  
  var part = Erp.Tables.Part.FindFirstByPrimaryKey(Db, ttEM.Company, ttEM.MtlPartNum);
  
  if(part != null)  
  {
    ttEM["Type_c"] = part.Type_c;
  }
}