///////////////////////////////////////////////////////////////////////////////////
//
//  Data Directive for GHA Mobile Field Service
//  Create service job from quote
//  Version 1.0.1
//
//  Written by Peter Roden - GHA Solutions - 26/10/2021
//
///////////////////////////////////////////////////////////////////////////////////


var engineered = false;
var released = false;

var call = ds.FSCallDt.FirstOrDefault();
if (call == null) return;

if (call.UDField<System.Int32>("GHA_MFS_TemplateQuote_c") == 0) return;

var prefix = Db.FsSyst.Where(r => r.Company == call.Company).FirstOrDefault();
if (prefix == null) return;

var jobNum = prefix.CallJobPrefix + call.CallNum.ToString("D6") + call.CallLine.ToString("D4");
var quoteNum = call.UDField<System.Int32>("GHA_MFS_TemplateQuote_c");

var callHd = Db.FSCallhd.Where(r => r.Company == call.Company).Where(r => r.CallNum == call.CallNum).FirstOrDefault();
if (callHd == null) return;

var rows = Db.QuoteDtl.Where(r => r.Company == call.Company).Where(r => r.QuoteNum == quoteNum).Where(r => r.Template == true).ToList();

if (rows == null)
    Epicor.Customization.Bpm.InfoMessage.Publish("The quote selected does not have any lines mrked as a template. The job will not have any operations or material added.");

foreach (var quoteline in rows)
{
    using (var svcJobEntry = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.JobEntrySvcContract>(Db))
    {
        JbAppendTableset appendDetails = svcJobEntry.BuildAppendDetails(
          sourceFile: "Quote",
          keyOne: quoteline.QuoteNum.ToString(),
          keyTwo: quoteline.QuoteLine.ToString(),
          keyThree: "0",
          targetJob: jobNum,
          targetAsm: 0,
          vDueDate: callHd.SchedDate ?? DateTime.Today
        );

        string errors = string.Empty;

        svcJobEntry.AppendDetails(
          ds: appendDetails,
          targetJob: jobNum,
          targetAsm: 0,
          sourceFile: "Quote",
          keyOne: quoteline.QuoteNum.ToString(),
          keyTwo: quoteline.QuoteLine.ToString(),
          keyThree: "0",
          getCostsFromInv: false,
          getCostsFromTemp: true,
          errorList: out errors
        );

        if (call.UDField<System.Decimal>("GHA_MFS_LabourRateValue_c") > 0)
        {
            using (System.Transactions.TransactionScope trans = IceDataContext.CreateDefaultTransactionScope())
            {
                var jobOper = (from row in Db.JobOper.With(LockHint.UpdLock)
                               where row.Company == call.Company && row.JobNum == jobNum
                               select row).ToList();

                foreach (var op in jobOper)
                {
                    op.DocLaborRate = call.UDField<System.Decimal>("GHA_MFS_LabourRateValue_c");
                    op.LaborRate = call.UDField<System.Decimal>("GHA_MFS_LabourRateValue_c");
                    op.DocBillableLaborRate = call.UDField<System.Decimal>("GHA_MFS_LabourRateValue_c");
                    op.BillableLaborRate = call.UDField<System.Decimal>("GHA_MFS_LabourRateValue_c");
                    Db.Validate(op);
                }
                trans.Complete();
            }
        }

        var matCovered = call.MatCovered;
        var labCovered = call.LabCovered;

        if (call.UDField<System.String>("GHA_MFS_LabourEntyMethod_c") == "FC")
        {
            matCovered = true;
            labCovered = true;
        }


        if (matCovered)
        {
            using (System.Transactions.TransactionScope trans = IceDataContext.CreateDefaultTransactionScope())
            {
                var jobMtl = (from row in Db.JobMtl.With(LockHint.UpdLock)
                              where row.Company == call.Company && row.JobNum == jobNum
                              select row).ToList();

                foreach (var mtl in jobMtl)
                {
                    mtl.Billable = false;
                    Db.Validate(mtl);
                }
                trans.Complete();
            }
        }

        if (labCovered)
        {
            using (System.Transactions.TransactionScope trans = IceDataContext.CreateDefaultTransactionScope())
            {
                var jobOper = (from row in Db.JobOper.With(LockHint.UpdLock)
                               where row.Company == call.Company && row.JobNum == jobNum
                               select row).ToList();

                foreach (var op in jobOper)
                {
                    op.Billable = false;
                    Db.Validate(op);
                }
                trans.Complete();
            }
        }

        var dsJobEntry = svcJobEntry.GetByID(jobNum);

        dsJobEntry.JobHead[0].SetUDField("GHA_MFS_JobType_c", "INST");
        svcJobEntry.Update(ref dsJobEntry);

        if (call.UDField<System.Boolean>("GHA_MFS_Engineered_c"))
        {
            dsJobEntry.JobHead[0].JobEngineered = true;
            dsJobEntry.JobHead[0].RowMod = "U";
            svcJobEntry.ChangeJobHeadJobEngineered(ref dsJobEntry);
            engineered = true;
        }


        if (call.UDField<System.Boolean>("GHA_MFS_Released_c"))
        {
            dsJobEntry.JobHead[0].JobEngineered = true;
            dsJobEntry.JobHead[0].JobReleased = true;
            dsJobEntry.JobHead[0].EnableJobFirm = false;
            dsJobEntry.JobHead[0].RowMod = "U";
            svcJobEntry.ChangeJobHeadJobReleased(ref dsJobEntry);
            engineered = true;
            released = true;
        }


        if (call.UDField<System.Boolean>("GHA_MFS_Engineered_c") || call.UDField<System.Boolean>("GHA_MFS_Released_c"))
        {
            dsJobEntry.JobHead[0].JobEngineered = engineered;
            dsJobEntry.JobHead[0].JobReleased = released;
            dsJobEntry.JobHead[0].EnableJobFirm = false;
            svcJobEntry.Update(ref dsJobEntry);
        }
    }
}