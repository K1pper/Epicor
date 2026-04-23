var shipDtl = ds.ShipDtl.Where(r => r.Added() || r.Updated()).FirstOrDefault();

if (shipDtl == null) return;

if (shipDtl.OurInventoryShipQty > shipDtl.OurReqQty - shipDtl.OurShippedQty)
{
  throw new BLException("You are trying to ship more than is available on the order");  
}