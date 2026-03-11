foreach (var ttMI in (from row in ttMassIssue select row))
{
  var prt = (from row in Db.Part
    where row.Company == Session.CompanyID &&
    row.PartNum == ttMI.PartNum &&
    row.Paint_c != ""
    select row).FirstOrDefault();
  if (prt != null) 
  {
    ttMI.PartDescMS = prt.Paint_c;
  }
}