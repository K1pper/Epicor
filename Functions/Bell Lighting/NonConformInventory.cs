////////////////////////////////////////////////////////////////////////
// GHA 358 Good In Inspection
////////////////////////////////////////////////////////////////////////
// Function to perform inspection processing on container receipt
// Version 1.0
// 
// Written by Peter Roden - GHA Solutions - 17/06/2026
////////////////////////////////////////////////////////////////////////

var func = "ReceiveContainerToStock";

ThisLib.Logger("NON CONFORM INVENTORY", func);

var partTran = Db.PartTran
  .Where(r => r.PartNum == PartNum)
  .Where(r => r.SysDate == SysDate)
  .Where(r => r.SysTime == SysTime)
  .Where(r => r.PONum == PONum)
  .Where(r => r.POLine == POLine)
  .Where(r => r.PORelNum == PORelNum)
  .FirstOrDefault();

if (partTran == null) return;

if (partTran.TranType != "PUR-STK") return;

if (Db.ContainerDetail
      .Where(r => r.Company == Company)
      .Where(r => r.PONum == partTran.PONum)
      .Where(r => r.POLine == partTran.POLine)
      .Where(r => r.PORelNum == partTran.PORelNum)
      .Any())
{

    this.CallService<Erp.Contracts.NonConfSvcContract>(nonConformSvc =>
    {
        try
        {
            var nonConformDs = new Erp.Tablesets.NonConfTableset();
            nonConformSvc.GetNewNonConf(ref nonConformDs);

            var employee = Db.UserComp.Where(r => r.Company == Company).Where(r => r.DcdUserID == Session.UserID).FirstOrDefault();
            if (employee == null) return;

            var udCodes = Db.UDCodes.Where(r => r.Company == Company).Where(r => r.CodeTypeID == "GoodsIn").Where(r => r.CodeID == "DefNRC").FirstOrDefault();
            if (udCodes == null) return;

            var currentPlant = Db.PlantConfCtrl.Where(r => r.Company == Company).Where(r => r.Plant == partTran.Plant).FirstOrDefault();
            if (currentPlant == null)
            {
                Ice.Diagnostics.Log.WriteEntry("Current Plant not found");
                return;
            }
            var inspWarehouseCode = currentPlant.DefInspWhse;
            var inspBinNum = currentPlant.DefInspBin;

            nonConformDs.NonConf[0].EmpID = employee.EmpID;
            nonConformDs.NonConf[0].PartNum = PartNum;
            nonConformDs.NonConf[0].TranUOM = partTran.UM;
            nonConformDs.NonConf[0].ScrapUM = partTran.UM;
            nonConformDs.NonConf[0].Quantity = partTran.TranQty;
            nonConformDs.NonConf[0].WarehouseCode = partTran.WareHouseCode;
            nonConformDs.NonConf[0].BinNum = partTran.BinNum;
            nonConformDs.NonConf[0].ToWarehouseCode = inspWarehouseCode;
            nonConformDs.NonConf[0].ToBinNum = inspBinNum;
            nonConformDs.NonConf[0].ReasonCode = udCodes.CodeDesc;
            nonConformDs.NonConf[0].TrnTyp = "I";

            nonConformSvc.Update(ref nonConformDs);

            DMRNum = ThisLib.FailInspectInventory(Company, partTran.Plant, nonConformDs.NonConf[0].TranID, partTran.TranQty);
        }
        catch (Exception ex)
        {
            ThisLib.Logger($"Non confomance exception {ex}", func);
        }
    });
}