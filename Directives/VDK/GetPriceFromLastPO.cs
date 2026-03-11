foreach(var ttPD in ds.PODetail.Where(x => x.Added() || x.Updated() && !string.IsNullOrEmpty(x.PartNum) && x.DocScrUnitCost == Decimal.Zero))
{
  PODetailRow bitt = null;
  if(ttPD.Updated())
  {
    bitt = ds.PODetail.Where(x => x.SysRowID == ttPD.SysRowID && x.Unchanged()).FirstOrDefault();
    
    if(bitt != null && bitt.PartNum.Equals(ttPD.PartNum, StringComparison.OrdinalIgnoreCase)) continue;
  }
  
  var poHeader = Erp.Tables.POHeader.FindFirstByPrimaryKey(Db, ttPD.Company, ttPD.PONUM);
  
  if(poHeader != null)
  {
    //find the last purchase for the part / pum / vendor / currency
    var lastPODetail = (from row in Db.PODetail join ph in Db.POHeader on new {p1 = row.Company, p2 = row.PONUM} equals new {p1 = ph.Company, p2 = ph.PONum} where row.Company == ttPD.Company && row.PartNum == ttPD.PartNum && ph.VendorNum == poHeader.VendorNum && ph.PurPoint == poHeader.PurPoint && ph.CurrencyCode == poHeader.CurrencyCode && row.PUM == ttPD.PUM orderby ph.PONum descending, row.POLine descending select row).FirstOrDefault();
    
    if(lastPODetail != null)
    {
        if (ttPD.CurrencySwitch)
        {
            ttPD.ScrUnitCost = lastPODetail.UnitCost;
        }
        else
        {
            ttPD.DocScrUnitCost = lastPODetail.DocUnitCost;
        }
        
        var updTS = new Erp.Tablesets.POTableset();
        if(ttPD.Updated())
        {
          var updbi = new Erp.Tablesets.PODetailRow();
          BufferCopy.Copy(bitt, updbi);
          updTS.PODetail.Add(updbi);
        }
        
        var updtt = new Erp.Tablesets.PODetailRow();
        BufferCopy.Copy(ttPD, updtt);
        updTS.PODetail.Add(updtt);
        
        
        using(var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.POSvcContract>(Db))
        {
          svc.ChangeUnitPrice(ref updTS);
          
          BufferCopy.Copy(updtt, ttPD);
        }
        
        if(!lastPODetail.CostPerCode.Equals(ttPD.CostPerCode, StringComparison.OrdinalIgnoreCase))
        {
          using(var svc = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.POSvcContract>(Db))
          {
            svc.ChangeDetailCostPerCode(lastPODetail.CostPerCode, ref updTS);
            
            BufferCopy.Copy(updtt, ttPD);
          }
        }
    }
  }
}
