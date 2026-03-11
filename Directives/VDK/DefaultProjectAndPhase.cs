//assignment of the project is done as a preprocessor
//the job number validation in the method fails for a project job if the project has not been set
var ttLaborDtl = ds.LaborDtl.Where(x => x.Added()).FirstOrDefault();

//Set the related Project and Phase, if found.
if (ttLaborDtl != null && ttLaborDtl.LaborCollection)
{
    var jobHead = (from row in Db.JobHead where row.Company == Session.CompanyID &&
    row.JobNum == jobNum select row).FirstOrDefault();
    
    if (jobHead != null && jobHead.JobType.Equals("PRJ", StringComparison.OrdinalIgnoreCase))
    {
        ttLaborDtl.ProjectID = jobHead.ProjectID;
        ttLaborDtl.PhaseID = jobHead.PhaseID;
    }
}
