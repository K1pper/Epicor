
/* No longer required, here because I wrote and teted the code so may serve as an example */

Message = string.Empty;

var Company = Session.CompanyID; 

this.CallService<Erp.Contracts.OrderAllocSvcContract>(alloc =>
{

  var ts = new Erp.Tablesets.OrderAllocTableset();

   var orderAlloc = new Erp.Tablesets.OrderAllocRow();
    orderAlloc.Company = Company;
    orderAlloc.JobNum = JobNum;
    orderAlloc.AssemblySeq = AssemblySeq;
    orderAlloc.DemandType = "Job";
    orderAlloc.MtlSeq = MtlSeq;
    orderAlloc.OrderLine = 0;
    orderAlloc.OrderNum = 0;
    orderAlloc.OrderRelNum = 0;
    orderAlloc.ReservePriorityOverride = 0;
    orderAlloc.TFOrdLine = 0;
    orderAlloc.TFOrdNum = "";
    orderAlloc.EnableSelectForPicking = true;
    orderAlloc.SelectedForAction = true;
    
    var jobMtl = Db.JobMtl
      .Where(r => r.Company == Company)
      .Where(r => r.JobNum == JobNum)
      .Where(r => r.AssemblySeq == AssemblySeq)
      .Where(r => r.MtlSeq == MtlSeq)
      .FirstOrDefault();
    var qtyReq = jobMtl == null ? 0 : jobMtl.RequiredQty;
    
    var partAlloc = Db.PartAlloc
      .Where(r => r.Company == Company)
      .Where(r => r.JobNum == JobNum)
      .Where(r => r.AssemblySeq == AssemblySeq)
      .Where(r => r.MtlSeq == MtlSeq)
      .FirstOrDefault();
    var qtyAlloc = partAlloc == null ? 0 : partAlloc.ReservedQty + partAlloc.AllocatedQty + partAlloc.PickingQty + partAlloc.PickedQty;

    var partPlant = Db.PartPlant.Where(r => r.Company == Session.CompanyID).Where(r => r.PartNum == jobMtl.PartNum).FirstOrDefault();

    var primWhse = partPlant == null ? "" : partPlant.PrimWhse;
    
    orderAlloc.WarehouseCode = primWhse;

    orderAlloc.FulfilledQty = qtyReq - qtyAlloc;
    orderAlloc.RequiredQty = qtyReq - qtyAlloc;
    orderAlloc.OurReqQty = qtyReq - qtyAlloc; 
    orderAlloc.ReservedQty = qtyReq - qtyAlloc; 
    orderAlloc.RowMod = "U";
    
    ts.OrderAlloc.Add(orderAlloc);
    
    var partAllocTran = new Erp.Tablesets.PartAllocTranRow();
    
    partAllocTran.Company = Company;
    partAllocTran.DcdUserID = Session.UserID;
    partAllocTran.AllocType = "Job";
    partAllocTran.DemandType = "Job";
    partAllocTran.BinType = "Standard";
    partAllocTran.AddHoc = true;
    partAllocTran.RowMod = "U";
    
    ts.PartAllocTran.Add(partAllocTran);
    
    bool lReleased;
    alloc.AutoAllocation(ref ts, "", "All", out Message, out lReleased);
});