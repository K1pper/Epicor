///////////////////////////////////////////////////////////////////////////////////
//
//  Data Directive for GHA Mobile Field Service
//  Create service job from template
//  Version 1.0.1
//
//  Written by Peter Roden - GHA Solutions - 08/09/2021
//  
//  Fixed issue where billable flag not copied for operations
//    Peter Roden 20/02/2024
//
///////////////////////////////////////////////////////////////////////////////////


var engineered = false;
var released = false;

var call = ds.FSCallDt.FirstOrDefault();
if (call == null) return;

var templateJobNum = call.UDField<System.String>("GHA_MFS_TemplateJob_c");

if (string.IsNullOrEmpty(templateJobNum)) return;

var prefix = Db.FsSyst.Where(r => r.Company == call.Company).FirstOrDefault();
if (prefix == null) return;

var jobNum = prefix.CallJobPrefix + call.CallNum.ToString("D6") + call.CallLine.ToString("D4");

using (var svcJobEntry = Ice.Assemblies.ServiceRenderer.GetService<Erp.Contracts.JobEntrySvcContract>(Db) )
{
  svcJobEntry.GetDetails(
    currJobNum: jobNum,
    currAsmSeq: 0,
    sourceFile: "Job",
    sourceQuote: 0,
    sourceLine: 0,
    sourceJob: templateJobNum,
    sourceAsm: 0,
    sourcePart: string.Empty,
    sourceRev: string.Empty,
    sourceAltMethod: string.Empty,
    resequence: true,
    useMethodForParts: false,
    getCostsFromInv: false,
    getCostsFromTemp: true
  );
  

  using (System.Transactions.TransactionScope trans = IceDataContext.CreateDefaultTransactionScope())
  {
    var jobOper = (from row in Db.JobOper.With(LockHint.UpdLock)
                 where row.Company == call.Company && row.JobNum == jobNum
                 select row).ToList();
             
    var labourRate = call.UDField<System.String>("GHA_MFS_LabourEntryMethod_c") == "FC" ? 0 : call.UDField<System.Decimal>("GHA_MFS_LabourRateValue_c");
             
    foreach (var op in jobOper)
    {
    /* fix issue where billable flag does not copy from template job but is always set to true */
      var templateJobOper = (from trow in Db.JobOper.With(LockHint.UpdLock)
                 where trow.Company == call.Company && trow.JobNum == templateJobNum && trow.OprSeq == op.OprSeq
                 select trow).FirstOrDefault();
    
      if (templateJobOper != null) op.Billable = templateJobOper.Billable;
        
      if (call.UDField<System.Decimal>("GHA_MFS_LabourRateValue_c") > 0)
      {
        op.DocLaborRate = labourRate;
        op.LaborRate = labourRate;
        op.DocBillableLaborRate = labourRate;
        op.BillableLaborRate = labourRate;
        Db.Validate(op);
      }
    }      
    trans.Complete();
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