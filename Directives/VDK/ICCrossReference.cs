foreach(var dtl in (from tt in ds.OrderDtl where tt.RowMod == "U" || tt.RowMod == "A" select tt))
{
  var cust = (from tt in Db.Customer where tt.Company == dtl.Company && tt.CustNum == dtl.CustNum select tt.ICCust).FirstOrDefault();
  
  if(cust) //this is an IC customer
  {
    //see if there is a cross reference
    var ud08 = (from tt in Db.UD08 where tt.Company == dtl.Company && tt.Key1 == "ICPRODXREF" && tt.Key2 == dtl.ProdCode select tt).FirstOrDefault();
    
    if(ud08 != null)
      dtl.ProdCode = ud08.Character01;
  }
}