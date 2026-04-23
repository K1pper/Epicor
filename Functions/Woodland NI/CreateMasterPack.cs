string CustID = "";
string ShipToNum = "";
string ShipVia = "";
double CustNum = 0;
DateTime CollectionDate = DateTime.Today;
DateTime DeliveryDate = DateTime.Today;

try
{
    if (input.Tables.Count > 0)
    {
        var NumRows = 0;
        bool DuplicateShipTo = false;
        List<KeyValuePair<double, string>> Packs = new List<KeyValuePair<double, string>>();

        foreach (DataRow r in input.Tables[0].Rows)
        {
            if ((bool)r["Calculated_Add"] == false) continue;

            var tCustID = r["Customer_CustID"].ToString();
            var tShipToNum = r["ShipDtl_ShipToNum"].ToString();

            var tPackNum = (double)r["ShipHead_PackNum"];

            var ShipHead = Db.ShipHead.Where(t => t.Company == Session.CompanyID).Where(t => t.PackNum == tPackNum).FirstOrDefault();

            ShipVia = ShipHead.ShipViaCode;
            CustNum = ShipHead.CustNum;


            var rCollectionDate = r["Calculated_ShipBy"]?.ToString();

            if (!string.IsNullOrWhiteSpace(rCollectionDate) &&
              DateTime.TryParse(rCollectionDate, out var tCollectionDate))
            {
                CollectionDate = tCollectionDate < CollectionDate ? tCollectionDate : CollectionDate;
            }

            var rDeliveryDate = r["Calculated_NeedBy"]?.ToString();

            if (!string.IsNullOrWhiteSpace(rDeliveryDate) &&
              DateTime.TryParse(rDeliveryDate, out var tDeliveryDate))
            {
                DeliveryDate = tDeliveryDate < DeliveryDate ? tDeliveryDate : CollectionDate;
            }


            if (!string.IsNullOrEmpty(CustID))
            {
                if (string.Compare(tCustID, CustID, true) != 0 || string.Compare(tShipToNum, ShipToNum, true) != 0)
                {
                    DuplicateShipTo = true;
                    break;
                }
            }
            CustID = tCustID;
            ShipToNum = tShipToNum;

            Packs.Add(new KeyValuePair<double, string>((double)r["ShipHead_PackNum"], r["ShipHead_PalletNum_c"].ToString()));


            NumRows++;
        }


        if (NumRows == 0)
        {
            Result = "No packs have been selected";
        }
        else if (DuplicateShipTo)
        {
            Result = "Master packs can only be created from packs with the same ship to address, please check";
        }
        else
        {

            var shipmentNum = Db.UD101
              .Where(r => r.Number01 != null)
              .Select(r => r.Number01)
              .DefaultIfEmpty(0m)
              .Max();

            shipmentNum++;


            this.CallService<Ice.Contracts.UD101SvcContract>(ud101Svc =>
            {
                try
                {
                    var ud101Ts = new Ice.Tablesets.UD101Tableset();
                    ud101Svc.GetaNewUD101(ref ud101Ts);
                    var ud101 = ud101Ts.UD101.Where(r => r.RowMod == "A").FirstOrDefault();

                    ud101.Key1 = ((int)shipmentNum).ToString();
                    ud101.Key2 = "";
                    ud101.Key3 = "";
                    ud101.Key4 = "";
                    ud101.Key5 = "";
                    ud101.Number01 = shipmentNum;
                    ud101.Number02 = (decimal)CustNum;
                    ud101.ShortChar02 = ShipVia;
                    ud101.ShortChar03 = ShipToNum;
                    ud101.Date01 = DateTime.Now;
                    ud101.Date03 = CollectionDate;
                    ud101.Date04 = DeliveryDate;
                    ud101Svc.Update(ref ud101Ts);

                    foreach (var pack in Packs)
                    {
                        ud101Svc.GetaNewUD101A(ref ud101Ts, ((int)shipmentNum).ToString(), "", "", "", "");
                        var ud101a = ud101Ts.UD101A.Where(r => r.RowMod == "A").FirstOrDefault();
                        ud101a.ChildKey1 = ((int)pack.Key).ToString();
                        ud101a.ChildKey2 = "";
                        ud101a.ChildKey3 = "";
                        ud101a.ChildKey4 = "";
                        ud101a.ChildKey5 = "";
                        ud101a.Number01 = shipmentNum;
                        ud101a.Number02 = (decimal)pack.Key;
                        ud101a.ShortChar01 = pack.Value;
                        ud101Svc.Update(ref ud101Ts);

                    }
                }
                catch (Exception ex)
                {
                    Result = $"Error in Create Master Pack() function (Create master pack): {ex}";
                    Ice.Diagnostics.Log.WriteEntry(Result);
                    Ice.Diagnostics.Log.WriteEntry(ex.StackTrace);
                    throw new Ice.Common.BusinessObjectException(Result);
                    return;
                }
            });


            Result = $"Shipment {(int)shipmentNum} created";
        }
    }
}
catch (Exception ex)
{
    Ice.Diagnostics.Log.WriteEntry($"Create Master Pack error: {ex.Message}");
    Result = $"Error: {ex.StackTrace}";
}
