using System;
using System.Collections.Generic;
using System.Text;

namespace StockManager.Domain.Enums;

public enum StockMovementType
{
    PurchaseEntry = 1,   // ingreso/compra
    Sale = 2,            // venta
    Adjustment = 3,      // ajuste manual
    Shrinkage = 4        // merma/rotura
}

