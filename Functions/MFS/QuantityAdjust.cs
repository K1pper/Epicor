  this.CallService<Erp.Contracts.InventoryQtyAdjSvcContract>(qtyAdjSvc => {
  try
  {
    var qtyAdj = qtyAdjSvc.GetInventoryQtyAdj(PartNum, UOM);
    
    qtyAdj.InventoryQtyAdj[0].WareHseCode = WareHseCode;
    qtyAdj.InventoryQtyAdj[0].BinNum = BinNum;
    qtyAdj.InventoryQtyAdj[0].LotNum = LotNum;
    qtyAdj.InventoryQtyAdj[0].ReasonCode = ReasonCode;
    qtyAdj.InventoryQtyAdj[0].AdjustQuantity = AdjustQuantity;
    qtyAdj.InventoryQtyAdj[0].RowMod = "U";
    
    qtyAdjSvc.SetInventoryQtyAdj(ref qtyAdj, out string partTranPKs);
  }
  catch(Exception ex) {
    Ice.Diagnostics.Log.WriteEntry(ex.Message);
  }
});
