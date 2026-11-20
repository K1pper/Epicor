////////////////////////////////////////////////////////////////////////
// GHA 358 Good In Inspection
////////////////////////////////////////////////////////////////////////
// Function to perform inspection processing on container receipt
// Version 1.0
// 
// Written by Peter Roden - GHA Solutions - 17/06/2026
////////////////////////////////////////////////////////////////////////

var fn = "ReserveAndRelease";

Success = false;
Message = "";

bool returnValue;
string returnString;

ThisLib.Logger("=== Reserve and Release Job ===", fn);

this.CallService<Erp.Contracts.OrderAllocSvcContract>(alloc =>
{


    var jobList = alloc.callGetListOfJobs("", "", JobNum, JobNum, "", "", "", "", null, null, null, null, null, null, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 0, 1, out returnValue);

    ThisLib.Logger($"OrderAllocList {JsonConvert.SerializeObject(jobList)}", fn);

    foreach (var a in jobList.OrderAllocList)
    {
        var sts = new Erp.Tablesets.SlimOrderAllocTableset();
        var ts = new Erp.Tablesets.OrderAllocTableset();

        ThisLib.Logger($"Allocating {a.JobNum} material {a.MtlSeq}", fn);

        sts.SlimOrderAlloc.Add(new Erp.Tablesets.SlimOrderAllocRow());

        sts.SlimOrderAlloc[0].Company = a.Company;
        sts.SlimOrderAlloc[0].JobNum = a.JobNum;
        sts.SlimOrderAlloc[0].AssemblySeq = a.AssemblySeq;
        sts.SlimOrderAlloc[0].DemandType = a.DemandType;
        sts.SlimOrderAlloc[0].MtlSeq = a.MtlSeq;
        sts.SlimOrderAlloc[0].OrderLine = a.OrderLine;
        sts.SlimOrderAlloc[0].OrderNum = a.OrderNum;
        sts.SlimOrderAlloc[0].OrderRelNum = a.OrderRelNum;
        sts.SlimOrderAlloc[0].ReservePriorityOverride = 0;
        sts.SlimOrderAlloc[0].SelectedForAction = true;
        sts.SlimOrderAlloc[0].TFOrdLine = a.TFOrdLine;
        sts.SlimOrderAlloc[0].TFOrdNum = a.TFOrdNum;
        sts.SlimOrderAlloc[0].RowMod = "A";

        alloc.AutoReserve(ref sts, "", "all", out returnString);

        ThisLib.Logger($"Auto reserved {returnString}", fn);

        Message += $"{a.JobNum} {a.PartNum}\n";
        Message += returnString + "\n";

        var orderAlloc = new Erp.Tablesets.OrderAllocRow();
        orderAlloc.Company = a.Company;
        orderAlloc.JobNum = a.JobNum;
        orderAlloc.AssemblySeq = a.AssemblySeq;
        orderAlloc.DemandType = a.DemandType;
        orderAlloc.MtlSeq = a.MtlSeq;
        orderAlloc.OrderLine = a.OrderLine;
        orderAlloc.OrderNum = a.OrderNum;
        orderAlloc.OrderRelNum = a.OrderRelNum;
        orderAlloc.ReservePriorityOverride = 0;
        orderAlloc.TFOrdLine = a.TFOrdLine;
        orderAlloc.TFOrdNum = a.TFOrdNum;
        orderAlloc.EnableSelectForPicking = true;
        orderAlloc.SelectedForAction = true;

        var jobMtl = Db.JobMtl
          .Where(r => r.Company == a.Company)
          .Where(r => r.JobNum == a.JobNum)
          .Where(r => r.AssemblySeq == a.AssemblySeq)
          .Where(r => r.MtlSeq == a.MtlSeq)
          .FirstOrDefault();
        var qtyReq = jobMtl == null ? 0 : jobMtl.RequiredQty;

        var partAlloc = Db.PartAlloc
          .Where(r => r.Company == a.Company)
          .Where(r => r.JobNum == a.JobNum)
          .Where(r => r.AssemblySeq == a.AssemblySeq)
          .Where(r => r.MtlSeq == a.MtlSeq)
          .FirstOrDefault();
        var qtyAlloc = partAlloc == null ? 0 : partAlloc.ReservedQty + partAlloc.AllocatedQty + partAlloc.PickingQty + partAlloc.PickedQty;

        orderAlloc.FulfilledQty = qtyReq - qtyAlloc;
        orderAlloc.RequiredQty = qtyReq - qtyAlloc;
        orderAlloc.OurReqQty = qtyReq - qtyAlloc;
        orderAlloc.ReservedQty = qtyReq - qtyAlloc;
        orderAlloc.RowMod = "U";

        ts.OrderAlloc.Add(orderAlloc);

        bool success;
        string messageText;
        int iReleaseForPickingSeq;
        alloc.AutoPick(ref ts, "", out success, out messageText);

        Message += messageText + "\n\n";

        alloc.SubmitForPicking(ref ts, 0, null, false, out messageText, out iReleaseForPickingSeq, out success);

        ThisLib.Logger($"Auto pick: {success}, {messageText}", fn);

        ThisLib.Logger($"Allocation result {JsonConvert.SerializeObject(ts)}", fn);

    }
});
