var added = ttUD101A.Where(r => r.Added()).FirstOrDefault();

var deleted = ttUD101A.Where(r => r.Deleted()).FirstOrDefault();

if (added == null && deleted == null) return;

Ice.Diagnostics.Log.WriteEntry("BPM OK");

var ud101A = ttUD101A.FirstOrDefault();

var pack = Db.UD101.Where(r => r.Company == Session.CompanyID).Where(r => r.Number01 == ud101A.Number01).FirstOrDefault();
var pallet = Db.ShipHead.Where(r => r.Company == Session.CompanyID).Where(r => r.PackNum == ud101A.Number02).FirstOrDefault();

if (pack == null) return;
if (pallet == null) return;

Ice.Diagnostics.Log.WriteEntry("Pack and pallet OK");

var jobNumber = pack.UDField<string>("JobNumber_c");
if (string.IsNullOrEmpty(jobNumber)) return;
Ice.Diagnostics.Log.WriteEntry("Job OK");
string palletNum = "";
if (added != null) palletNum = added.Number02.ToString();
if (deleted != null) palletNum = deleted.Number02.ToString();
Ice.Diagnostics.Log.WriteEntry("Pallet num OK");
if (string.IsNullOrEmpty(palletNum)) return;

var palletSize = pallet.UDField<System.Int32>("PalletSize_c");

Ice.Diagnostics.Log.WriteEntry("Ready");

if (added != null)
{
    Ice.Diagnostics.Log.WriteEntry("Added");
    this.InvokeFunction("Ship-Mandata", "AddGoods", Tuple.Create(jobNumber, palletNum, palletSize));
}

if (deleted != null)
{
    Ice.Diagnostics.Log.WriteEntry("Deleted");
    this.InvokeFunction("Ship-Mandata", "DeleteGoods", Tuple.Create(jobNumber, palletNum));
}