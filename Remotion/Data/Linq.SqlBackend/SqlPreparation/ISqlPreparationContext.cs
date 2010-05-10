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
using System.Linq.Expressions;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="ISqlPreparationContext"/> provides methods to handle a concrete preparation context.
  /// </summary>
  public interface ISqlPreparationContext
  {
    int QuerySourceMappingCount { get; } // TODO Review 2668: It should be possible to remove this property, it's only used for tests
    void AddContextMapping (Expression key, Expression value); // TODO Review 2668: Maybe rename to "ExpressionMapping"? (Here and in the Get methods) Also, I'd prefer "original", "replacement" instead of "key", "value"
    Expression GetContextMapping (Expression key);
    Expression TryGetContextMappingFromHierarchy (Expression key);
  }
}