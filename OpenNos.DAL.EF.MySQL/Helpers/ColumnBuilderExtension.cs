using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Builders;
using System.Data.Entity.Migrations.Model;

namespace System.Data.Entity.Migrations
{
    public static class ColumnBuilderExtensions
    {
        public static ColumnModel Time(this ColumnBuilder c, bool? nullable = null, byte? precision = null, TimeSpan? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, System.Data.Entity.Infrastructure.Annotations.AnnotationValues> annotations = null, bool fixedLength = false)
        {
            return c.Time(nullable, precision, defaultValue, defaultValueSql, name, storeType, annotations);
        }

        public static ColumnModel SByte(this ColumnBuilder c, bool? nullable = null, bool identity = false, byte? defaultValue = null, string defaultValueSql = null, string name = null, string storeType = null, IDictionary<string, System.Data.Entity.Infrastructure.Annotations.AnnotationValues> annotations = null)
        {
            return c.Byte(nullable, identity, defaultValue, defaultValueSql, name, storeType ?? "tinyint", annotations);
        }
    }
}