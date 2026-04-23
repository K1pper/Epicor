if (callContextBpmData.ShortChar01 != "COLLECTION") return;

var collections = ds.UD101.ToList();


foreach (var collection in collections)
{
    var result = this.InvokeFunction("Ship-Mandata", "Collected", Tuple.Create((int)collection.Number01, collection.CheckBox02));

    if (!string.IsNullOrEmpty(result[0].ToString()))
    {

        InfoMessage.Publish(result[0].ToString());
    }


}


callContextBpmData.ShortChar01 = string.Empty;