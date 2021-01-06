using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections.Immutable;

namespace TygerbergNeonatalAddin
{
    public abstract class TransformationException : Exception
    {
        public ITransformation Transformation { get; private set; }

        public TransformationException(ITransformation transformation) : base() { Transformation = transformation; }
        public TransformationException(ITransformation transformation, string message) : base(message) { Transformation = transformation; }
        public TransformationException(ITransformation transformation, string message, Exception innerException) : base(message, innerException) { Transformation = transformation; }
    }

    public class ColumnHeaderNotFoundException : TransformationException
    {
        public string ColumnHeader { get; private set; }

        public ColumnHeaderNotFoundException(ITransformation transformation, string columnHeader) : base(transformation) { ColumnHeader = columnHeader; }
        public ColumnHeaderNotFoundException(ITransformation transformation, string columnHeader, string message) : base(transformation, message) { ColumnHeader = columnHeader; }
        public ColumnHeaderNotFoundException(ITransformation transformation, string columnHeader, string message, Exception innerException) : base(transformation, message, innerException) { ColumnHeader = columnHeader; }
    }

    public class MultipleColumnsWithThisHeaderException : TransformationException
    {
        public string ColumnHeader { get; private set; }

        public MultipleColumnsWithThisHeaderException(ITransformation transformation, string columnHeader) : base(transformation) { ColumnHeader = columnHeader; }
        public MultipleColumnsWithThisHeaderException(ITransformation transformation, string columnHeader, string message) : base(transformation, message) { ColumnHeader = columnHeader; }
        public MultipleColumnsWithThisHeaderException(ITransformation transformation, string columnHeader, string message, Exception innerException) : base(transformation, message, innerException) { ColumnHeader = columnHeader; }
    }

    public class GroupingInconsistencyException : TransformationException
    {
        public ImmutableList<GroupingFilter.Inconsistency> Inconsistencies { get; }

        public GroupingInconsistencyException(ITransformation transformation, ImmutableList<GroupingFilter.Inconsistency> inconsistencies) : base(transformation) { Inconsistencies = inconsistencies; }
        public GroupingInconsistencyException(ITransformation transformation, ImmutableList<GroupingFilter.Inconsistency> inconsistencies, string message) : base(transformation, message) { Inconsistencies = inconsistencies; }
        public GroupingInconsistencyException(ITransformation transformation, ImmutableList<GroupingFilter.Inconsistency> inconsistencies, string message, Exception innerException) : base(transformation, message, innerException) { Inconsistencies = inconsistencies; }
    }

    public class CannotGroupMembersException : TransformationException
    {
        public ImmutableList<GroupingFilter.MemberDetails> Members { get; }

        public CannotGroupMembersException(ITransformation transformation, ImmutableList<GroupingFilter.MemberDetails> members) : base(transformation) { Members = members; }
        public CannotGroupMembersException(ITransformation transformation, ImmutableList<GroupingFilter.MemberDetails> members, string message) : base(transformation, message) { Members = members; }
        public CannotGroupMembersException(ITransformation transformation, ImmutableList<GroupingFilter.MemberDetails> members, string message, Exception innerException) : base(transformation, message, innerException) { Members = members; }
    }

    public class ExpectedDateColumnException : TransformationException
    {
        public string ColumnHeader { get; }

        public ExpectedDateColumnException(ITransformation transformation, string columnHeader) : base(transformation) { ColumnHeader = columnHeader; }
        public ExpectedDateColumnException(ITransformation tranaformation, string columnHeader, string message) : base(tranaformation, message) { ColumnHeader = columnHeader; }
        public ExpectedDateColumnException(ITransformation transformation, string columnHeader, string message, Exception innerException) : base(transformation, message, innerException) { ColumnHeader = columnHeader; }
    }
}
