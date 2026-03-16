var data = new
{
    data = new
    {
        account = "JTC01",
        cTimeType = "DAY",
        dTimeType = "DAY",
        colDateTime = DateTimeOffset.Parse("2026-03-05T00:01:00+01:00"),
        delDateTime = DateTimeOffset.Parse("2026-03-15T12:00:00+01:00"),
        cDateType = "BY",
        dDateType = "BY",
        jobByDates = new
        {
            byColDate = "2026-03-05",
            byDelDate = "2026-03-15"
        },
        jobType = "DEL",
        delType = "12",
        workType = "STD",
        orderNo = "TEST ORDER NO",
        custRef2 = "",
        custRef3 = "",
        chgMethod = "QUANTITY",
        weight = 0.0,
        volume = 0.0,
        quantity = 3.0,
        collAdd1 = "Collection Address Line 1",
        collAdd2 = "CollAdd2",
        collAdd3 = "CollAdd3",
        collAdd4 = "CollAdd4",
        collAdd5 = "",
        collPostCode = "WD18 8YE",
        collectCountry = "",
        collTelNo = "",
        collFaxNo = "",
        delAdd1 = "ROYAL SURREY",
        delAdd2 = "ORTHOPAEDIC DEPARTMENT",
        delAdd3 = "EGERTON ROAD",
        delAdd4 = "",
        delAdd5 = "GUILDFORD",
        delPostCode = "GU2 7XX",
        deliverCountry = "",
        delTelNo = "",
        delFaxNo = "",
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
                    systemName = "YOUR_SYSTEM_NAME",
                    orderReference = "TEST_UNIQUE_REF_123456789"
                }
            },
        extras = new[]
            {
                new
                {
                    fieldName = "CollectBy",
                    fieldValue = "05/03/2026"
                },
                new
                {
                    fieldName = "DeliveryBy",
                    fieldValue = "15/03/2026"
                }
            },
        jobPalletsDimensions = new[]
            {
                new
                {
                    height = 1000,
                    length = 1000,
                    quantity = 1,
                    region = "TEST",
                    stackable = false,
                    weight = 250,
                    width = 1000
                },
                new
                {
                    height = 1000,
                    length = 1200,
                    quantity = 2,
                    region = "TEST",
                    stackable = false,
                    weight = 500,
                    width = 1000
                }
            },
        notes = new
        {
            notes = "Some notes 1x 1000mm x 1000mm x 250kg\n2x1000mm x 1200mm x 500kg"
        },
        instructions = new
        {
            notes = "please use a small van for access"
        }
    }
};

Request = JsonConvert.SerializeObject(data);