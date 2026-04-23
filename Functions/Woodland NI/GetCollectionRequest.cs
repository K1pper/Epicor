IsSuccess = true;
ErrorMessage = "";
Request = "";

string ColAddress1 = "";
string ColAddress2 = "";
string ColAddress3 = "";
string ColAddress4 = "";
string ColAddress5 = "";
string ColPostcode = "";
string ColCountry = "";
string ColPhoneNum = "";
string ColFaxNum = "";
string DelAddress1 = "";
string DelAddress2 = "";
string DelAddress3 = "";
string DelAddress4 = "";
string DelAddress5 = "";
string DelPostcode = "";
string DelCountry = "";
string DelPhoneNum = "";
string DelFaxNum = "";
List<string> OrderNumbers = new List<string>();
int NumberOfPallets = 0;
List<ExpandoObject> Pallets = new List<ExpandoObject>();
List<ExpandoObject> Goods = new List<ExpandoObject>();
string ShipComments = "";

var shipments = Db.UD101.Where(r => r.Company == Session.CompanyID).Where(r => r.Number01 == PackNum).ToList();


var shipment = Db.UD101.Where(r => r.Company == Session.CompanyID).Where(r => r.Number01 == PackNum).FirstOrDefault();
if (shipment == null) { IsSuccess = false; ErrorMessage = $"Master pack {PackNum} not found"; return; }

/* 
   Not sure what the collection address will be. There are two sites, so the obvious is to use the site address, but we may be using the 
   orderrel.site for moving stock to React locations
   Use shiphead.site for now, may need to use a UD field somewhere
*/



var site = Db.Plant.Where(r => r.Company == Session.CompanyID).Where(r => r.Plant1 == Session.PlantID).FirstOrDefault();
if (site == null) return;

var collectionCountry = Db.Country.Where(r => r.Company == Session.CompanyID).Where(r => r.CountryNum == site.CountryNum).FirstOrDefault();

ColAddress1 = site.Address1;
ColAddress2 = site.Address2;
ColAddress3 = site.Address3;
ColAddress4 = site.City;
ColPostcode = site.Zip;
ColCountry = collectionCountry == null ? string.Empty : collectionCountry.Description;

var shipTo = Db.ShipTo.Where(r => r.Company == Session.CompanyID).Where(r => r.CustNum == shipment.Number02).Where(r => r.ShipToNum == shipment.ShortChar03).FirstOrDefault();
if (shipTo == null) { IsSuccess = false; ErrorMessage = $"Ship to {shipment.ShortChar03} not found for master pack {PackNum}"; return; }

var deliveryCountry = Db.Country.Where(r => r.Company == Session.CompanyID).Where(r => r.CountryNum == shipTo.CountryNum).FirstOrDefault();

DelAddress1 = shipTo.Address1;
DelAddress2 = shipTo.Address2;
DelAddress3 = shipTo.Address3;
DelAddress4 = shipTo.City;
DelPostcode = shipTo.ZIP;
DelCountry = deliveryCountry == null ? string.Empty : deliveryCountry.Description;

if (IsSuccess)
{
    Pallets = new List<ExpandoObject>();

    var packs = Db.UD101A.Where(r => r.Company == Session.CompanyID).Where(r => r.Number01 == PackNum).ToList();

    foreach (var pack in packs.OrderBy(r => r.Number02))
    {
        var ship = Db.ShipHead.Where(r => r.Company == Session.CompanyID).Where(r => r.PackNum == pack.Number02).FirstOrDefault();
        if (ship == null) continue;
        if (ship.Pivot_c) continue;

        dynamic Pallet = new ExpandoObject();
        dynamic Good = new ExpandoObject();
        Pallet.height = (int)ship.PkgHeight;
        Pallet.length = (int)ship.PkgLength;
        Pallet.quantity = 1;
        Pallet.region = "";
        Pallet.stackable = false;
        Pallet.weight = (int)ship.Weight;
        Pallet.width = (int)ship.PkgWidth;

        Pallets.Add(Pallet);

        Good.line = 1;
        Good.description2 = ship.PackNum.ToString(); //Stock Code (full)
        if (ship.PalletSize_c > 1)
        {
            Good.description2 += " Long (" + ship.PalletSize_c.ToString() + "x)";
        }
        Good.palletType = ship.PackNum.ToString(); //Stock Code
        Good.stackFactor = 1;
        Good.description = ship.PackNum.ToString(); //Stock description
        Good.integer1 = ship.PalletSize_c;


        Goods.Add(Good);

        if (string.IsNullOrEmpty(ShipComments))
        {
            ShipComments = ship.ShipComment;
        }

        NumberOfPallets += 1;
    }


    if (NumberOfPallets == 0)
    {
        IsSuccess = false;
        ErrorMessage = $"No pallets added for master pack {PackNum}";
        return;
    }



    var data = new
    {
        data = new
        {
            account = Account,
            cTimeType = "DAY",
            dTimeType = "DAY",
            colDateTime = CollectionDate.HasValue ? CollectionDate.Value.ToString("yyyy-MM-ddThh:mm:ss+00:00") : "",
            delDateTime = DeliveryDate.HasValue ? DeliveryDate.Value.ToString("yyyy-MM-ddThh:mm:ss+00:00") : "",
            cDateType = "BY",
            dDateType = "BY",
            depot = "DUN",
            jobByDates = new
            {
                byColDate = CollectionDate.HasValue ? CollectionDate.Value.ToString("yyyy-MM-dd") : "",
                byDelDate = DeliveryDate.HasValue ? DeliveryDate.Value.ToString("yyyy-MM-dd") : "",
            },
            jobType = "DEL",
            delType = "12",
            workType = "STD",
            orderNo = PackNum, //String.Join(" ", OrderNumbers),
            custRef2 = "",
            custRef3 = "",
            chgMethod = "QUANTITY",
            weight = 0.0,
            volume = 0.0,
            quantity = NumberOfPallets,
            collAdd1 = ColAddress1,
            collAdd2 = ColAddress2,
            collAdd3 = ColAddress3,
            collAdd4 = ColAddress4,
            collAdd5 = ColAddress5,
            collPostCode = ColPostcode,
            collectCountry = ColCountry,
            collTelNo = ColPhoneNum,
            collFaxNo = ColFaxNum,
            delAdd1 = DelAddress1,
            delAdd2 = DelAddress2,
            delAdd3 = DelAddress3,
            delAdd4 = DelAddress4,
            delAdd5 = DelAddress5,
            delPostCode = DelPostcode,
            deliverCountry = DelCountry,
            delTelNo = DelPhoneNum,
            delFaxNo = DelFaxNum,
            jobSyncRules = new
            {
                quantity = "syncPalletDimsQuantity",
                volume = "syncPalletDimsSpaces",
                weight = "syncPalletDimsWeight"
            },
            jobThirdPartyRef = new[]
                {
                new
                {
                    systemName = "Epicor",
                    orderReference = PackNum
                }
            },
            jobPalletsDimensions = Pallets,
            extraGoods = Goods,
            notes = new
            {
                notes = ""
            },
            instructions = new
            {
                notes = ShipComments
            }
        }
    };

    Request = JsonConvert.SerializeObject(data);
}
