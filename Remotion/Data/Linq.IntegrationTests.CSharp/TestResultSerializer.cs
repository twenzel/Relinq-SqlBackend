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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp
{
  /// <summary>
  /// Provides functionality to serialize the result of LINQ tests into a human-readable and machine-readable format for comparison between tests.
  /// </summary>
  public class TestResultSerializer
  {
    public const string DefaultSpacer = "  ";

    private readonly TextWriter _textWriter;
    private readonly string _spacer;
    private readonly int _level;

    public TestResultSerializer (TextWriter textWriter, string spacer, int level)
    {
      ArgumentUtility.CheckNotNull ("textWriter", textWriter);

      _textWriter = textWriter;
      _spacer = spacer;
      _level = level;
    }

    public TestResultSerializer (TextWriter textWriter)
      : this (textWriter, DefaultSpacer, 0)
    {}

    public void Serialize (object value)
    {
      WriteSpacing();

      SerializeWithoutSpacing(value);
    }

    public void Serialize (object value, string name)
    {
      ArgumentUtility.CheckNotNull ("name", name);

      WriteSpacing();

      _textWriter.Write (name);
      _textWriter.Write (": ");

      SerializeWithoutSpacing(value);
    }

    private void SerializeWithoutSpacing (object value)
    {
      if (value == null)
        _textWriter.WriteLine ("null");
      else if (value is string)
        SerializeString ((string) value);
      else if (value is IEnumerable)
        SerializeEnumerable ((IEnumerable) value);
      else if (value is ValueType)
        _textWriter.WriteLine (value);
      else
        SerializeComplexValue (value);
    }

    private void SerializeString (string value)
    {
      Debug.Assert (value != null, "should be handled by caller");

      var escapedValue = value.Replace ("'", "''");
      _textWriter.WriteLine ("'" + escapedValue + "'");
    }

    private void SerializeEnumerable (IEnumerable value)
    {
      Debug.Assert (value != null, "should be handled by caller");

      WriteTypeName (value);
      _textWriter.WriteLine (" {");
      TestResultSerializer elementSerializer = CreateIndentedSerializer();
      
      foreach (var element in value)
        elementSerializer.Serialize (element);

      WriteSpacing();
      _textWriter.WriteLine ("}");
    }

    private void SerializeComplexValue (object value)
    {
      Debug.Assert (value != null, "should be handled by caller");
      WriteTypeName(value);
      _textWriter.WriteLine();

      MemberInfo[] members = value.GetType().GetMembers (BindingFlags.Public | BindingFlags.Instance);
      
      Array.Sort (members, (m1, m2) => m1.Name.CompareTo(m2.Name));

      var memberValueSerializer = CreateIndentedSerializer();
      foreach (var memberInfo in members)
      {
        object memberValue;
        if (TryGetValue (value, memberInfo, out memberValue))
          memberValueSerializer.Serialize (memberValue, memberInfo.Name);
      }
    }

    private bool TryGetValue (object instance, MemberInfo memberInfo, out object value)
    {
      if (memberInfo.MemberType == MemberTypes.Property)
      {
        value = ((PropertyInfo) memberInfo).GetValue (instance, null);
        return true;
      }
      else if (memberInfo.MemberType == MemberTypes.Field)
      {
        value = ((FieldInfo) memberInfo).GetValue (instance);
        return true;
      }
      else
      {
        value = null;
        return false;
      }
    }

    private void WriteSpacing ()
    {
      for (int i = 0; i < _level; ++i)
        _textWriter.Write (_spacer);
    }

    private void WriteTypeName (object value)
    {
      _textWriter.Write (value.GetType ().Name);
    }

    private TestResultSerializer CreateIndentedSerializer ()
    {
      return new TestResultSerializer (_textWriter, _spacer, _level + 1);
    }
  }
}