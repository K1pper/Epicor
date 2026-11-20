////////////////////////////////////////////////////////////////////////
// GHA 358 Good In Inspection
////////////////////////////////////////////////////////////////////////
// Function to perform inspection processing on container receipt
// Version 1.0
// 
// Written by Peter Roden - GHA Solutions - 17/06/2026
////////////////////////////////////////////////////////////////////////

string legalNumberMessage;
bool requiresUserInput;
int iDMRNum = 0;
int iNonConfID;

var udCodes = Db.UDCodes.Where(r => r.Company == Company).Where(r => r.CodeTypeID == "GoodsIn").ToList();

var defIRcCode = udCodes.Where(r => r.CodeID == "DefIRC").FirstOrDefault();
if (defIRcCode == null)
{
    return;
}
var defIRc = defIRcCode.CodeDesc;

var defInspCode = udCodes.Where(r => r.CodeID == "DefInsp").FirstOrDefault();
if (defInspCode == null)
{
    return;
}
var defInsp = defInspCode.CodeDesc;

var currentPlant = Db.PlantConfCtrl.Where(r => r.Company == Company).Where(r => r.Plant == Plant).FirstOrDefault();
if (currentPlant == null)
{
    return;
}
var warehouseCode = currentPlant.DefDMRWhse;
var binNum = currentPlant.DefDMRBin;

this.CallService<Erp.Contracts.InspProcessingSvcContract>(inspProcessingSvc =>
{
    try
    {
        var inspProcessingDs = inspProcessingSvc.GetByID(TranID);

        var nc = inspProcessingDs.InspNonConf[0];

        inspProcessingDs.InspNonConf[0].InspectorID = defInsp;
        inspProcessingDs.InspNonConf[0].InspectedBy = defInsp;
        inspProcessingDs.InspNonConf[0].FailedQty = Quantity;
        inspProcessingDs.InspNonConf[0].DimFailedQty = Quantity;
        inspProcessingDs.InspNonConf[0].FailedWarehouseCode = warehouseCode;
        inspProcessingDs.InspNonConf[0].FailedBin = binNum;
        inspProcessingDs.InspNonConf[0].FailedReasonCode = defIRc;
        inspProcessingDs.InspNonConf[0].RowMod = "U";

        inspProcessingSvc.GetPassedLegalNumGenOpts(ref inspProcessingDs, "Inventory", out requiresUserInput);

        inspProcessingDs.InspNonConf[0].InspectionPending = true;
        inspProcessingDs.InspNonConf[0].InspectorID = defInsp;
        inspProcessingDs.InspNonConf[0].InspectedBy = defInsp;
        inspProcessingDs.InspNonConf[0].MoveCostsToDMR = true;
        inspProcessingDs.InspNonConf[0].FailedQty = Quantity;
        inspProcessingDs.InspNonConf[0].DimFailedQty = Quantity;
        inspProcessingDs.InspNonConf[0].Done = true;
        inspProcessingDs.InspNonConf[0].FailedWarehouseCode = warehouseCode;
        inspProcessingDs.InspNonConf[0].FailedBin = binNum;
        inspProcessingDs.InspNonConf[0].FailedReasonCode = defIRc;
        inspProcessingDs.InspNonConf[0].RowMod = "U";

        inspProcessingSvc.InspectInventory(out legalNumberMessage, out iDMRNum, ref inspProcessingDs);
    }
    catch (Exception ex)
    {
        Ice.Diagnostics.Log.WriteEntry($"Error in Inspection fails {ex.ToString()}");
    }
});

DMRNum = iDMRNum;