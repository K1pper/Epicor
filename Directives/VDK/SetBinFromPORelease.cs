foreach(var ttRcvDtl in ds.RcvDtl.Where(x => x.Added() || x.Updated()))
{
  if(ttRcvDtl.TranType.Equals("PUR-STK", StringComparison.OrdinalIgnoreCase))
  {
    var PORel = (from row in Db.PORel where row.Company == ttRcvDtl.Company && row.PONum == ttRcvDtl.PONum && row.POLine == ttRcvDtl.POLine && row.PORelNum == ttRcvDtl.PORelNum select row).FirstOrDefault();
    
    if (PORel != null && !string.IsNullOrEmpty(PORel.Bin_c))
    {
      ttRcvDtl.BinNum = PORel.Bin_c;
    }
  }
}