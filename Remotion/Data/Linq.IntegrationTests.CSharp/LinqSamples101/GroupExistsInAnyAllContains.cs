﻿// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  class GroupExistsInAnyAllContains:TestBase
  {

    //This sample uses Any to return only Customers that have no Orders.")]
    public void LinqToSqlExists01 ()
    {
      var q =
          from c in DB.Customers
          where !c.Orders.Any ()
          select c;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    //This sample uses Any to return only Categories that have at least one Discontinued product.")]
    public void LinqToSqlExists02 ()
    {
      var q =
          from c in DB.Categories
          where c.Products.Any (p => p.Discontinued)
          select c;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    //This sample uses All to return Customers whom all of their orders have been shipped to their own city or whom have no orders.")]
    public void LinqToSqlExists03 ()
    {
      var q =
          from c in DB.Customers
          where c.Orders.All (o => o.ShipCity == c.City)
          select c;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    //This sample uses Contain to find which Customer contains an order with OrderID 10248.")]
    public void LinqToSqlExists04 ()
    {
      var order = (from o in DB.Orders
                   where o.OrderID == 10248
                   select o).First ();

      var q = DB.Customers.Where (p => p.Orders.Contains (order)).ToList ();

      List<String> strings = new List<String>();
      foreach (var cust in q)
      {
        foreach (var ord in cust.Orders)
        {
          strings.Add (String.Format ("Customer {0} has OrderID {1}.", cust.CustomerID, ord.OrderID));
        }
      }
    }

    //This sample uses Contains to find customers whose city is Seattle, London, Paris or Vancouver.")]
    public void LinqToSqlExists05 ()
    {
      string[] cities = new string[] { "Seattle", "London", "Vancouver", "Paris" };
      var q = DB.Customers.Where (p => cities.Contains (p.City)).ToList ();

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
  }
}
