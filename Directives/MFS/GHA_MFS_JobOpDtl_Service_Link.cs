///////////////////////////////////////////////////////////////////////////////////
//
//  Data Directive to link Epicor to GHA Mobile Field Service
//  Version 1.1.1
//
//  Written by Peter Roden - GHA Solutions - 08/07/2021
//  Amended by Peter Roden - GHA Solutions - 11/02/2022
//    Changed to use functions and handshake with MFS
//  Amended by Peter Roden - GHA Solutions - 17/10/2023
//     Serial number missing from data call
//  Amended by Peter Roden - GHA Solutions - 27/02/2024
//     Fixed missing OTS Ship to number 
//
///////////////////////////////////////////////////////////////////////////////////

var unchanged = ttJobOpDtl.Where(r => r.Unchanged()).FirstOrDefault();
var updated = ttJobOpDtl.Where(r => r.Updated()).FirstOrDefault();
var added = ttJobOpDtl.Where(r => r.Added()).FirstOrDefault();
var deleted = ttJobOpDtl.Where(r => r.Deleted()).FirstOrDefault();
var delete = deleted != null;

if (unchanged == null && updated == null && added == null && deleted == null) return;

var changed = (added == null) ? (deleted == null) ? updated : deleted : added;

///////////////////////////////////////////////////////////////////////////////////
// Perform any checks
///////////////////////////////////////////////////////////////////////////////////

if (BpmContext.BpmData.Checkbox01 && BpmContext.BpmData.ShortChar01 == "MFS")
{
    BpmContext.BpmData.Checkbox01 = false;
    BpmContext.BpmData.ShortChar01 = "";

    return;
}
if (updated != null)
{
    if (
      unchanged.ResourceID == changed.ResourceID)
    {
        return;
    }
}
var job = (from jobh in Db.JobHead
           join line in Db.FSCallDt on new { jobh.Company, jobh.JobNum } equals new { line.Company, line.JobNum }
           join head in Db.FSCallhd on new { line.Company, line.CallNum } equals new { head.Company, head.CallNum }
           join tech in Db.FsTech on new { line.Company, line.CallNum } equals new { tech.Company, tech.CallNum } into t
           from tech in t.DefaultIfEmpty()
           join cust in Db.Customer on new { head.Company, head.CustNum } equals new { cust.Company, cust.CustNum }
           where jobh.Company == changed.Company && jobh.JobNum == changed.JobNum && jobh.JobComplete == false && jobh.JobClosed == false && jobh.JobReleased == true
           select new
           {
               JobNum = line.JobNum,
               ERPJobNum = jobh.JobNum,
               head.CallNum,
               PartNum = line.PartNum,
               SerialNum = line.SerialNumber,
               CallDate = head.SchedDate,
               CallTime = head.SchedTime,
               JobDate = jobh.StartDate,
               JobTime = (int)jobh.StartHour * 3600,
               ReqDueDate = jobh.ReqDueDate,
               JobComments = jobh.PartDescription,
               Employee = tech.EmpID,
               CustID = cust.CustID,
               ShipToNum = head.UseOTS ? "GHA_MFS_OTS_" + head.CallNum.ToString() : head.ShipToNum,
               ContactShipToNum = head.UseOTS ? "GHA_MFS_OTS_" + head.CallNum.ToString() : head.ShpConNum == 0 ? "" : head.ShipToNum,
               ContactNum = head.UseOTS ? 1 : head.ShpConNum == 0 ? head.PrcConNum : head.ShpConNum,
               MatCovered = line.MatCovered,
               LabCovered = line.LabCovered,
               LabourEntryMethod = line.GHA_MFS_LabourEntryMethod_c,
               TravelTimeEntryMethod = line.GHA_MFS_TimeEntryMethod_c,
               TravelTimeCap = line.GHA_MFS_TravelTimeCap_c,
               MileageCap = line.GHA_MFS_MileageCap_c,
               LabourIncrement = line.GHA_MFS_LabourIncrement_c,
               MinimumLabourPeriod = line.GHA_MFS_MinimumLabourPeriod_c,
               MinimumTravelTime = line.GHA_MFS_MinimumTravelTime_c,
               TravelTimeIncrement = line.GHA_MFS_TravelTimeIncrement_c,
               FixedCost = line.GHA_MFS_FixedCost_c,
               TravelFixedCost = line.GHA_MFS_TravelFixedCost_c,
               FixedLabourRate = line.GHA_MFS_FixedLabourRate_c,
               LabourRate = line.GHA_MFS_LabourRate_c,
               LabourRateValue = line.GHA_MFS_LabourRateValue_c,
               CashPayment = line.GHA_MFS_CollectPayment_c,
               Void = head.VoidCall,
               Released = jobh.JobReleased,
               Complete = jobh.JobComplete,
               JobType = jobh.GHA_MFS_JobType_c
           }
           ).FirstOrDefault();
if (job == null) return;

var call = Db.FSCallhd.Where(r => r.Company == changed.Company).Where(r => r.CallNum == job.CallNum).FirstOrDefault();
var DepositRequired = Db.ZDataField.Where(r => r.DBTableName == "FSCallhd_UD").Where(r => r.DBFieldName == "GHA_MFS_DepositRequired_c").Any() ? (bool)call["GHA_MFS_DepositRequired_c"] : false;
var DepositReceived = Db.ZDataField.Where(r => r.DBTableName == "FSCallhd_UD").Where(r => r.DBFieldName == "GHA_MFS_DepositReceived_c").Any() ? (bool)call["GHA_MFS_DepositReceived_c"] : false;
var DepositAmount = Db.ZDataField.Where(r => r.DBTableName == "FSCallhd_UD").Where(r => r.DBFieldName == "GHA_MFS_Deposit_Amount_c").Any() ? (decimal)call["GHA_MFS_Deposit_Amount_c"] : 0;


var jobDate = (job.JobDate != null) ? ((DateTime)job.JobDate).AddSeconds(job.JobTime).ToString("yyyy-MM-ddTHH:mm:ss.000Z") : job.CallDate != null ? ((DateTime)job.CallDate).AddSeconds(job.CallTime).ToString("yyyy-MM-ddTHH:mm:ss.000Z") : string.Empty;
var resourceId = changed.ResourceID;

var jobEmployee = (from opdtl in Db.JobOpDtl
                   join res in Db.Resource on new { opdtl.Company, opdtl.ResourceID } equals new { res.Company, res.ResourceID }
                   join emp in Db.EmpBasic on new { res.Company, res.ResourceID } equals new { emp.Company, emp.ResourceID }
                   where opdtl.Company == changed.Company && opdtl.JobNum == job.JobNum
                   select new { opdtl.OprSeq, emp.EmpID }).OrderBy(r => r.OprSeq).FirstOrDefault();

var employee = (jobEmployee == null) ? job.Employee == null ? string.Empty : job.Employee : jobEmployee.EmpID;

///////////////////////////////////////////////////////////////////////////////////
//Send changes to field service
///////////////////////////////////////////////////////////////////////////////////

Tuple<String, String> result;
Tuple<int> taskTimeResult;
int taskTime;
string status;
string error;
string keys;
var jobData = new
{
    JobNum = job.JobNum,
    ERPJobNum = job.ERPJobNum,
    JobType = job.JobType,
    PartNum = job.PartNum,
    SerialNumber = job.SerialNum,
    ReqByDate = job.ReqDueDate,
    JobDate = jobDate,
    JobComments = job.JobComments,
    Employee = employee,
    CustID = job.CustID,
    ShipToNum = job.ShipToNum,
    ContactShipToNum = job.ContactShipToNum,
    ContactNum = job.ContactNum,
    MatCovered = job.MatCovered,
    LabCovered = job.LabCovered,
    LabourEntryMethod = job.LabourEntryMethod,
    TravelTimeEntry = job.TravelTimeEntryMethod,
    TravelTimeCap = job.TravelTimeCap,
    MileageCap = job.MileageCap,
    LabourIncrement = job.LabourIncrement,
    MinimumLabourPeriod = job.MinimumLabourPeriod,
    MinimumTravelTime = job.MinimumTravelTime,
    TravelTimeIncrement = job.TravelTimeIncrement,
    FixedCost = job.FixedCost,
    TravelFixedCost = job.TravelFixedCost,
    FixedLabourRate = job.FixedLabourRate,
    LabourRate = job.LabourRate,
    LabourRateValue = job.LabourRateValue,
    CashPayment = job.CashPayment,
    DepositRequired = DepositRequired,
    DepositReceived = DepositReceived,
    DepositAmount = DepositAmount,
    Released = job.Released,
    Void = job.Released ? job.Void : !job.Released,
    Complete = job.Complete,
    Delete = delete
};

var jobDataString = JsonConvert.SerializeObject(new List<object> { jobData });
jobDataString = jobDataString.Replace("\"JobDate\":\"\"", "\"JobDate\": null");
result = (Tuple<String, String>)this.InvokeFunction("GHA-MFS", "CallFieldService", Tuple.Create("Job", jobDataString));
status = result.Item1;
error = result.Item2;
keys = String.Join(",", new string[] { changed.JobNum });
this.InvokeFunction("GHA-MFS", "GetSync", Tuple.Create("JobHead", keys, jobDataString, status, error));

