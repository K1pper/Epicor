MaxAmount = 0;
IsAllowed = false;
var FirstRep = true;

List<string> salesReps = QSalesRep.Split(',').ToList();

foreach (var rep in salesReps)
{
  var salesRep = Db.SalesRep.Where(r => r.Company == Session.CompanyID).Where(r => r.SalesRepCode == rep).FirstOrDefault();
  if (salesRep == null) return;
  var max = salesRep.GHA_AQS_MaximumDiscountPercentage_c;
  if (FirstRep)
  {
    MaxAmount = salesRep.GHA_AQS_MaximumDiscountPercentage_c;
  }
  else
  {
    if (max < MaxAmount)
    {
      MaxAmount = salesRep.GHA_AQS_MaximumDiscountPercentage_c;
    }
  }
  FirstRep = false;
  IsAllowed = true;
}