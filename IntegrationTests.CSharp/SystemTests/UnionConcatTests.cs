﻿// Microsoft Public License (Ms-PL)
// 
// This license governs use of the accompanying software. If you use the software, you
// accept this license. If you do not accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
// same meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, 
// prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, 
// sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from 
// such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present 
// in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of 
// this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a 
// license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. 
// You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws,
// the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Remotion.Linq.IntegrationTests.CSharp.SystemTests
{
  [TestFixture]
  public class UnionConcatTests : TestBase
  {
    [Test]
    public void UnionQuery ()
    {
      var query =
          DB.Customers.Where (c => c.City == "London")
              .Union (DB.Customers.Where (c => c.City == "Berlin"))
              .Union (DB.Customers.Where (c => c.City == "Madrid"));

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void ConcatQuery ()
    {
      var query =
          DB.Customers.Where (c => c.City == "London")
              .Concat (DB.Customers.Where (c => c.City == "Berlin"));

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void ConcatQueryWithDuplicates ()
    {
      var query =
          DB.Customers.Where (c => c.City == "London")
              .Concat (DB.Customers.Where (c => c.City == "Berlin"))
              .Concat (DB.Customers.Where (c => c.City == "London"));
      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void ThrowingQueryUnion ()
    {
      var query = DB.Customers.Where (c => c.City == "London").Select (c => SomeMethod (c.City))
          .Union (DB.Customers.Where (c => c.City == "Berlin").Select (c => SomeMethod (c.City)));

      //Test seperate because it is not implemented in Linq to sql but not in Relinq sql backend
      if (IsLinqToSqlActive)
      {
        TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
      }
      else if (IsRelinqSqlBackendActive)
      {
        //Get Name of Current Method now because it would return false results when called
        //from inside the lambda Method
        MethodBase currentMethod = MethodBase.GetCurrentMethod();

        Assert.That (
            () => TestExecutor.Execute (query, currentMethod),
            Throws.TypeOf<NotSupportedException>()
                .With.Message.EqualTo (
                    "In-memory method calls are not supported when a set operation (such as Union or Concat) is used. "
                    + "Rewrite the query to perform the in-memory operation after the set operation has been performed."
                    + Environment.NewLine
                    + "For example, instead of the following query:"
                    + Environment.NewLine
                    + "    SomeOrders.Select (o => SomeMethod (o.ID)).Concat (OtherOrders.Select (o => SomeMethod (o.ID)))"
                    + Environment.NewLine
                    + "Try the following query:"
                    + Environment.NewLine
                    + "    SomeOrders.Select (o => o.ID).Concat (OtherOrders.Select (o => o.ID)).Select (i => SomeMethod (i))"));
      }
    }

    [Test]
    public void ThrowingQueryConcat ()
    {
      var query = DB.Customers.Where (c => c.City == "London").Select (c => SomeMethod (c.City))
          .Concat (DB.Customers.Where (c => c.City == "Berlin").Select (c => SomeMethod (c.City)));

      //Test seperate because it is not implemented in Linq to sql but not in Relinq sql backend
      if (IsLinqToSqlActive)
      {
        TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
      }
      else if (IsRelinqSqlBackendActive)
      {
        //Get Name of Current Method now because it woul return false results when called
        //from inside the lambda Method
        MethodBase currentMethod = MethodBase.GetCurrentMethod();

        Assert.That (
            () => TestExecutor.Execute (query, currentMethod),
            Throws.TypeOf<NotSupportedException>()
                .With.Message.EqualTo (
                    "In-memory method calls are not supported when a set operation (such as Union or Concat) is used. "
                    + "Rewrite the query to perform the in-memory operation after the set operation has been performed."
                    + Environment.NewLine
                    + "For example, instead of the following query:"
                    + Environment.NewLine
                    + "    SomeOrders.Select (o => SomeMethod (o.ID)).Concat (OtherOrders.Select (o => SomeMethod (o.ID)))"
                    + Environment.NewLine
                    + "Try the following query:"
                    + Environment.NewLine
                    + "    SomeOrders.Select (o => o.ID).Concat (OtherOrders.Select (o => o.ID)).Select (i => SomeMethod (i))"));
      }
    }

    private object SomeMethod (string city)
    {
      return "testcity";
    }
  }
}