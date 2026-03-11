var ttDMRA = ds.DMRActn.FirstOrDefault();

if(ttDMRA != null)
{
  var ttDMRH = ds.DMRHead.FirstOrDefault();
  
  if(ttDMRH != null && !ttDMRH.OpenDMR)
  {
    using(var scope = Erp.ErpContext.CreateDefaultTransactionScope())
    {
      var dmrHead = Erp.Tables.DMRHead.FindFirstBySysRowIDWithUpdLock(Db, ttDMRH.SysRowID);
      
      if(dmrHead != null)
      {
        dmrHead.OpenDMR = true;
        
        Db.Validate(dmrHead);
        
        BufferCopy.Copy(dmrHead, ttDMRH);
      }
    
      scope.Complete();
    }
  }
}