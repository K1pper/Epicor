var updated = ds.UD101.Where(r => r.Updated()).FirstOrDefault();
var unchanged = ds.UD101.Where(r => r.Unchanged()).FirstOrDefault();


if (updated == null || unchanged == null) return;

// Collection times
// Check Date03 is null because it may be updated from Mandata
if (updated.CheckBox02 && !unchanged.CheckBox02 && updated.ShortChar07 == "")
{
    updated.ShortChar07 = DateTime.Now.ToString("dd/MM/yyyy hh:mm");
    callContextBpmData.ShortChar01 = "COLLECTION";
}

if (!updated.CheckBox02 && unchanged.CheckBox02)
{
    updated.ShortChar07 = string.Empty;
    callContextBpmData.ShortChar01 = "COLLECTION";
}

// Delivery times
// Check Date05 is null because it may be updated from Mandata
if (updated.CheckBox03 && !unchanged.CheckBox03 && updated.ShortChar08 == "")
{
    updated.ShortChar08 = DateTime.Now.ToString("dd/MM/yyyy hh:mm");
    callContextBpmData.ShortChar02 = "DELIVERY";
}

if (!updated.CheckBox03 && unchanged.CheckBox03 && updated.Date05 == null)
{
    updated.ShortChar08 = string.Empty;
    callContextBpmData.ShortChar02 = "DELIVERY";
}