
////////////////////////////////////////////////////////////////////////
// Mandata Shipping Functions
////////////////////////////////////////////////////////////////////////
// Function to obtan POD info and ship goods on delivery
// Version 1.0
// 
// Written by Peter Roden - GHA Solutions - 24/04/2026
////////////////////////////////////////////////////////////////////////

Ice.Diagnostics.Log.WriteEntry("DELIVERIES: Function started");

var fn = "Deliveries";
System.Text.StringBuilder debug = new System.Text.StringBuilder();
var packOpen = true;

this.EfxLib.GHA.Logger("Deliveries Function called", fn, false);

var deliveries = Db.UD101.Where(r => r.Company == Session.CompanyID).ToList();

var delivery = Db.UD101.Where(r => r.Company == Session.CompanyID).Where(r => r.Number01 == PackNum).FirstOrDefault();

if (delivery == null)
{
    Ice.Diagnostics.Log.WriteEntry($"Pack number {PackNum} not found");
}
else
{
    //if (delivery.ShortChar02 == "REA")
    {
        var pallets = Db.UD101A.Where(r => r.Company == Session.CompanyID).Where(r => r.Number01 == PackNum).ToList();
        if (pallets == null) return;


        Ice.Diagnostics.Log.WriteEntry(
            $"DELIVERIES TEST: " +
            $"PackNum={PackNum}, " +
            $"Delivered={Delivered}, " +
            $"DeliveryFound={delivery != null}, " +
            $"PalletCount={pallets.Count}, " +
            $"PalletShipments=[{string.Join(",", pallets.Select(p => p.Number02))}]"
        );

        if (Delivered)
        {
            foreach (var pallet in pallets)
            {

                try
                {
                    var custShipTs = new Erp.Tablesets.CustShipTableset();
                    this.CallService<Erp.Contracts.CustShipSvcContract>(custShipSvc =>
                    {

                        custShipTs = custShipSvc.GetByID((int)pallet.Number02);
                        if (custShipTs.ShipHead[0].ShipStatus != "OPEN")
                        {
                            Ice.Diagnostics.Log.WriteEntry($"Pack number {PackNum} is not open");
                            packOpen = false;
                        }

                        Ice.Diagnostics.Log.WriteEntry($"Pack num {custShipTs.ShipHead[0].PackNum}");

                        var detailsChanged = false;

                        if (packOpen)
                        {
                            foreach (var shipDtl in custShipTs.ShipDtl)
                            {
                                Ice.Diagnostics.Log.WriteEntry($"Pack line {shipDtl.PackLine}, Order num {shipDtl.OrderNum}, order Line {shipDtl.OrderLine}");

                                var shippedSoFar = 0m;

                                var orderDtl = Db.OrderDtl
                                    .Where(r => r.Company == shipDtl.Company)
                                    .Where(r => r.OrderNum == shipDtl.OrderNum)
                                    .Where(r => r.OrderLine == shipDtl.OrderLine)
                                    .FirstOrDefault();

                                var orderRels = Db.OrderRel
                                    .Where(r => r.Company == shipDtl.Company)
                                    .Where(r => r.OrderNum == shipDtl.OrderNum)
                                    .Where(r => r.OrderLine == shipDtl.OrderLine)
                                    .ToList();

                                var reqQty = orderDtl == null ? 0 : orderDtl.OrderQty;

                                Ice.Diagnostics.Log.WriteEntry($"Required {reqQty}");


                                foreach (var orderRel in orderRels)
                                {
                                    shippedSoFar += orderRel.OurJobShippedQty + orderRel.OurStockShippedQty;
                                    Ice.Diagnostics.Log.WriteEntry($"Shipped on order release {orderRel.OrderRelNum} {orderRel.OurJobShippedQty + orderRel.OurStockShippedQty}");
                                }

                                Ice.Diagnostics.Log.WriteEntry($"Shipped so far {shippedSoFar}");

                                Ice.Diagnostics.Log.WriteEntry($"Checking shipped so far ({shippedSoFar}) + shipping on this line ({shipDtl.OurJobShipQty + shipDtl.OurShippedQty}) is greater or equal to required quantity {reqQty}");


                                if (shippedSoFar + shipDtl.OurJobShipQty + shipDtl.OurShippedQty >= reqQty)
                                {
                                    Ice.Diagnostics.Log.WriteEntry($"Setting ShipCmpl on pack {PackNum}, {shipDtl.PackLine}, order {shipDtl.OrderNum}, line {shipDtl.OrderLine}");

                                    shipDtl.ShipCmpl = true;
                                    shipDtl.RowMod = "U";
                                    detailsChanged = true;
                                }
                            }

                            if (detailsChanged)
                            {
                                Ice.Diagnostics.Log.WriteEntry($"Updating detail rows for pack {PackNum}");
                                custShipSvc.Update(ref custShipTs);
                                Ice.Diagnostics.Log.WriteEntry($"Updated detail rows for pack {PackNum}");
                            }
                        }
                    });

                    if (packOpen)
                    {
                        this.CallService<Erp.Contracts.CustShipSvcContract>(custShipSvc =>
                        {


                            custShipTs = custShipSvc.GetByID((int)pallet.Number02);
                            var shipHeadEntry = BufferCopy.Copy<Erp.Tablesets.ShipHeadRow>(custShipTs.ShipHead[0]);
                            shipHeadEntry.RowMod = "";

                            custShipTs.ShipHead[0].ShipDate = DateTime.Today;
                            custShipTs.ShipHead[0].RowMod = "U";

                            custShipTs.ShipHead.Add(shipHeadEntry);

                            Ice.Diagnostics.Log.WriteEntry($"Updating Ship Date {(int)pallet.Number02}");
                            custShipSvc.Update(ref custShipTs);
                            Ice.Diagnostics.Log.WriteEntry($"Updated Ship Date");
                        });


                        this.CallService<Erp.Contracts.CustShipSvcContract>(custShipSvc =>
                        {
                            custShipTs = custShipSvc.GetByID((int)pallet.Number02);
                            var shipHeadEntry = BufferCopy.Copy<Erp.Tablesets.ShipHeadRow>(custShipTs.ShipHead[0]);
                            shipHeadEntry.RowMod = "";

                            custShipTs.ShipHead[0].ShipPerson = Session.UserID;
                            custShipTs.ShipHead[0].ReadyToInvoice = true;
                            custShipTs.ShipHead[0].ShipStatus = "SHIPPED";
                            custShipTs.ShipHead[0].RowMod = "U";

                            custShipTs.ShipHead.Add(shipHeadEntry);
                            Ice.Diagnostics.Log.WriteEntry($"Updating Ship person and ready to invoice");
                            custShipSvc.Update(ref custShipTs);
                            Ice.Diagnostics.Log.WriteEntry($"Updated Ship person  and ready to invoice");
                        });
                    }
                }
                catch (Exception ex)
                {

                    Ice.Diagnostics.Log.WriteEntry($"Delivered error: {ex.ToString()}");
                    if (ex.InnerException != null)
                        Ice.Diagnostics.Log.WriteEntry($"Delivered error inner exception: {ex.InnerException.Message}");
                }
            }
        }
        this.EfxLib.GHA.Logger(debug.ToString(), fn, false);
    }
}

Ice.Diagnostics.Log.WriteEntry("DELIVERIES: Reached end of function");
Ice.Diagnostics.Log.WriteEntry("DELIVERIES DEBUG:\r\n" + debug.ToString());
