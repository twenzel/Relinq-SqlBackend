﻿// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Linq.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.Development.UnitTesting
{
  /// <summary>
  /// Compares two SQL <see cref="Expression"/> trees constructed from <b>System.Linq</b> expressions.
  /// </summary>
  public sealed class SqlExpressionTreeComparer : ExpressionTreeComparerBase
  {
    public static void CheckAreEqualTrees (Expression expectedTree, Expression actualTree)
    {
      ArgumentUtility.CheckNotNull ("expectedTree", expectedTree);
      ArgumentUtility.CheckNotNull ("actualTree", actualTree);

      var comparer = new SqlExpressionTreeComparer (expectedTree.ToString(), actualTree.ToString());
      comparer.CheckAreEqualNodes (expectedTree, actualTree);
    }

    private SqlExpressionTreeComparer (string expectedInitial, string actualInitial)
        : base (expectedInitial, actualInitial, typeof (SqlCaseExpression.CaseWhenPair))
    {
    }
  }
}