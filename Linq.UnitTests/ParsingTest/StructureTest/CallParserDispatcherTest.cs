using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.Parsing.Structure;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest
{
  [TestFixture]
  public class CallParserDispatcherTest
  {
    private CallParserDispatcher _dispatcher;
    private IQueryable<Student> _source;
    private MethodInfo _method;
    private Expression<Func<object>> _wrappedCall;
    private Action<ParseResultCollector, Expression> _emptyParser;

    [SetUp]
    public void SetUp ()
    {
      _dispatcher = new CallParserDispatcher ();
      _source = ExpressionHelper.CreateQuerySource ();
      _wrappedCall = (() => _source.Select (s => s.First));
      _method = ExpressionHelper.GetMethod (_wrappedCall);
      _emptyParser = delegate { };
    }

    [Test]
    public void CanParse_True()
    {
      _dispatcher.RegisterParser ("Select", _emptyParser);
      Assert.IsTrue (_dispatcher.CanParse (_method));
    }

    [Test]
    public void CanParse_False ()
    {
      Assert.IsFalse (_dispatcher.CanParse (_method));
    }

    [Test]
    public void GetParser_Registered ()
    {
      Action<ParseResultCollector, Expression> parser = _emptyParser;
      _dispatcher.RegisterParser ("Select", parser);

      Assert.That (_dispatcher.GetParser (_method), Is.SameAs (parser));
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected OrderBy, Where for dispatching a method call to a parser, found Select.")]
    public void GetParser_Unregistered ()
    {
      _dispatcher.RegisterParser ("OrderBy", _emptyParser);
      _dispatcher.RegisterParser ("Where", _emptyParser);
      
      _dispatcher.GetParser (_method);
    }

    [Test]
    public void Dispatch_Registered ()
    {
      ParseResultCollector actualCollector = null;
      Expression actualExpression = null;

      _dispatcher.RegisterParser ("Select", (collector, expression) => { actualCollector = collector; actualExpression = expression; });

      ParseResultCollector expectedCollector = new ParseResultCollector (Expression.Constant (0));
      MethodCallExpression expectedExpression = (MethodCallExpression) _wrappedCall.Body;
      
      _dispatcher.Dispatch (expectedCollector, expectedExpression, null);

      Assert.That (actualCollector, Is.SameAs (expectedCollector));
      Assert.That (actualExpression, Is.SameAs (expectedExpression));
    }

    [Test]
    public void Dispatch_WithPotentialFromIdentifier ()
    {
      ParseResultCollector actualCollector = null;
      Expression actualExpression = null;
      ParameterExpression actualPotentialFromIdentifier = null;

      _dispatcher.RegisterParser ("Select", (collector, expression, potentialFromIdentifier) =>
      {
        actualCollector = collector;
        actualExpression = expression;
        actualPotentialFromIdentifier = potentialFromIdentifier;
      });

      ParseResultCollector expectedCollector = new ParseResultCollector (Expression.Constant (0));
      MethodCallExpression expectedExpression = (MethodCallExpression) _wrappedCall.Body;
      ParameterExpression expectedPotentialFromIdentifier = ExpressionHelper.CreateParameterExpression();
      _dispatcher.Dispatch (expectedCollector, expectedExpression, expectedPotentialFromIdentifier);

      Assert.That (actualCollector, Is.SameAs (expectedCollector));
      Assert.That (actualExpression, Is.SameAs (expectedExpression));
      Assert.That (actualPotentialFromIdentifier, Is.SameAs (expectedPotentialFromIdentifier));
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected OrderBy, Where for dispatching a method call to a parser, found Select.")]
    public void Dispatch_Unregistered ()
    {
      _dispatcher.RegisterParser ("OrderBy", _emptyParser);
      _dispatcher.RegisterParser ("Where", _emptyParser);

      _dispatcher.Dispatch (null, (MethodCallExpression) _wrappedCall.Body, null);
    }
  }
}