/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using System.Collections.Generic;
using System.Data.Entity.Migrations.Builders;
using System.Data.Entity.Migrations.Model;

namespace System.Data.Entity.Migrations
{
    public static class ColumnBuilderExtensions
    {
        #region Methods

        public static ColumnModel SByte(this ColumnBuilder c, bool? nullable = null, bool identity = false, byte? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, System.Data.Entity.Infrastructure.Annotations.AnnotationValues> annotations = null)
        {
            return c.Byte(nullable, identity, defaultValue, defaultValueSql, name, storeType ?? "tinyint", annotations);
        }

        public static ColumnModel Time(this ColumnBuilder c, bool? nullable = null, byte? precision = null, TimeSpan? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, System.Data.Entity.Infrastructure.Annotations.AnnotationValues> annotations = null, bool fixedLength = false)
        {
            return c.Time(nullable, precision, defaultValue, defaultValueSql, name, storeType, annotations);
        }

        #endregion
    }
}