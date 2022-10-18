public IEnumerable<Dto_ShipOrderList> GetShipOrderLists(SalesQueryParameter parameter)
{
      var filterExpr = Expr.CreateMyExpression<Dto_ShipOrderList>()
                .And(Expr.Equal, nameof(Dto_ShipOrderList.Status), "C")
                .MaybeAnd(Expr.GreaterThanOrEqual, nameof(Dto_ShipOrderList.ShipDate), parameter.StartDate)
                .MaybeAnd(Expr.LessThanOrEqual, nameof(Dto_ShipOrderList.ShipDate), parameter.EndDate)
                .MaybeAnd(Expr.Equal, nameof(Dto_ShipOrderList.CustomerID), parameter.CustomerID)
                .MaybeAnd(Expr.Equal, nameof(Dto_ShipOrderList.SalesPersonID), parameter.SalesPersonID)
                .MaybeAnd(Expr.Equal, nameof(Dto_ShipOrderList.SalesTeamID), parameter.SalesTeamID)
                .MaybeAnd(Expr.StartsWith, nameof(Dto_ShipOrderList.ShipType), parameter.ShipType)
                .MaybeAnd(Expr.Equal, nameof(Dto_ShipOrderList.IsShipViaSalesPerson), parameter.IsShipViaSalesPerson)

                .MaybeAnd(Expr.GreaterThanOrEqual, nameof(Dto_ShipOrderList.SalesPersonID), parameter.StartSalesPersonID)
                .MaybeAnd(Expr.LessThanOrEqual, nameof(Dto_ShipOrderList.SalesPersonID), parameter.EndSalesPersonID)
                .MaybeAnd(parameter.ShipTypeOption == ShipTypeOptions.Exclude_ShipToStore, Expr.NotEqual, nameof(Dto_ShipOrderList.ShipType), "3")
                .MaybeAnd(parameter.ShipTypeOption == ShipTypeOptions.Exclude_Replacement, Expr.NotEqual, nameof(Dto_ShipOrderList.ShipType), "4");

      return MyUsing(ContextHelper.GetContext(),
                     context => context.Set<Dto_ShipOrderList>()
                                  .Where(filterExpr.GetLambda())
                                  .OrderBy(x => x.ShipID)
                                  .ToList());
}

public void Test()
{
      var myExpr = Expr.CreateMyExpression<Customer>("x");
      var filter = myExpr.And(Expr.GreaterThan, nameof(Customer.CustomerID), "C0000001")
                         .And(Expr.Like, nameof(Customer.CustomerID), "%700%")
                         .And(Expr.StartsWith, nameof(Customer.CustomerID), "H")
                         .And(Expr.GreaterThan, nameof(Customer.PayDay), (byte)20)
                         .MaybeAnd(myExpr.Or(Expr.Contains, nameof(Customer.CustomerName), "醫院")
                                         .Or(Expr.Contains, nameof(Customer.CustomerName), "診所")
                                         .BodyExpr)
                         .GetLambda();
}
