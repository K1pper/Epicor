foreach (var ShipDtlRow in ttShipDtl.Where(a => a.RowMod == "A" || a.RowMod == "U"))
{
    var OrderRelRec = (from OrderRel_Row in Db.OrderRel
                       where OrderRel_Row.Company == Session.CompanyID
                             && OrderRel_Row.OrderNum == ShipDtlRow.OrderNum
                             && OrderRel_Row.OrderLine == ShipDtlRow.OrderLine
                             && OrderRel_Row.OrderRelNum == ShipDtlRow.OrderRelNum
                       select OrderRel_Row).FirstOrDefault();

    var JobHeadRec = (from JobHead_Row in Db.JobHead
                      where JobHead_Row.Company == Session.CompanyID
                            && JobHead_Row.JobNum == ShipDtlRow.JobNum
                      select JobHead_Row).FirstOrDefault();

    decimal TotalOurInventoryShipQty = (from ShipDtl_Row in Db.ShipDtl
                                        where ShipDtl_Row.Company == Session.CompanyID
                                              && ShipDtl_Row.OrderNum == OrderRelRec.OrderNum
                                              && ShipDtl_Row.OrderLine == OrderRelRec.OrderLine
                                              && ShipDtl_Row.OrderRelNum == OrderRelRec.OrderRelNum
                                        //&& ShipDtl_Row.PackNum != ShipDtlRow.PackNum
                                        //&& ShipDtl_Row.PackLine != ShipDtlRow.PackLine
                                        select ShipDtl_Row.OurInventoryShipQty).DefaultIfEmpty(0).Sum();

    decimal TotalOurJobShipQty = (from ShipDtl_Row in Db.ShipDtl
                                  where ShipDtl_Row.Company == Session.CompanyID
                                        && ShipDtl_Row.OrderNum == OrderRelRec.OrderNum
                                        && ShipDtl_Row.OrderLine == OrderRelRec.OrderLine
                                        && ShipDtl_Row.OrderRelNum == OrderRelRec.OrderRelNum
                                  //&& ShipDtl_Row.PackNum != ShipDtlRow.PackNum
                                  //&& ShipDtl_Row.PackLine != ShipDtlRow.PackLine
                                  select ShipDtl_Row.OurJobShipQty).DefaultIfEmpty(0).Sum();

    decimal TotalSumQty = TotalOurInventoryShipQty + TotalOurJobShipQty;


    if (OrderRelRec != null && TotalSumQty > JobHeadRec.QtyCompleted)
    {

        //string message = "Shipping Quantity: + TotalSumQty is more than the Job Completed Qty: " + JobHeadRec.QtyCompleted;
        string message = $"Shipping Quantity: {TotalSumQty:F2} is more than the Job Completed Qty: {JobHeadRec.QtyCompleted:F2}";

        throw new BLException(message);


    }
}