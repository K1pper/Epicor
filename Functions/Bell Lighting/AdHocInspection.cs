////////////////////////////////////////////////////////////////////////
// GHA 362 Ad Hoc Inspection
////////////////////////////////////////////////////////////////////////
// Function to perform ad hoc inspection processing
// Version 1.0
// 
// Written by Peter Roden - GHA Solutions - 17/06/2026
////////////////////////////////////////////////////////////////////////

var fn = "AdHocInspection";

ThisLib.Logger("=== Ad Hoc Inspection ===", fn);

var jobsPerDay = 0;
int[] jobRows = { 0 };
List<(string JobNum, string Operation, decimal Time)> jobs = new List<(string JobNum, string Operation, decimal Time)>();

var adHocInspCodes = Db.UDCodes.Where(r => r.Company == Session.CompanyID).Where(r => r.CodeTypeID == "AdHocInsp").Where(r => r.CodeID == "JobsPerDay").FirstOrDefault();

if (adHocInspCodes != null)
{
    Int32.TryParse(adHocInspCodes.CodeDesc, out jobsPerDay);
}

CallService<Ice.Contracts.DynamicQuerySvcContract>(svc =>
{
    using (DataSet queryResults = svc.ExecuteByID("GHA_AdHocInspection", new QueryExecutionTableset()))
    {
        try
        {
            var jobCount = queryResults.Tables["Results"].Rows.Count;

            jobsPerDay = Math.Min(jobCount, jobsPerDay);

            Random rng = new Random();

            jobRows = Enumerable
              .Range(0, jobCount)
              .OrderBy(_ => rng.Next())
              .Take(jobsPerDay)
              .ToArray();

            ThisLib.Logger($"Jobs selected are {string.Join(", ", jobRows)}", fn);

            foreach (var jobRow in jobRows)
            {
                decimal InspectionTime = 0m;
                Decimal.TryParse(queryResults.Tables["Results"].Rows[jobRow]["Part_InspectionTime_c"].ToString(), out InspectionTime);
                jobs.Add((
                  queryResults.Tables["Results"].Rows[jobRow]["JobHead_JobNum"].ToString(),
                  queryResults.Tables["Results"].Rows[jobRow]["Part_InspectionOperation_c"].ToString(),
                  InspectionTime
                ));
            }
        }
        catch (Exception ex)
        {
            ThisLib.Logger($"Query Error {ex.ToString()}", fn);
        }
    }
});



this.CallService<Erp.Contracts.JobEntrySvcContract>(jobSvc =>
{
    try
    {
        foreach (var job in jobs)
        {
            ThisLib.Logger($"Updating job {job.JobNum}", fn);
            var jobTs = jobSvc.GetByID(job.JobNum);

            jobSvc.GetNewJobOper(ref jobTs, job.JobNum, 0);

            var newOp = jobTs.JobOper.Where(r => r.RowMod == "A").FirstOrDefault();
            if (newOp == null) continue;

            var existingOps = Db.JobOper.Where(r => r.Company == newOp.Company).Where(r => r.JobNum == newOp.JobNum).ToList();

            var nextUnstartedOpSeq = 10000;
            var lastCompletedOpSeq = 0;

            foreach (var op in existingOps.OrderBy(r => r.OprSeq))
            {
                var isLabour = Db.LaborDtl.Where(r => r.Company == newOp.Company).Where(r => r.JobNum == op.JobNum).Where(r => r.AssemblySeq == op.AssemblySeq).Where(r => r.OprSeq == op.OprSeq).Any();

                if (isLabour || op.OpComplete)
                    lastCompletedOpSeq = op.OprSeq;

                if (!isLabour)
                {
                    nextUnstartedOpSeq = op.OprSeq;
                    break;
                }
            }

            newOp.OprSeq = (lastCompletedOpSeq + nextUnstartedOpSeq) / 2;
            newOp.OpCode = job.Operation;
            newOp.ProdStandard = job.Time;
            newOp.StdFormat = "HR";

            jobSvc.Update(ref jobTs);

            jobTs = jobSvc.GetByID(job.JobNum);
            var jobHead = jobTs.JobHead[0];
            jobHead.SetUDField<bool>("AdHocInspection_c", true);
            jobHead.SetUDField<DateTime>("AdHocInpectionDate_c", DateTime.Today);
            jobHead.RowMod = "U";

            jobSvc.Update(ref jobTs);
        }
    }
    catch (Exception ex)
    {
        ThisLib.Logger($"Job Entry Error {ex.ToString()}", fn);
    }
});
