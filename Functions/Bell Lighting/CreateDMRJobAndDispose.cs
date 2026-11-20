////////////////////////////////////////////////////////////////////////
// GHA 358 Good In Inspection
////////////////////////////////////////////////////////////////////////
// Function to perform inspection processing on container receipt
// Version 1.0
// 
// Written by Peter Roden - GHA Solutions - 17/06/2026
////////////////////////////////////////////////////////////////////////

var func = "ReceiveContainerForInspection";
ThisLib.Logger($"CREATE DMR JOB AND DISPOSE", func);


if (DMRNum > 0)
{
    var udCodes = Db.UDCodes.Where(r => r.Company == Company).Where(r => r.CodeTypeID == "GoodsIn").ToList();

    var tempJobCode = udCodes.Where(r => r.CodeID == "TempJob").FirstOrDefault();
    if (tempJobCode == null)
    {
        return;
    }
    var tempJob = tempJobCode.CodeDesc;

    var defDRCCode = udCodes.Where(r => r.CodeID == "DefDRC").FirstOrDefault();
    if (defDRCCode == null)
    {
        return;
    }
    var defDRC = defDRCCode.CodeDesc;

    var currentPlant = Db.PlantConfCtrl.Where(r => r.Company == Company).Where(r => r.Plant == Plant).FirstOrDefault();
    if (currentPlant == null)
    {
        ThisLib.Logger("Current Plant not found", func);
        return;
    }
    var warehouseCode = currentPlant.DefDMRWhse;
    var binNum = currentPlant.DefDMRBin;

    this.CallService<Erp.Contracts.JobEntrySvcContract>(jobEntrySvc =>
    {
        try
        {
            var jobEntryDs = new Erp.Tablesets.JobEntryTableset();
            jobEntrySvc.GetNewJobHead(ref jobEntryDs);

            jobEntryDs.JobHead[0].JobNum = "DMR" + DMRNum.ToString();
            jobEntryDs.JobHead[0].PartNum = PartNum;
            jobEntryDs.JobHead[0].PartDescription = PartNum;
            jobEntryDs.JobHead[0].IUM = IUM;
            jobEntryDs.JobHead[0].ReqDueDate = DateTime.Today;
            jobEntryDs.JobHead[0].Plant = Plant;


            jobEntrySvc.Update(ref jobEntryDs);

            string warningList = "";
            jobEntryDs = jobEntrySvc.GetDetailsMsgWarning(jobEntryDs.JobHead[0].JobNum, 0, "Job", 0, 0, tempJob, 0, "", "", "", true, false, true, false, out warningList);

            jobEntrySvc.Update(ref jobEntryDs);

            jobEntrySvc.GetNewJobProd(ref jobEntryDs, jobEntryDs.JobHead[0].JobNum, PartNum, 0, 0, 0, "", "", 0, 0);

            jobEntryDs.JobProd[0].MakeToType = "STOCK";
            jobEntryDs.JobProd[0].MakeToStockQty = Quantity;
            jobEntryDs.JobProd[0].StkWIPQty = Quantity;
            jobEntryDs.JobProd[0].WarehouseCode = warehouseCode;

            jobEntrySvc.Update(ref jobEntryDs);

            jobEntrySvc.GetNewJobMtl(ref jobEntryDs, jobEntryDs.JobHead[0].JobNum, 0);

            var vMsgText = "";
            var vSubAvail = false;
            var vMsgType = "";
            var multipleMatch = false;
            var opPartChgCompleted = true;
            var opMtlIssuedAction = "NONE";

            jobEntrySvc.ChangeJobMtlPartNum(ref jobEntryDs, true, ref PartNum, Guid.NewGuid(), "", "", out vMsgText, out vSubAvail, out vMsgType, out multipleMatch, out opPartChgCompleted, out opMtlIssuedAction);

            jobEntryDs.JobMtl[0].PartNum = PartNum;
            jobEntryDs.JobMtl[0].Description = PartNum;
            jobEntryDs.JobMtl[0].IUM = IUM;
            jobEntryDs.JobMtl[0].QtyPer = 1;

            jobEntrySvc.Update(ref jobEntryDs);

            jobEntryDs.JobHead[0].JobEngineered = true;
            jobEntryDs.JobHead[0].JobReleased = true;
            jobEntryDs.JobHead[0].EnableJobFirm = false;
            jobEntryDs.JobHead[0].RowMod = "U";

            jobEntrySvc.ChangeJobHeadJobReleased(ref jobEntryDs);

            jobEntrySvc.Update(ref jobEntryDs);
        }
        catch (Exception ex)
        {
            ThisLib.Logger($"Error in DMR Job Creation {ex.ToString()}", func);
        }
    });

    this.CallService<Erp.Contracts.DMRProcessingSvcContract>(dmrSvc =>
    {
        try
        {
            var dmrDs = dmrSvc.GetByID(DMRNum);

            dmrSvc.GetNewDMRActnAcceptMTL(ref dmrDs, DMRNum);

            dmrDs.DMRActn[0].JobNum = "DMR" + DMRNum.ToString();
            dmrDs.DMRActn[0].DMRSeqNum = 10;
            dmrDs.DMRActn[0].Quantity = Quantity;
            dmrDs.DMRActn[0].DispQuantity = Quantity;
            dmrDs.DMRActn[0].ReasonCode = defDRC;
            dmrDs.DMRActn[0].WarehouseCode = warehouseCode;
            dmrDs.DMRActn[0].BinNum = binNum;
            dmrDs.DMRActn[0].IssuedComplete = true;

            string opLegalNumMessage;
            dmrSvc.CustomUpdate(ref dmrDs, out opLegalNumMessage);
        }
        catch (Exception ex)
        {
            ThisLib.Logger($"Error in DMR Processing to Job {ex.ToString()}", func);
        }
    });
}
