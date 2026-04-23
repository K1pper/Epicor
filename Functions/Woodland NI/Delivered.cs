
var deliveries = Db.UD101.Where(r => r.Company == Session.CompanyID).ToList();

var delivery = Db.UD101.Where(r => r.Company == Session.CompanyID).Where(r => r.Number01 == PackNum).FirstOrDefault();

if (delivery == null)
{
    Ice.Diagnostics.Log.WriteEntry($"Pack number {PackNum} not found");
}
else
{
    if (delivery.ShortChar02 == "REA")
    {
        var pallets = Db.UD101A.Where(r => r.Company == Session.CompanyID).Where(r => r.Number01 == PackNum).ToList();

        if (pallets == null) return;

        if (Delivered)
        {
            foreach (var pallet in pallets)
            {
                this.CallService<Erp.Contracts.CustShipSvcContract>(custShipSvc =>
                {
                    try
                    {
                        var custShipTs = new Erp.Tablesets.CustShipTableset();
                        custShipTs = custShipSvc.GetByID((int)pallet.Number02);

                        var shipHeadEntry = BufferCopy.Copy<Erp.Tablesets.ShipHeadRow>(custShipTs.ShipHead[0]);
                        shipHeadEntry.RowMod = "";
                        custShipTs.ShipHead.Add(shipHeadEntry);

                        custShipTs.ShipHead[0].ReadyToInvoice = true;
                        custShipTs.ShipHead[0].ShipStatus = "SHIPPED";
                        custShipTs.ShipHead[0].RowMod = "U";


                        custShipSvc.Update(ref custShipTs);
                    }
                    catch (Exception ex)
                    {
                        Ice.Diagnostics.Log.WriteEntry($"Delivered error: {ex.Message}");
                        Ice.Diagnostics.Log.WriteEntry($"Delivered error: {ex.ToString()}");
                        if (ex.InnerException != null)
                            Ice.Diagnostics.Log.WriteEntry($"Delivered error inner exception: {ex.InnerException.Message}");
                    }
                });
            }
        }
    }


}