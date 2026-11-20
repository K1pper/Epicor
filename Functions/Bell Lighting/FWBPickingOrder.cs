
/* No longer required, here because I wrote and teted the code so may serve as an example */

var fileName = "FWBPickingOrders";

StringBuilder log = new StringBuilder();

var pickDays = 0;
var dateTo = DateTime.Today;

var pickDaysCode = Db.UDCodes.Where(r => r.CodeTypeID.ToUpper() == "AUTOALLOC").Where(r => r.CodeID.ToUpper() == "PICKDAYS").FirstOrDefault();

if (pickDaysCode != null)
{
    Int32.TryParse(pickDaysCode.CodeDesc, out pickDays);
    dateTo = DateTime.Today.AddDays(pickDays + 1);
}

var Company = Session.CompanyID;
try
{
    this.CallService<Erp.Contracts.OrderAllocSvcContract>(alloc =>
    {
        var orderrels = (from rel in Db.OrderRel
                      join dtl in Db.OrderDtl on new {rel.Company, rel.OrderNum, rel.OrderLine } equals new { dtl.Company, dtl.OrderNum, dtl.OrderLine }
                      join partAlloc in Db.PartAlloc on new { rel.Company, rel.OrderNum, rel.OrderLine, rel.OrderRelNum } equals new { partAlloc.Company, partAlloc.OrderNum, partAlloc.OrderLine, partAlloc.OrderRelNum }
                  into partAllocJoin
                    from partAlloc in partAllocJoin.DefaultIfEmpty()
                    where rel.Company == Session.CompanyID && rel.Make == false && rel.ReqDate < dateTo && (partAlloc == null ? 0 : partAlloc.AllocatedQty) > 0 && (partAlloc == null ? 0 : partAlloc.PickingQty) == 0
                    select new
                    {
                        OrderRel = rel,
                        ReservedQty = partAlloc == null ? 0 : partAlloc.ReservedQty,
                        AllocatedQty = partAlloc == null ? 0 : partAlloc.AllocatedQty,
                        PickingQty = partAlloc == null ? 0 : partAlloc.PickingQty,
                        PickedQty = partAlloc == null ? 0 : partAlloc.PickedQty
                    }).ToList();

        foreach (var rel in orderrels)
        {
            log.AppendLine($"Order {rel.OrderRel.OrderNum}/{rel.OrderRel.OrderLine}/{rel.OrderRel.OrderRelNum}");

            var ts = new Erp.Tablesets.OrderAllocTableset();

            var orderAlloc = new Erp.Tablesets.OrderAllocRow();
            orderAlloc.Company = Company;
            orderAlloc.JobNum = "";
            orderAlloc.AssemblySeq = 0;
            orderAlloc.DemandType = "Order";
            orderAlloc.MtlSeq = 0;
            orderAlloc.OrderLine = rel.OrderRel.OrderLine;
            orderAlloc.OrderNum = rel.OrderRel.OrderNum;
            orderAlloc.OrderRelNum = rel.OrderRel.OrderRelNum;
            orderAlloc.ReservePriorityOverride = 0;
            orderAlloc.TFOrdLine = 0;
            orderAlloc.TFOrdNum = "";
            orderAlloc.EnableSelectForPicking = true;
            orderAlloc.SelectForPicking = true;
            orderAlloc.SelectedForAction = true;

            var qtyAlloc = rel.ReservedQty + rel.AllocatedQty + rel.PickingQty + rel.PickedQty;
            var qtyReq = rel.OrderRel.OurReqQty;

            orderAlloc.FulfilledQty = qtyReq - qtyAlloc;
            orderAlloc.RequiredQty = qtyReq - qtyAlloc;
            orderAlloc.OurReqQty = qtyReq - qtyAlloc;
            orderAlloc.ReservedQty = qtyReq - qtyAlloc;
            orderAlloc.RowMod = "U";

            ts.OrderAlloc.Add(orderAlloc);

            bool success;
            string Message = "";
            int iReleaseForPickingSeq;
            alloc.AutoPick(ref ts, "", out success, out Message);
            log.AppendLine(Message);
            alloc.SubmitForPicking(ref ts, 0, null, false, out Message, out iReleaseForPickingSeq, out success);
            log.AppendLine(Message);
        }

    });
}
catch (Exception ex)
{
    log.AppendLine(ex.ToString());
}


using (var logger = Ice.Logging.ApplicationLoggerBuilder.CreateDefaultBuilder(this.Session, fileName)
  .ConfigureFile(options => { options.TruncateFile = true; options.MessageOptions.TimestampFormat = TimestampFormat.DateTime; })
  .Build())
{
    logger.LogInformation(log.ToString());
}