
/* No longer required, here because I wrote and teted the code so may serve as an example */

var fileName = "FWBPickingJobs";

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
        var jobs = (from mtl in Db.JobMtl
                    join hed in Db.JobHead on new { mtl.Company, mtl.JobNum } equals new { hed.Company, hed.JobNum }
                    join partAlloc in Db.PartAlloc on new { mtl.Company, mtl.JobNum, mtl.AssemblySeq, mtl.MtlSeq } equals new { partAlloc.Company, partAlloc.JobNum, partAlloc.AssemblySeq, partAlloc.MtlSeq }
                  into partAllocJoin
                    from partAlloc in partAllocJoin.DefaultIfEmpty()
                    where mtl.Company == Session.CompanyID && hed.StartDate < dateTo && (partAlloc == null ? 0 : partAlloc.AllocatedQty) > 0 && (partAlloc == null ? 0 : partAlloc.PickingQty) == 0
                    select new
                    {
                        JobMtl = mtl,
                        ReservedQty = partAlloc == null ? 0 : partAlloc.ReservedQty,
                        AllocatedQty = partAlloc == null ? 0 : partAlloc.AllocatedQty,
                        PickingQty = partAlloc == null ? 0 : partAlloc.PickingQty,
                        PickedQty = partAlloc == null ? 0 : partAlloc.PickedQty
                    }).ToList();

        foreach (var job in jobs)
        {
            log.AppendLine($"Job {job.JobMtl.JobNum}/{job.JobMtl.AssemblySeq}/{job.JobMtl.MtlSeq}");

            var ts = new Erp.Tablesets.OrderAllocTableset();

            var orderAlloc = new Erp.Tablesets.OrderAllocRow();
            orderAlloc.Company = Company;
            orderAlloc.JobNum = job.JobMtl.JobNum;
            orderAlloc.AssemblySeq = job.JobMtl.AssemblySeq;
            orderAlloc.DemandType = "Job";
            orderAlloc.MtlSeq = job.JobMtl.MtlSeq;
            orderAlloc.OrderLine = 0;
            orderAlloc.OrderNum = 0;
            orderAlloc.OrderRelNum = 0;
            orderAlloc.ReservePriorityOverride = 0;
            orderAlloc.TFOrdLine = 0;
            orderAlloc.TFOrdNum = "";
            orderAlloc.EnableSelectForPicking = true;
            orderAlloc.SelectForPicking = true;
            orderAlloc.SelectedForAction = true;

            var qtyAlloc = job.ReservedQty + job.AllocatedQty + job.PickingQty + job.PickedQty;
            var qtyReq = job.JobMtl.RequiredQty;

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