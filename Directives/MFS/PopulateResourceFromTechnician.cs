///////////////////////////////////////////////////////////////////////////////////
//
//  Data Directive for GHA Mobile Field Service
//  Populate Resoure Group From Technician
//  Version 1.0.1
//
//  Written by Peter Roden - GHA Solutions - 26/11/2021
//
///////////////////////////////////////////////////////////////////////////////////

var call = ds.FSCallDt.FirstOrDefault();
if (call == null) return;

var technician = Db.FsTech.Where(r => r.Company == call.Company).Where(r => r.CallNum == call.CallNum).OrderBy(r => r.EmpID).FirstOrDefault();
if (technician == null) return;

var employee = Db.EmpBasic.Where(r => r.Company == technician.Company).Where(r => r.EmpID == technician.EmpID).FirstOrDefault();
if (employee == null) return;
if (string.IsNullOrEmpty(employee.ResourceGrpID)) return;
if (string.IsNullOrEmpty(employee.ResourceID)) return;


var prefix = Db.FsSyst.Where(r => r.Company == call.Company).FirstOrDefault();
if (prefix == null) return;

var jobNum = prefix.CallJobPrefix + call.CallNum.ToString("D6") + call.CallLine.ToString("D4");

using (System.Transactions.TransactionScope trans = IceDataContext.CreateDefaultTransactionScope())
{
  var jobOpDtl = (from row in Db.JobOpDtl.With(LockHint.UpdLock)
               where row.Company == call.Company && row.JobNum == jobNum
               select row).ToList();
         
  foreach (var op in jobOpDtl)
  {
    op.ResourceGrpID = employee.ResourceGrpID;
    op.ResourceID = employee.ResourceID;
    Db.Validate(op);
  }      
  trans.Complete();
}