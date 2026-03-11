
this.CallService<Erp.Contracts.PartSvcContract>(partSvc => {
  try
  {
    var part = partSvc.GetByID(PartNum);
    
    var plantExists = part.PartPlant.Where(r => r.Plant.ToUpper() == Plant.ToUpper()).Any();
    
    if (!plantExists)
    {
      partSvc.GetNewPartPlant(ref part, PartNum);
      var x = part.PartPlant.Where(r => r.Added()).FirstOrDefault();
      x.Plant = Plant;
      x.PrimWhse = WarehouseCode;
    } 

    var warehouseExists = part.PartWhse.Where(r => r.WarehouseCode.ToUpper() == WarehouseCode.ToUpper()).Any();
    
    if (!warehouseExists)
    {
      partSvc.GetNewPartWhse(ref part, PartNum, Plant);
      var y = part.PartWhse.Where(r => r.Added()).FirstOrDefault();
      y.WarehouseCode = WarehouseCode;
      var bin = Db.WhseBin.Where(r => r.Company == this.callContextClient.CurrentCompany).Where(r => r.WarehouseCode == y.WarehouseCode).OrderBy(r => r.BinNum).FirstOrDefault();
      if (bin != null)
      {
        y.PrimBinNum = bin.BinNum;
      }

      partSvc.Update(ref part);
    }

  }
  catch(Exception ex) {
    Ice.Diagnostics.Log.WriteEntry(ex.Message);
  }
});

