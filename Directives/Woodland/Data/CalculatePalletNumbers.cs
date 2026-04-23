var pallet = ttUD101A.FirstOrDefault();

var pallets = Db.UD101A.Where(r => r.Company == pallet.Company).Where(r => r.Number01 == pallet.Number01).ToList().OrderBy(r => r.Number02);

var numPallets = pallets.Count();
int n = 1;

foreach (var p in pallets)
{
    var shipHead = Db.ShipHead.Where(r => r.Company == p.Company).Where(r => r.PackNum == p.Number02).FirstOrDefault();
    if (shipHead == null) continue;

    shipHead.PackNumber_c = $"Pallet {n} of {numPallets}";
    Db.SaveChanges();

    n++;
}