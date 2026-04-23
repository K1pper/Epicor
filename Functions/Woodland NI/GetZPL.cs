bool isOk = true;
zplResponse = string.Empty;
StringBuilder zpl = new StringBuilder();
var shipHead = Db.ShipHead.Where(r => r.Company == Session.CompanyID).Where(r => r.PackNum == PackNum).FirstOrDefault();
if (shipHead == null)
{
    ErrorMessage = "Ship header not found";
    isOk = false;
}
var customer = Db.Customer.Where(r => r.Company == Session.CompanyID).Where(r => r.CustNum == shipHead.ShipToCustNum).FirstOrDefault();
if (customer == null)
{
    ErrorMessage = "Customer not found";
    isOk = false;
}

var shipTo = Db.ShipTo.Where(r => r.Company == Session.CompanyID).Where(r => r.CustNum == shipHead.ShipToCustNum).Where(r => r.ShipToNum == shipHead.ShipToNum).FirstOrDefault();
if (shipTo == null)
{
    ErrorMessage = "Ship to not found";
    isOk = false;
}

if (isOk)
{
    int y = 610;

    zpl.Append("^XA");
    zpl.Append("^PW1174");
    zpl.Append("^LL1670");
    zpl.Append("^CF0,30");

    zpl.Append("^BY3,2,220");
    zpl.Append("^FO95,120");
    zpl.Append("^BCN,220,Y,N,N");
    zpl.Append($"^FD{shipHead.Barcode_c}^FS");

    zpl.Append("^FO95,430");
    zpl.Append("^A0N,65,65");
    zpl.Append($"^FD{customer.Name.Substring(0, 35)}^FS");

    if (!string.IsNullOrEmpty(shipTo.Address1))
    {
        zpl.Append($"^FO95,{y}");
        zpl.Append("^A0N,50,50");
        zpl.Append("^FD");
        zpl.Append($"{shipTo.Address1}");
        zpl.Append("^FS");
        y += 80;
    }

    if (!string.IsNullOrEmpty(shipTo.Address2))
    {
        zpl.Append($"^FO95,{y}");
        zpl.Append("^A0N,50,50");
        zpl.Append("^FD");
        zpl.Append($"{shipTo.Address2}");
        zpl.Append("^FS");
        y += 80;
    }

    if (!string.IsNullOrEmpty(shipTo.Address3))
    {
        zpl.Append($"^FO95,{y}");
        zpl.Append("^A0N,50,50");
        zpl.Append("^FD");
        zpl.Append($"{shipTo.Address3}");
        zpl.Append("^FS");
        y += 80;
    }

    if (!string.IsNullOrEmpty(shipTo.City))
    {
        zpl.Append($"^FO95,{y}");
        zpl.Append("^A0N,50,50");
        zpl.Append("^FD");
        zpl.Append($"{shipTo.City}");
        zpl.Append("^FS");
        y += 80;
    }

    if (!string.IsNullOrEmpty(shipTo.State))
    {
        zpl.Append($"^FO95,{y}");
        zpl.Append("^A0N,50,50");
        zpl.Append("^FD");
        zpl.Append($"{shipTo.State}");
        zpl.Append("^FS");
        y += 80;
    }

    if (!string.IsNullOrEmpty(shipTo.ZIP))
    {
        zpl.Append($"^FO95,{y}");
        zpl.Append("^A0N,50,50");
        zpl.Append("^FD");
        zpl.Append($"{shipTo.ZIP}");
        zpl.Append("^FS");
        y += 80;
    }

    zpl.Append("^FO95,1450");
    zpl.Append("^A0N,60,60");
    zpl.Append($"^FD{shipHead.PalletNum_c}^FS");

    zpl.Append("^FO700,1450");
    zpl.Append("^A0N,60,60");
    zpl.Append($"^FD{shipHead.PackNum}^FS");

    zpl.Append("^FO700,1530");
    zpl.Append("^A0N,60,60");
    zpl.Append($"^FD{shipHead.PackNumber_c}^FS");

    zpl.Append("^XZ");

}

zplResponse = zpl.ToString();
