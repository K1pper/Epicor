if(!string.IsNullOrEmpty(EmpID))
{

  if (!(from row in Db.EmpBasic where row.Company == Session.CompanyID && row.EmpID == EmpID select row.EmpID).Any())
  {
    if(!(from row in Db.UDCodes where row.Company == Session.CompanyID && row.CodeTypeID == "GHA_MIM_TM" && row.CodeID == EmpID select row.CodeID).Any())
    {
      throw new BLException(string.Format("'{0}' is not a valid Employee or Team ID", EmpID));
    }
    else
    {
      //check the team has at least one member
      if(!(from row in Db.EmpBasic where row.Company == Session.CompanyID && row.GHA_MIMS_Team_c == EmpID select row.EmpID).Any())
      {
        throw new BLException(string.Format("No Employees are assigned to the Team '{0}', cannot use.", EmpID));
      }
    }
  }
}
