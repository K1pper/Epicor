if (callContextBpmData.ShortChar02 != "DELIVERY") return;

var deliveries = ds.UD101.ToList();


foreach (var delivery in deliveries)
{
    this.InvokeFunction("Ship-Mandata", "Delivered", Tuple.Create((int)delivery.Number01, delivery.CheckBox03));
}

callContextBpmData.ShortChar02 = string.Empty;