using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing
{
  public class QueryParser
  {
    private readonly List<FromLetWhereExpressionBase> _fromLetWhereExpressions = new List<FromLetWhereExpressionBase> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();
    private readonly List<OrderExpression> _orderExpressions = new List<OrderExpression>();

    public QueryParser (Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      MethodCallExpression rootExpression = ParserUtility.GetTypedExpression<MethodCallExpression> (expressionTreeRoot,
          "expression tree root", expressionTreeRoot);

      SourceExpression = expressionTreeRoot;

      switch (ParserUtility.CheckMethodCallExpression (rootExpression, expressionTreeRoot, "Select", "Where","SelectMany"))
      {
        case "Select":
          SelectExpressionParser selectExpressionParser = new SelectExpressionParser(rootExpression, expressionTreeRoot);
          _fromLetWhereExpressions.AddRange (selectExpressionParser.FromLetWhereExpressions);
          _projectionExpressions.AddRange (selectExpressionParser.ProjectionExpressions);
          break;
        case "Where":
          WhereExpressionParser whereExpressionParser = new WhereExpressionParser (rootExpression, expressionTreeRoot, true);
          _fromLetWhereExpressions.AddRange (whereExpressionParser.FromLetWhereExpressions);
          _projectionExpressions.AddRange (whereExpressionParser.ProjectionExpressions);
          break;
        case "SelectMany":
          SelectManyExpressionParser selectManyExpressionParser = new SelectManyExpressionParser (rootExpression, expressionTreeRoot);
          _fromLetWhereExpressions.AddRange (selectManyExpressionParser.FromLetWhereExpressions);
          _projectionExpressions.AddRange (selectManyExpressionParser.ProjectionExpressions);
          break;
      }
    }

    public Expression SourceExpression { get; private set; }

    public ReadOnlyCollection<FromLetWhereExpressionBase> FromLetWhereExpressions
    {
      get { return new ReadOnlyCollection<FromLetWhereExpressionBase> (_fromLetWhereExpressions); }
    }

    public ReadOnlyCollection<LambdaExpression> ProjectionExpressions
    {
      get { return new ReadOnlyCollection<LambdaExpression> (_projectionExpressions); }
    }

    public ReadOnlyCollection<OrderExpression> OrderExpressions
    {
      get { return new ReadOnlyCollection<OrderExpression> (_orderExpressions); }
    }

    public QueryExpression GetParsedQuery ()
    {
      MainFromClause mainFromClause = CreateMainFromClause();
      QueryBody queryBody = CreateQueryBody(mainFromClause);

      return new QueryExpression (mainFromClause, queryBody, SourceExpression);
    }

    private MainFromClause CreateMainFromClause ()
    {
      FromExpression mainFromExpression = (FromExpression) _fromLetWhereExpressions[0];
      return new MainFromClause (mainFromExpression.Identifier,
          (IQueryable) ((ConstantExpression) mainFromExpression.Expression).Value);
    }

    private QueryBody CreateQueryBody (MainFromClause mainFromClause)
    {
      List<IFromLetWhereClause> fromLetWhereClauses = new List<IFromLetWhereClause> ();
      IClause previousClause = mainFromClause;

      int currentProjection = 0;
      for (int currentFromLetWhere = 1; currentFromLetWhere < _fromLetWhereExpressions.Count; currentFromLetWhere++)
      {
        IFromLetWhereClause fromLetWhereClause = CreateFromLetWhereClause (_fromLetWhereExpressions[currentFromLetWhere], previousClause, ref currentProjection);
        fromLetWhereClauses.Add (fromLetWhereClause);
        previousClause = fromLetWhereClause;
      }

      SelectClause selectClause = new SelectClause (previousClause, _projectionExpressions.Last ());
      QueryBody queryBody = new QueryBody (selectClause);

      foreach (IFromLetWhereClause fromLetWhereClause in fromLetWhereClauses)
        queryBody.Add (fromLetWhereClause);

      // TODO: translate OrderExpressions into queryBody.OrderBy clause

      return queryBody;
    }

    private IFromLetWhereClause CreateFromLetWhereClause (FromLetWhereExpressionBase expression, IClause previousClause, ref int currentProjection)
    {
      FromExpression fromExpression = expression as FromExpression;
      if (fromExpression != null)
      {
        AdditionalFromClause additionalFromClause = new AdditionalFromClause (previousClause, fromExpression.Identifier,
            (LambdaExpression) fromExpression.Expression, _projectionExpressions[currentProjection]);
        ++currentProjection;
        return additionalFromClause;
      }

      WhereExpression whereExpression = expression as WhereExpression;
      if (whereExpression != null)
      {
        WhereClause whereClause = new WhereClause (previousClause, whereExpression.Expression);
        return whereClause;
      }

      throw new NotSupportedException ("The FromLetWhereExpression type " + expression.GetType().Name + " is not supported.");
    }
  }
}