foreach(var ttRcvDtl in ds.RcvDtl.Where(x => x.Added()))
{
  var poDetail = Erp.Tables.PODetail.FindFirstByPrimaryKey(Db, ttRcvDtl.Company, ttRcvDtl.PONum, ttRcvDtl.POLine);
  
  if(poDetail != null)
  {
    ttRcvDtl["EmpID_c"] = poDetail.EmpID_c;
    ttRcvDtl["EmpName_c"] = poDetail.EmpName_c;
  }
}