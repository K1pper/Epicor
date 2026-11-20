////////////////////////////////////////////////////////////////////////
// GHA 358 Good In Inspection
////////////////////////////////////////////////////////////////////////
// Function to perform inspection processing on container receipt
// Version 1.0
// 
// Written by Peter Roden - GHA Solutions - 17/06/2026
////////////////////////////////////////////////////////////////////////

Ice.Diagnostics.Log.WriteEntry("Release Stock");

var udCodes = Db.UDCodes.Where(r => r.Company == Session.CompanyID).Where(r => r.CodeTypeID == "GoodsIn").ToList();

var defDRCCode = udCodes.Where(r => r.CodeID == "DefDRC").FirstOrDefault();
if (defDRCCode == null)
{
    return;
}
var defDRC = defDRCCode.CodeDesc;

try
{
    if (input.Tables.Count > 0)
    {
        foreach (DataRow r in input.Tables[0].Rows)
        {
            if ((bool)r["Calculated_Process"] == false) continue;

            int poNum = Convert.ToInt32(r["RcvDtl_PONum"]);
            int poLine = Convert.ToInt32(r["RcvDtl_POLine"]);
            int poRelNum = Convert.ToInt32(r["RcvDtl_PORelNum"]);

            var dmrs = inputDetail.Tables[0]
                .AsEnumerable()
                .Where(d => Convert.ToInt32(d["RcvDtl_PONum"]) == poNum)
                .Where(d => Convert.ToInt32(d["RcvDtl_POLine"]) == poLine)
                .Where(d => Convert.ToInt32(d["RcvDtl_PORelNum"]) == poRelNum)
                .ToList();

            foreach (DataRow dmr in dmrs)
            {
                Ice.Diagnostics.Log.WriteEntry($"{JsonConvert.SerializeObject(dmr)}");
                if (!string.IsNullOrEmpty(dmr["JobHead_JobNum"].ToString())) continue;
                var DMRNum = Convert.ToDouble(dmr["DMRActn_DMRNum"]);
                Ice.Diagnostics.Log.WriteEntry($"1");
                var Quantity = Convert.ToDecimal(dmr["Calculated_Quantiy"]);

                this.CallService<Erp.Contracts.DMRProcessingSvcContract>(dmrSvc =>
                {
                    try
                    {
                        var dmrDs = dmrSvc.GetByID((int)DMRNum);

                        var currentPlant = Db.PlantConfCtrl.Where(p => p.Company == dmrDs.DMRHead[0].Company).Where(p => p.Plant == dmrDs.DMRHead[0].Plant).FirstOrDefault();
                        if (currentPlant != null)
                        {
                            var warehouseCode = currentPlant.DefaultWhse;
                            var binNum = currentPlant.DefGenBin;

                            dmrSvc.GetNewDMRActnAcceptSTK(ref dmrDs, (int)DMRNum);

                            dmrDs.DMRActn[0].Quantity = Quantity;
                            dmrDs.DMRActn[0].DispQuantity = Quantity;
                            dmrDs.DMRActn[0].ReasonCode = defDRC;
                            dmrDs.DMRActn[0].WarehouseCode = warehouseCode;
                            dmrDs.DMRActn[0].BinNum = binNum;
                            dmrDs.DMRActn[0].IssuedComplete = true;
                            dmrDs.DMRActn[0].OpenDMR = false;

                            string opLegalNumMessage;
                            dmrSvc.CustomUpdate(ref dmrDs, out opLegalNumMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        Ice.Diagnostics.Log.WriteEntry($"Error in DMR Processing to Stock {ex.ToString()}");
                    }
                });
            }
        }
    }
}
catch (Exception ex)
{
    Ice.Diagnostics.Log.WriteEntry($"Release Stock error: {ex.ToString()}");
}