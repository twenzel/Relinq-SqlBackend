// This file is part of the re-motion Core Framework (www.re-motion.org)
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
using System.Linq;
using NUnit.Framework;
using Remotion.Data.Linq.Backend.SqlGeneration;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class SubQuerySqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void InWhereCondition_First ()
    {
      CheckQuery (
         from c in Cooks where c.Name == (from a in Cooks select a.FirstName).First() select c.Name,
         "SELECT [t0].[Name] FROM [CookTable] AS [t0] WHERE ([t0].[Name] = (SELECT TOP (@1) [t1].[FirstName] FROM [CookTable] AS [t1]))",
         new CommandParameter("@1", 1));
    }

    [Test]
    public void InWhereCondition_Single ()
    {
      CheckQuery (
         from c in Cooks where c.Name == (from k in Kitchens select k.Name).Single () select c.Name,
         "SELECT [t0].[Name] FROM [CookTable] AS [t0] WHERE ([t0].[Name] = (SELECT TOP (@1) [t1].[Name] FROM [KitchenTable] AS [t1]))",
         new CommandParameter ("@1", 1));
    }

    [Test]
    public void InWhereCondition_Count ()
    {
      CheckQuery (
         from c in Cooks where c.ID == (from k in Kitchens select k).Count() select c.Name,
         "SELECT [t0].[Name] FROM [CookTable] AS [t0] WHERE ([t0].[ID] = (SELECT COUNT(*) FROM [KitchenTable] AS [t1]))");
    }

    [Test]
    public void InSelectProjection_Count ()
    {
      CheckQuery (
          from c in Cooks select (from k in Kitchens select k.Name).Count(),
         "SELECT (SELECT COUNT(*) FROM [KitchenTable] AS [t1]) FROM [CookTable] AS [t0]");
    }

    [Test]
    public void SubQueryInMainFromClause ()
    {
      CheckQuery (from s in (from s2 in Cooks select s2).Take(1) select s,
        "SELECT [q0].[ID],[q0].[FirstName],[q0].[Name],[q0].[IsStarredCook],[q0].[IsFullTimeCook],[q0].[SubstitutedID],[q0].[KitchenID] "
        + "FROM "
        +"(SELECT TOP (@1) [t1].[ID],[t1].[FirstName],[t1].[Name],[t1].[IsStarredCook],[t1].[IsFullTimeCook],[t1].[SubstitutedID],[t1].[KitchenID] "
        +"FROM [CookTable] AS [t1]) AS [q0]",
        new CommandParameter("@1", 1));
    }

    [Test]
    public void SubQueryInAdditionalFromClause ()
    {
      CheckQuery (from s in Cooks from s2 in (from s3 in Cooks select s3) select s.FirstName,
        "SELECT [t1].[FirstName] FROM [CookTable] AS [t1] CROSS APPLY "
        +"(SELECT [t2].[ID],[t2].[FirstName],[t2].[Name],[t2].[IsStarredCook],[t2].[IsFullTimeCook],[t2].[SubstitutedID],[t2].[KitchenID] "
        +"FROM [CookTable] AS [t2]) AS [q0]");
    }

    [Test]
    public void ComplexSubQueryInAdditionalFromClause ()
    {
      CheckQuery (from s in Cooks from s2 in (from s3 in Cooks where s3.ID == s.ID && s3.ID > 3 select s3) select s2.FirstName, 
      "SELECT [q0].[FirstName] FROM [CookTable] AS [t1] CROSS APPLY "
      +"(SELECT [t2].[ID],[t2].[FirstName],[t2].[Name],[t2].[IsStarredCook],[t2].[IsFullTimeCook],[t2].[SubstitutedID],[t2].[KitchenID] "
      +"FROM [CookTable] AS [t2] "
      +"WHERE (([t2].[ID] = [t1].[ID]) AND ([t2].[ID] > @1))) AS [q0]",
      new CommandParameter("@1",3));
      
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Subquery selects a collection where a single value is expected.")]
    public void InSelectProjection_ThrowsNotSupportedException ()
    {
      CheckQuery (
          from c in Cooks select (from k in Kitchens select k.Name),
         "SELECT (SELECT [t1].[Name] FROM [KitchenTable] AS [t1]) FROM [CookTable] AS [t0]");
    }

    [Test]
    public void InOrderByClause ()
    {
      CheckQuery (
          from c in Cooks orderby (from k in Kitchens select k).Count() select c.Name,
         "SELECT [t0].[Name] FROM [CookTable] AS [t0] ORDER BY (SELECT COUNT(*) FROM [KitchenTable] AS [t1]) ASC");
    }

    [Test]
    public void SubQueryInSubQuery ()
    {
      CheckQuery (
          from c in Cooks where c.ID== (from k in Kitchens where k.ID == (from r in Restaurants select r).Count() select k).Count () select c.Name,
         "SELECT [t0].[Name] FROM [CookTable] AS [t0] "
        +"WHERE ([t0].[ID] = (SELECT COUNT(*) FROM [KitchenTable] AS [t1] WHERE ([t1].[ID] = (SELECT COUNT(*) FROM [RestaurantTable] AS [t2]))))");
    }

    [Test]
    [Ignore("TODO Review 2461: check from clauses - create dummy tables (to be discussed)")]
    public void DependentSubQueryInSubQuery ()
    {
      CheckQuery (
          from r in Restaurants where r.ID == (from c in r.Cooks where c.ID == (from a in c.Assistants select a).Count () select c).Count () select r.ID,
         "SELECT [t0].[ID] FROM [RestaurantTable] AS [t0] "
         +"LEFT OUTER JOIN [CookTable] AS [t1] ON [t0].[ID] = [t1].[RestaurantID] "
         +"LEFT OUTER JOIN [CookTable] AS [t2] ON [t1].[ID] = [t2].[AssistedID] "
         +"WHERE ([t0].[ID] = (SELECT COUNT(*) FROM  WHERE ([t1].[ID] = (SELECT COUNT(*) FROM ))))");
    }

    [Test]
    [Ignore("TODO: Subquery with Contains")]
    public void Contains ()
    {
      CheckQuery (
          from c in Cooks where (from k in Cooks select k.Substitution).Contains (c) select c.FirstName, 
          "SELECT [c].[FirstName] FROM [CookTable] AS [t0] WHERE @1 IN (SELECT [k].[ID] FROM [CookTable] AS [t1])"); //@1 = [c].[ID]
    }
  }
}