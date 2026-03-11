callContextBpmData.Character01 = "";
callContextBpmData.Checkbox01 = false;

var partNum = (from tt in ttPODetail where (tt.RowMod == "A" || tt.RowMod == "U") select tt.PartNum).FirstOrDefault().ToString();

foreach(var memo in (from tt in Db.UD05 where tt.Company == Session.CompanyID && tt.Key1 == "Part" && tt.Key2 == partNum && tt.CheckBox01 && (tt.Date01 == null || tt.Date01 <= DateTime.Today) && (tt.Date02 == null || tt.Date02 >= DateTime.Today) select tt))
{
  callContextBpmData.Checkbox01 = true;
  callContextBpmData.Character01 += "Memo Code: " + memo.ShortChar01
                                  + Environment.NewLine + memo.Character01 + Environment.NewLine;
}