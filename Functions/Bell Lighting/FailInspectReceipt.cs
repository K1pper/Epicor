////////////////////////////////////////////////////////////////////////
// GHA 358 Good In Inspection
////////////////////////////////////////////////////////////////////////
// Function to perform inspection processing on container receipt
// Version 1.0
// 
// Written by Peter Roden - GHA Solutions - 17/06/2026
////////////////////////////////////////////////////////////////////////

string legalNumberMessage;
int iDMRNum = 0;
int iNonConfID;
var func = "ReceiveContainerForInspection";

ThisLib.Logger($"FAIL INSPECT RECIEPT", func);

var udCodes = Db.UDCodes.Where(r => r.Company == Company).Where(r => r.CodeTypeID == "GoodsIn").ToList();

var defIRcCode = udCodes.Where(r => r.CodeID == "DefIRC").FirstOrDefault();
if (defIRcCode == null)
{
    ThisLib.Logger("Inspection Code not found", func);
    return;
}
var defIRc = defIRcCode.CodeDesc;

var defInspCode = udCodes.Where(r => r.CodeID == "DefInsp").FirstOrDefault();
if (defInspCode == null)
{
    ThisLib.Logger("Inspector not found", func);
    return;
}
var defInsp = defInspCode.CodeDesc;

var currentPlant = Db.PlantConfCtrl.Where(r => r.Company == Company).Where(r => r.Plant == Plant).FirstOrDefault();
if (currentPlant == null)
{
    ThisLib.Logger("Current Plant not found", func);
    return;
}
var warehouseCode = currentPlant.DefDMRWhse;
var binNum = currentPlant.DefDMRBin;

this.CallService<Erp.Contracts.InspProcessingSvcContract>(inspProcessingSvc =>
{
    try
    {
        var inspProcessingDs = inspProcessingSvc.GetReceiptByID(VendorNum, PurPoint, PackSlip, PackLine);

        inspProcessingDs.InspRcpt[0].InspectionPending = false;
        inspProcessingDs.InspRcpt[0].InspectorID = defInsp;
        inspProcessingDs.InspRcpt[0].InspectedBy = defInsp;
        inspProcessingDs.InspRcpt[0].InspectedTime = (int)DateTime.Now.TimeOfDay.TotalSeconds;
        inspProcessingDs.InspRcpt[0].InspectedDate = DateTime.Today;
        inspProcessingDs.InspRcpt[0].FailedQty = OurQty;
        inspProcessingDs.InspRcpt[0].DimFailedQty = OurQty;
        inspProcessingDs.InspRcpt[0].Done = true;
        inspProcessingDs.InspRcpt[0].FailedWarehouseCode = warehouseCode;
        inspProcessingDs.InspRcpt[0].FailedBin = binNum;
        inspProcessingDs.InspRcpt[0].FailedReasonCode = defIRc;
        inspProcessingDs.InspRcpt[0].RowMod = "U";

        inspProcessingSvc.InspectReceipt(out legalNumberMessage, out iDMRNum, out iNonConfID, ref inspProcessingDs);
    }
    catch (Exception ex)
    {
        ThisLib.Logger($"Error in Inspection fails {ex.ToString()}", func);
    }
});

DMRNum = iDMRNum;