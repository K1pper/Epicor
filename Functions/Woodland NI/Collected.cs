/* Move the stock from despatch wrehouse to react hub */

ErrorMessage = "";

var shipVia = "REA";
var shipViaDesc = "React Transport";
var shipWarehouse = "RE";
var shipBin = "RE";
var defWarehouse = "DE";
var defBin = "DE";

var collection = Db.UD101.Where(r => r.Company == Session.CompanyID).Where(r => r.Number01 == PackNum).FirstOrDefault();

if (collection.ShortChar02 == "REA")
{
    var pallets = Db.UD101A.Where(r => r.Company == Session.CompanyID).Where(r => r.Number01 == PackNum).ToList();
    foreach (var pallet in pallets)
    {
        var shipments = Db.ShipDtl.Where(r => r.Company == Session.CompanyID).Where(r => r.PackNum == pallet.Number02).ToList();
        foreach (var shipment in shipments)
        {
            var partWhse = Db.PartWhse.Where(r => r.Company == Session.CompanyID).Where(r => r.PartNum == shipment.PartNum).Where(r => r.WarehouseCode == shipWarehouse).FirstOrDefault();
            if (partWhse == null) continue;
            if (shipment.OurInventoryShipQty <= 0) continue;

            this.CallService<Erp.Contracts.InvTransferSvcContract>(invTransferSvc =>
            {
                try
                {
                    var invTransferTs = new Erp.Tablesets.InvTransferTableset();
                    invTransferSvc.GetNewInventoryTransfer("STK-STK", ref invTransferTs);

                    invTransferTs.InvTrans[0].PartNum = shipment.PartNum;
                    invTransferTs.InvTrans[0].TranDate = DateTime.Now;
                    invTransferTs.InvTrans[0].TranReference = $"Auto move for {shipViaDesc} shipping";
                    invTransferTs.InvTrans[0].FromWarehouseCode = Collected ? defWarehouse : shipWarehouse;
                    invTransferTs.InvTrans[0].ToWarehouseCode = Collected ? shipWarehouse : defWarehouse;
                    invTransferTs.InvTrans[0].FromBinNum = Collected ? defBin : shipBin;
                    invTransferTs.InvTrans[0].ToBinNum = Collected ? shipBin : defBin;
                    invTransferTs.InvTrans[0].Plant = shipment.Plant;
                    invTransferTs.InvTrans[0].Plant2 = shipment.Plant;
                    invTransferTs.InvTrans[0].FromPlant = shipment.Plant;
                    invTransferTs.InvTrans[0].ToPlant = shipment.Plant;
                    invTransferTs.InvTrans[0].FromOnHandUOM = shipment.IUM;
                    invTransferTs.InvTrans[0].TransferQty = shipment.OurInventoryShipQty;
                    invTransferTs.InvTrans[0].TransferQtyUOM = shipment.IUM;
                    invTransferTs.InvTrans[0].ToOnHandUOM = shipment.IUM;
                    invTransferTs.InvTrans[0].TrackingUOM = shipment.IUM;
                    invTransferTs.InvTrans[0].RowMod = "U";

                    string legalNumberMessage = "";
                    string partTranPKs = "";

                    invTransferSvc.CommitTransfer(ref invTransferTs, out legalNumberMessage, out partTranPKs);

                    shipment.WarehouseCode = Collected ? shipWarehouse : defWarehouse;
                    shipment.BinNum = Collected ? shipBin : defBin;
                }
                catch (Exception ex)
                {
                    if (ex.Message == "There is not enough unallocated inventory in this bin for this transaction.")
                    {
                        ErrorMessage = $"Part Number {shipment.PartNum} on pack {shipment.PackNum}, {ex.Message}";
                    }

                    Ice.Diagnostics.Log.WriteEntry($"Collected error: {ex.Message}");
                    if (ex.InnerException != null)
                        Ice.Diagnostics.Log.WriteEntry($"Collected error inner exception: {ex.InnerException.Message}");
                }
            });

            this.CallService<Erp.Contracts.CustShipSvcContract>(custShipSvc =>
            {
                try
                {
                    var custShipTs = new Erp.Tablesets.CustShipTableset();
                    custShipSvc.GetByID(shipment.PackNum);

                    var shipDtl = custShipTs.ShipDtl.Where(r => r.PackLine == shipment.PackLine).FirstOrDefault();
                    if (shipDtl == null) return;

                    shipDtl.WarehouseCode = Collected ? shipWarehouse : defWarehouse;
                    shipDtl.BinNum = Collected ? shipBin : defBin;
                    shipDtl.RowMod = "U";

                    custShipSvc.Update(ref custShipTs);
                }
                catch (Exception ex)
                {
                    Ice.Diagnostics.Log.WriteEntry($"Change shipment warehouse error: {ex.Message}");
                    if (ex.InnerException != null)
                        Ice.Diagnostics.Log.WriteEntry($"Change shipment warehouse error inner exception: {ex.InnerException.Message}");
                }
            });
            ThisLib.PrintLabels(PackNum);
        }
    }

}