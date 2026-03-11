/* Convert production standard to minutes for operations */

decimal basis = 0m;

switch (StdBasis)
{
  case "E":
    basis = 1;
    break;
  case "C":
    basis = 100;
    break;
  case "M":
    basis = 1000;
    break;
  case "T":
    basis = 10000;
    break;
  default:
    basis = 1;
    break;
}

if (ProdStandard == 0) {
  TaskTime = 0;
  return;
 }

switch (StdFormat) {
  case "HR":
    TaskTime = (int)(60 * ProdStandard);
    break;
  case "PH":
    TaskTime = (int)(60 * RunQty / ProdStandard);
    break;
  case "PM":
    TaskTime = (int)(RunQty / ProdStandard);
    break;
  case "OH":
    TaskTime = (int)(60 * (RunQty / OpsPerPart) / ProdStandard);
    break;
  case "OM":
    TaskTime = (int)((RunQty / OpsPerPart) / ProdStandard);
    break;
  case "CH":
    TaskTime = (int)(60 * (RunQty / QtyPerCycle) / ProdStandard);
    break;
  case "CM":
    TaskTime = (int)((RunQty / QtyPerCycle) / ProdStandard);
    break;
  case "HP":
    TaskTime = (int)(60 * (RunQty / basis) * ProdStandard);
    break;
  case "MP":
    TaskTime = (int)((RunQty / basis) * ProdStandard);
    break;
  default:
    TaskTime = 0;
    break;
}

