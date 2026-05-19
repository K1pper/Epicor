Result = string.Empty;

Result = "It works!";
Result += System.Environment.NewLine;

var Continue = true;
var closeJob = false;
var jobNum = string.Empty;

Ice.Diagnostics.Log.WriteEntry($"Batch Job Processing...");

try
{
  if (input.Tables.Count > 0)
  {
    var NumRows = 0;
        
    var Codes = Db.UDCodes.Where(r => r.CodeTypeID == "BatchJob").ToList();
        
    string employee = Codes.Where(r => r.CodeID == "DefEmp").Select(r => r.CodeDesc).FirstOrDefault() ?? string.Empty; 
        
    if (string.IsNullOrEmpty(employee))
    {
      Result += "Default employee not found, check user codes";
      Continue = false; 
    }
    
    Result += $"Employee is {employee}";
    Result += System.Environment.NewLine;
        
    if (Continue)
    {
      foreach (DataRow row in input.Tables[0].Rows)
      {
        closeJob = false;
        if ((bool)row["Calculated_Add"] == false) continue;
        
        Result += $"Job Number {row["JobOper_JobNum"]}" + ", ";
        Result += System.Environment.NewLine + System.Environment.NewLine;
        
        //Result += $"{JsonConvert.SerializeObject(row)}";
        Result += System.Environment.NewLine + System.Environment.NewLine;
        
        var labourHead = Db.LaborHed.Where(r => r.Company == Session.CompanyID).Where(r => r.EmployeeNum == employee).Where(r => r.PayrollDate == DateTime.Today).FirstOrDefault();
        Result += $"Labour head checked" + ", ";
        Result += System.Environment.NewLine + System.Environment.NewLine;
        Result += $"Job Number {labourHead == null}" + ", ";
        Result += System.Environment.NewLine + System.Environment.NewLine;
        var isLabourHead = labourHead != null;
        Result += $"Job Number {isLabourHead}" + ", ";
        Result += System.Environment.NewLine + System.Environment.NewLine;
        
        this.CallService<Erp.Contracts.LaborSvcContract>(labour =>
        {
          var ts = new Erp.Tablesets.LaborTableset();
      
          if (!isLabourHead)
          {
            Result += $"Labour Head is not found, add one...";
            Result += System.Environment.NewLine + System.Environment.NewLine;
            
            labour.GetNewLaborDtlNoHdr(ref ts, employee, false, DateTime.Today, 0, DateTime.Today, 0);
            
            Result += $"Got new dtl";
            Result += System.Environment.NewLine + System.Environment.NewLine;
          }
          else
          {
            Result += $"Labour Head found, {labourHead.LaborHedSeq}";
            Result += System.Environment.NewLine + System.Environment.NewLine;
            
            labour.GetNewLaborDtlWithHdr(ref ts, DateTime.Today, 0, DateTime.Today, 0, labourHead.LaborHedSeq);
            
            Result += $"Got new dtl";
            Result += System.Environment.NewLine + System.Environment.NewLine;
          }
          
          var dtl = ts.LaborDtl.Where(r => r.RowMod == "A").FirstOrDefault();
          
          if (dtl == null) Continue = false;
          
          Result += $"Continue {Continue}";
          Result += System.Environment.NewLine + System.Environment.NewLine;
          
          jobNum = row["JobOper_JobNum"].ToString();
          var opCode = row["JobOper_OpCode"].ToString();
          
          var empBasic = Db.EmpBasic.Where(r => r.Company == Session.CompanyID).Where(r => r.EmpID == employee).FirstOrDefault();
          var opMasDtl = Db.OpMasDtl.Where(r => r.Company == Session.CompanyID).Where(r => r.OpCode == opCode).FirstOrDefault();
          
          if (empBasic == null)
          {
            Result += $"Emp Basic not found";
            Result += System.Environment.NewLine + System.Environment.NewLine;
            Continue = false;
          }
          
          if (empBasic == null)
          {
            Result += $"Emp Basic not found";
            Result += System.Environment.NewLine + System.Environment.NewLine;
            Continue = false;
          }
          
          if (opMasDtl == null)
          {
            Result += $"Op Mas Dtl not found";
            Result += System.Environment.NewLine + System.Environment.NewLine;
            Continue = false;
          }
          
          if (Continue)
          {
            dtl.LaborDtlSeq = 0;
            dtl.JobNum = row["JobOper_JobNum"].ToString();
            dtl.AssemblySeq = 0;
            dtl.OprSeq = Convert.ToInt32(row["JobOper_OprSeq"]);
            dtl.OpCode = row["JobOper_OpCode"].ToString();
            dtl.LaborType = "P";
            dtl.LaborTypePseudo = "P";
            dtl.JCDept = empBasic.JCDept;
            dtl.ExpenseCode = empBasic.ExpenseCode;
            dtl.ResourceGrpID = opMasDtl.ResourceGrpID;
            dtl.OpComplete = true;
            dtl.TimeStatus = "E";
            dtl.ClockInDate = DateTime.Today;
            dtl.ActiveTrans = false;
            dtl.LaborEntryMethod = "T";
            dtl.TimeStatus = "E";
            dtl.EnableComplete = true;
            dtl.Complete = false;

          
          
            Result += $"{JsonConvert.SerializeObject(dtl)}";
            Result += System.Environment.NewLine + System.Environment.NewLine;

            labour.Update(ref ts);
            
            var coParts = (
              from jobHead in Db.JobHead
              join jobHead1 in Db.JobHead on new { jobHead.Company, jobHead.UserChar1 } equals new { jobHead1.Company, jobHead1.UserChar1 }
              join jobMtl in Db.JobMtl on new { jobHead1.Company, jobHead1.JobNum } equals new { jobMtl.Company, jobMtl.JobNum }
              join jobProd in Db.JobProd on new { jobHead1.Company, jobHead1.JobNum } equals new { jobProd.Company, jobProd.JobNum }
              where jobHead.JobNum == jobNum
              group new { jobMtl, jobProd } by jobMtl.PartNum into g
              select new
              {
                PartNum = g.Key,
                ProdQty = g.Sum(x => x.jobProd.ProdQty)
              }
            );
              
            Result += $"Co Parts: {JsonConvert.SerializeObject(coParts)}";
            Result += System.Environment.NewLine + System.Environment.NewLine;

            foreach (var coPart in ts.LaborPart)
            {
              coPart.PartQty = coParts.FirstOrDefault(x => x.PartNum == coPart.PartNum)?.ProdQty ?? 0;
              coPart.RowMod = "U";
            }
            
            labour.Update(ref ts);
          }
          
          string cMessageText = "";
          dtl.RowMod = "U";
          
          this.callContextBpmData.ShortChar01 = "BATCHJOB";
          this.callContextBpmData.ShortChar02 = row["JobOper_JobNum"].ToString();
          this.callContextBpmData.Number01 = Convert.ToInt32(row["JobOper_OprSeq"]);
          
          labour.SubmitForApproval(ref ts, false, out cMessageText);
          
          this.callContextBpmData.ShortChar01 = "";
          this.callContextBpmData.ShortChar02 = "";
          this.callContextBpmData.Number01 = 0;
          
          
          Result += $"Subitted: {cMessageText}";
          Result += System.Environment.NewLine + System.Environment.NewLine;
          
        }); 
        
        var checkClosedJob = Db.JobHead.Where(r => r.Company == Session.CompanyID).Where(r => r.JobNum == jobNum).Where(r => r.ProdQty > 0).Where(r => r.ProdQty == r.QtyCompleted).Where(r => r.JobClosed == false).FirstOrDefault();
        
        closeJob = checkClosedJob != null;
        
        Result += $"Close Parent Job: {closeJob}";
        Result += System.Environment.NewLine + System.Environment.NewLine;
        
        if (closeJob) {
          this.CallService<Erp.Contracts.JobClosingSvcContract>(close =>
          {
            var jc = new Erp.Tablesets.JobClosingTableset();
            
            close.GetNewJobClosing(ref jc);
      
            jc.JobClosing[0].Company = Session.CompanyID;
            jc.JobClosing[0].JobNum = jobNum;
            jc.JobClosing[0].JobClosed = true;
            jc.JobClosing[0].ClosedDate = DateTime.Today;
            jc.JobClosing[0].JobComplete = true;
            jc.JobClosing[0].JobCompletionDate = DateTime.Today;
            jc.JobClosing[0].QuantityContinue = 1;
            jc.JobClosing[0].UserAllowedToCloseJob = true;
            jc.JobClosing[0].RowMod = "U";
            
            string pcMessage = "";
            
            close.CloseJob(ref jc, out pcMessage);
          
          });
        }
        
        var childJobs = Db.JobHead.Where(r => r.Company == Session.CompanyID).Where(r => r.UserChar1 == checkClosedJob.UserChar1).ToList();
          
        foreach (var childJob in childJobs)
        {
          Result += $"Child Job: {childJob.JobNum}";
          Result += System.Environment.NewLine + System.Environment.NewLine;

          closeJob = false;
          var jobProd = Db.JobProd.Where(r => r.Company == childJob.Company).Where(r => r.JobNum == childJob.JobNum).FirstOrDefault();
          if (jobProd.TargetJobNum == "" && jobProd.WarehouseCode == "" && jobProd.OrderNum > 0 && jobProd.ShippedQty >= checkClosedJob.ProdQty) closeJob = true; 
          if (jobProd.TargetJobNum == "" && jobProd.WarehouseCode != "" && jobProd.OrderNum == 0 && jobProd.ShippedQty >= checkClosedJob.ProdQty) closeJob = true; 

          Result += $"Close Childt Job {childJob.JobNum}: {closeJob}";
          Result += System.Environment.NewLine + System.Environment.NewLine;


          if (closeJob)
          {
            this.CallService<Erp.Contracts.JobClosingSvcContract>(close =>
            {
              var jc = new Erp.Tablesets.JobClosingTableset();
              
              close.GetNewJobClosing(ref jc);
              
              jc.JobClosing[0].Company = Session.CompanyID;
              jc.JobClosing[0].JobNum = childJob.JobNum;
              jc.JobClosing[0].JobClosed = true;
              jc.JobClosing[0].ClosedDate = DateTime.Today;
              jc.JobClosing[0].JobComplete = true;
              jc.JobClosing[0].JobCompletionDate = DateTime.Today;
              jc.JobClosing[0].QuantityContinue = 1;
              jc.JobClosing[0].UserAllowedToCloseJob = true;
              jc.JobClosing[0].RowMod = "U";
          
              string pcMessage = "";
          
              close.CloseJob(ref jc, out pcMessage);
        
            });
          }
        }
      }
    }
  }
  else
  {
    Result += "No jobs have been selected";
  }
}
catch (Exception ex)
{
  Result += "It doesn't work!";
  Result += System.Environment.NewLine;
  Result += ex.ToString();
}

Result = "Batch jobs successfully Processed";