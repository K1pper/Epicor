
/* No longer required, here because I wrote and teted the code so may serve as an example */

Message = string.Empty;

var Company = Session.CompanyID; 

try
{
  this.CallService<Erp.Contracts.OrderAllocSvcContract>(alloc =>
  {
  
    var ts = new Erp.Tablesets.OrderAllocTableset();
  
    var orderAlloc = new Erp.Tablesets.OrderAllocRow();
    orderAlloc.Company = Company;
    orderAlloc.JobNum = "";
    orderAlloc.AssemblySeq = 0;
    orderAlloc.DemandType = "Order";
    orderAlloc.MtlSeq = 0;
    orderAlloc.OrderLine = OrderLine;
    orderAlloc.OrderNum = OrderNum;
    orderAlloc.OrderRelNum = OrderRelNum;
    orderAlloc.LineType = "PART";
    orderAlloc.ReservePriorityOverride = 0;
    orderAlloc.TFOrdLine = 0;
    orderAlloc.TFOrdNum = "";
    orderAlloc.EnableSelectForPicking = true;
    orderAlloc.SelectedForAction = true;
    orderAlloc.SelectForPicking = false;
    
    var orderRel = Db.OrderRel
      .Where(r => r.Company == Company)
      .Where(r => r.OrderNum == OrderNum)
      .Where(r => r.OrderLine == OrderLine)
      .Where(r => r.OrderRelNum == OrderRelNum)
      .FirstOrDefault();
    var qtyReq = orderRel == null ? 0 : orderRel.OurReqQty - orderRel.OurJobShippedQty - orderRel.OurStockShippedQty;
    
    var partAlloc = Db.PartAlloc
      .Where(r => r.Company == Company)
      .Where(r => r.OrderNum == OrderNum)
      .Where(r => r.OrderLine == OrderLine)
      .Where(r => r.OrderRelNum == OrderRelNum)
      .FirstOrDefault();
    var qtyAlloc = partAlloc == null ? 0 : partAlloc.ReservedQty + partAlloc.AllocatedQty + partAlloc.PickingQty + partAlloc.PickedQty;

    var partPlant = Db.PartPlant.Where(r => r.Company == Session.CompanyID).Where(r => r.PartNum == orderRel.PartNum).FirstOrDefault();

    var primWhse = partPlant == null ? "" : partPlant.PrimWhse;
    
    orderAlloc.WarehouseCode = primWhse;
    
    orderAlloc.PartNum = orderRel.PartNum;
    orderAlloc.RevisionNum = orderRel.RevisionNum;
    orderAlloc.Plant = orderRel.Plant;
    orderAlloc.IUM = orderRel.IUM;
    orderAlloc.SalesUM = orderRel.SalesUM;
    orderAlloc.ReqDate = orderRel.ReqDate;
    
    orderAlloc.FulfilledQty = qtyReq - qtyAlloc;
    orderAlloc.RequiredQty = qtyReq - qtyAlloc;
    orderAlloc.OurReqQty = qtyReq - qtyAlloc; 
    orderAlloc.ReservedQty = qtyReq - qtyAlloc; 
    orderAlloc.RowMod = "U";
    
    ts.OrderAlloc.Add(orderAlloc);
    
    var partAllocTran = new Erp.Tablesets.PartAllocTranRow();
    
    partAllocTran.Company = Company;
    partAllocTran.DcdUserID = Session.UserID;
    partAllocTran.AllocType = "Order";
    partAllocTran.DemandType = "Order";
    partAllocTran.BinType = "Standard";
    partAllocTran.AddHoc = true;
    partAllocTran.RowMod = "U";
    
    ts.PartAllocTran.Add(partAllocTran);
    
    bool lReleased;
    
    Ice.Diagnostics.Log.WriteEntry(JsonConvert.SerializeObject(ts));
    
    alloc.AutoAllocation(ref ts, "", "All", out Message, out lReleased);
  });
}
catch (Exception ex)
{
Message = ex.ToString();
}