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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlSubStatementExpression"/> represents a SQL database subquery. The <see cref="QueryModel"/> of the subquery is translated to 
  /// this model, and the <see cref="SqlSubStatementExpression"/> is transformed several times until it can easily be translated to SQL text.
  /// </summary>
  public class SqlSubStatementExpression : ExtensionExpression
  {
    private readonly SqlStatement _sqlStatement;

    public SqlSubStatementExpression (SqlStatement sqlStatement)
        : base (sqlStatement.DataInfo.DataType)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);

      _sqlStatement = sqlStatement;
    }

    public SqlStatement SqlStatement
    {
      get { return _sqlStatement; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as ISqlSubStatementVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlSubStatementExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return "(" + _sqlStatement + ")";
    }
    
    public SqlTable ConvertToSqlTable (string uniqueIdentifier)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("uniqueIdentifier", uniqueIdentifier);
      ArgumentUtility.CheckNotNullOrEmpty ("uniqueIdentifier", uniqueIdentifier);
      
      var joinSemantic = CalculateJoinSemantic();

      SqlStatement sequenceStatement;
      if (SqlStatement.DataInfo is StreamedSequenceInfo)
        sequenceStatement = SqlStatement;
      else
        sequenceStatement = ConvertValueStatementToSequenceStatement ();

      var resolvedSubStatementTableInfo = new ResolvedSubStatementTableInfo (uniqueIdentifier, sequenceStatement);
      return new SqlTable (resolvedSubStatementTableInfo, joinSemantic);
    }

    private JoinSemantics CalculateJoinSemantic ()
    {
      var dataInfoAsStreamedSingleValueInfo = SqlStatement.DataInfo as StreamedSingleValueInfo;
      if (dataInfoAsStreamedSingleValueInfo != null && dataInfoAsStreamedSingleValueInfo.ReturnDefaultWhenEmpty)
        return JoinSemantics.Left;
      else
        return JoinSemantics.Inner;
    }

    private SqlStatement ConvertValueStatementToSequenceStatement ()
    {
      var newDataInfo = new StreamedSequenceInfo (typeof (IEnumerable<>).MakeGenericType (SqlStatement.DataInfo.DataType), SqlStatement.SelectProjection);

      var adjustedStatementBuilder = new SqlStatementBuilder (SqlStatement) { DataInfo = newDataInfo };
      // TODO Review 3100: I don't really like the StreamedForcedSingleValueInfo approach, it was a solution, but not really the best solution.
      // TODO Review 3100: Change as follows: Check for StreamedSingleValueInfo instead; always set the TopExpression to new SqlLiteralExpression (1). We can do this because we know that the sequence was expected to return a single value anyway.
      // TODO Review 3100: All tests (esp. integration tests) should still work. Then, remove StreamedForcedSingleValueInfo. In SingleResultOperatorHandler, use UpdateDataInfo instead.
      if (SqlStatement.DataInfo is StreamedForcedSingleValueInfo)
      {
        Debug.Assert (
            adjustedStatementBuilder.TopExpression is SqlLiteralExpression
            && ((SqlLiteralExpression) adjustedStatementBuilder.TopExpression).Value.Equals (2));
        // A sub-statement might use a different TopExpression than 1 (or none at all) in order to provoke a SQL error when more than one item is 
        // returned. When we convert the statement to a sequence statement, however, we must ensure that the exact "only 1 value is returned" 
        // semantics is ensured because we can't provoke a SQL error (but instead would return strange result sets).
        adjustedStatementBuilder.TopExpression = new SqlLiteralExpression (1);
      }

      return adjustedStatementBuilder.GetSqlStatement();
    }
  }
}