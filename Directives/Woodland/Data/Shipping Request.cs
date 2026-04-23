var shipments = ttUD101.Where(r => r.Updated()).ToList();


foreach (var shipment in shipments)
{
    var unchanged = ttUD101.Where(r => r.Number01 == shipment.Number01).Where(r => r.Unchanged()).FirstOrDefault();
    if (shipment.CheckBox01 == unchanged.CheckBox01) continue;

    if (shipment.ShortChar02 != "REA" && shipment.CheckBox01) continue;

    if (shipment.CheckBox01 && (shipment.Date02 == null || string.IsNullOrEmpty(shipment.ShortChar04)))
    {
        InfoMessage.Publish($"Pallet {shipment.Key1}: You need a valid collection date and time.");
        shipment.CheckBox01 = false;
        Db.Validate();
        Db.SaveChanges();
        continue;
    }

    if (shipment.CheckBox01 && (shipment.Date04 == null))
    {
        InfoMessage.Publish($"Pallet {shipment.Key1}: You need a valid delivery date.");
        shipment.CheckBox01 = false;
        Db.Validate();
        Db.SaveChanges();
        continue;
    }

    var requestDate = shipment.Date02;

    var udCode = Db.UDCodes.Where(r => r.Company == Session.CompanyID).Where(r => r.CodeTypeID == "CollTimes").Where(r => r.CodeID == shipment.ShortChar04).FirstOrDefault();

    var requestTime = udCode != null ? udCode.CodeDesc : "";

    if (requestDate.HasValue)
    {
        TimeSpan time;
        if (TimeSpan.TryParse(requestTime, out time))
        {
            requestDate = new DateTime(
              requestDate.Value.Year,
              requestDate.Value.Month,
              requestDate.Value.Day
            ).Add(time);
        }
    }

    var response = this.InvokeFunction("Ship-Mandata", "Collection", Tuple.Create((int)shipment.Number01, shipment.CheckBox01, requestDate, shipment.Date04));

    shipment.ShortChar05 = shipment.CheckBox01 ? response[2].ToString() : "";

    var pallets = Db.UD101A.Where(r => r.Company == Session.CompanyID).Where(r => r.Key1 == shipment.Key1).ToList();

    string[] barcodes = response[3].ToString().Split(',');

    int count = 0;

    foreach (var pallet in pallets)
    {
        var shipHead = Db.ShipHead.Where(r => r.Company == Session.CompanyID).Where(r => r.PackNum == pallet.Number02).FirstOrDefault();

        if (shipHead == null)
        {
            continue;
        }

        if (shipment.CheckBox01)
        {
            shipHead.SetUDField("Barcode_c", barcodes[count++]);
        }
        else
        {
            shipHead.SetUDField("Barcode_c", "");
        }

        Db.Validate();
        Db.SaveChanges();
    }
}

