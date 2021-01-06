using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace TygerbergNeonatalAddin
{
    public enum FilterTypes
    {
        Null,
        Blacklist,
        Whitelist,
        DuplicateRemoval,
        TimePeriod,
        Grouping,
        Count,
    }

    static class FilterTypesExtensions
    {
        public static string Description(this FilterTypes type)
        {
            if (type == FilterTypes.Null) { return "Do nothing"; }
            if (type == FilterTypes.Blacklist) { return "Blacklist fields"; }
            if (type == FilterTypes.Whitelist) { return "Whitelist fields"; }
            if (type == FilterTypes.DuplicateRemoval) { return "Remove duplicates"; }
            if (type == FilterTypes.TimePeriod) { return "Time period filter"; }
            if (type == FilterTypes.Grouping) { return "Grouping"; }
            throw new InvalidOperationException("Unrecognised filter type");
        }
    }

    [KnownType(typeof(NullFilter))]
    [KnownType(typeof(BlacklistFilter))]
    [KnownType(typeof(WhitelistFilter))]
    [KnownType(typeof(DuplicateRemovalFilter))]
    [KnownType(typeof(TimePeriodFilter))]
    [KnownType(typeof(GroupingFilter))]
    public abstract class Filter : ITransformation
    {
        public abstract Table Apply(Table table);

        public string Name { get; set; } = "";
        public bool ExcludeFromBatchProcessing { get; set; } = false;

        public Filter()
        { }

        public Filter(string name, bool excludeFromBatchProcessing)
        {
            Name = name;
            ExcludeFromBatchProcessing = excludeFromBatchProcessing;
        }

        public abstract FilterTypes FilterType { get; }
    }

    public class NullFilter : Filter
    {
        public NullFilter()
        { }

        public NullFilter(string name, bool excludeFromBatchProcessing) : base(name, excludeFromBatchProcessing)
        { }

        public override FilterTypes FilterType => FilterTypes.Null;

        public override Table Apply(Table input)
        {
            return input;
        }
    }

    public class BlacklistFilter : Filter
    {
        public string ColumnHeader { get; set; } = "";
        public List<string> DisallowedValues { get; set; } = new List<string>();

        public BlacklistFilter()
        { }

        public BlacklistFilter(string name, bool excludeFromBatchProcessing) : base(name, excludeFromBatchProcessing)
        { }

        public override FilterTypes FilterType => FilterTypes.Blacklist;

        public override Table Apply(Table input)
        {
            TransformationUtil.ThrowExceptionIfHeaderDoesNotUniquelyReferenceAColumnInTable(this, ColumnHeader, input);

            Table ret = new Table(input.Columns);
            
            foreach (IRow row in input)
            {
                if (DisallowedValues.Contains(row[ColumnHeader], StringComparer.OrdinalIgnoreCase)) continue;

                ret.AddRow(row);
            }

            return ret;
        }
    }

    public class WhitelistFilter : Filter
    {
        public string ColumnHeader { get; set; } = "";
        public List<string> AllowedValues { get; set; } = new List<string>();

        public WhitelistFilter()
        { }

        public WhitelistFilter(string name, bool excludeFromBatchProcessing) : base(name, excludeFromBatchProcessing)
        { }

        public override FilterTypes FilterType => FilterTypes.Whitelist;

        public override Table Apply(Table input)
        {
            TransformationUtil.ThrowExceptionIfHeaderDoesNotUniquelyReferenceAColumnInTable(this, ColumnHeader, input);

            Table ret = new Table(input.Columns);
            
            foreach (IRow row in input)
            {
                if (AllowedValues.Contains(row[ColumnHeader], StringComparer.OrdinalIgnoreCase))
                {
                    ret.AddRow(row);
                }
            }

            return ret;
        }
    }

    public class DuplicateRemovalFilter : Filter
    {
        public string ColumnHeader { get; set; } = "";

        public DuplicateRemovalFilter()
        { }

        public DuplicateRemovalFilter(string name, bool excludeFromBatchProcessing) : base(name, excludeFromBatchProcessing)
        { }

        public override FilterTypes FilterType => FilterTypes.DuplicateRemoval;

        public override Table Apply(Table input)
        {
            TransformationUtil.ThrowExceptionIfHeaderDoesNotUniquelyReferenceAColumnInTable(this, ColumnHeader, input);

            Table ret = new Table(input.Columns);

            Dictionary<string, bool> seenValues = new Dictionary<string, bool>();

            foreach (IRow row in input)
            {
                if (seenValues.ContainsKey(row[ColumnHeader].ToLowerInvariant())) continue;

                seenValues[row[ColumnHeader].ToLowerInvariant()] = true;
                ret.AddRow(row);
            }

            return ret;
        }
    }

    public class TimePeriodFilter : Filter
    {
        public string DateColumnHeader { get; set; } = "";
        public TimeSpan MaximumAdjacentSpan { get; set; } = TimeSpan.FromDays(7);
        public List<string> ClusterDefiningColumnHeaders { get; set; } = new List<string>();

        public TimePeriodFilter()
        { }

        public TimePeriodFilter(string name, bool excludeFromBatchProcessing) : base(name, excludeFromBatchProcessing)
        { }

        public override FilterTypes FilterType => FilterTypes.TimePeriod;

        public override Table Apply(Table input)
        {
            foreach (string header in ClusterDefiningColumnHeaders)
            {
                TransformationUtil.ThrowExceptionIfHeaderDoesNotUniquelyReferenceAColumnInTable(this, header, input);
            }
            TransformationUtil.ThrowExceptionIfHeaderDoesNotUniquelyReferenceAColumnInTable(this, DateColumnHeader, input);
            if (input.Columns[input.IndexForColumnWithHeader(DateColumnHeader)].typeHint != Table.TypeHint.Date) throw new ExpectedDateColumnException(this, DateColumnHeader);

            List<List<IRow>> clusters = Clusters.FindClustersInTable(input, MaximumAdjacentSpan, DateColumnHeader,
                minimumNumberOfInstancesPerCluster: 1,
                headersForColumnsWhoseValuesShouldBeUniformAcrossEachCluster: ClusterDefiningColumnHeaders);

            return new Table(input.Columns, (from cluster in clusters select cluster[0]).OrderBy((row) => row.Index) /* order by index in original table */);
        }
    }

    public class GroupingFilter : Filter
    {
        public class Inconsistency
        {
            public string Member { get; }
            public ImmutableList<string> GroupsMemberIsAPartOf { get; }

            public Inconsistency(string member, ImmutableList<string> groupsMemberIsAPartOf)
            {
                Member = member;
                GroupsMemberIsAPartOf = groupsMemberIsAPartOf;
            }
        }

        public class MemberDetails
        {
            public string Value { get; }
            public IRow Row { get; }
            public int ColumnIndex { get; }

            public MemberDetails(string value, IRow row, int columnIndex)
            {
                Value = value;
                Row = row;
                ColumnIndex = columnIndex;
            }
        }

        public string ColumnHeader { get; set; } = "";
        public Dictionary<string, List<string>> MembersDictionary { get; set; } = new Dictionary<string, List<string>>();
        public string GroupForMembersThatCannotBeGrouped { get; set; } = null;

        public List<Inconsistency> GetInconsistencies()
        {
            var ret = new List<Inconsistency>();

            var groupsDictionary = new Dictionary<string, List<string>>();
            List<string> inconsistentMembers = new List<string>();

            foreach (var pair in MembersDictionary)
            {
                foreach (string member in pair.Value)
                {
                    List<string> groups;
                    if (groupsDictionary.TryGetValue(member, out groups))
                    {
                        groups.Add(pair.Key);
                        inconsistentMembers.Add(member);
                    }
                    else
                    {
                        groups = new List<string>();
                        groupsDictionary[member] = groups;
                        groups.Add(pair.Key);
                    }
                }
            }

            foreach (string inconsistentMember in inconsistentMembers)
            {
                ret.Add(new Inconsistency(inconsistentMember, groupsDictionary[inconsistentMember].ToImmutableList()));
            }

            return ret;
        }

        public Dictionary<string, string> GetGroupsDictionary(bool lowercaseMembers = false)
        {
            var groupsDictionary = new Dictionary<string, string>();

            foreach (var pair in MembersDictionary)
            {
                foreach (string member in pair.Value)
                {
                    if (groupsDictionary.ContainsKey(member)) { throw new InvalidOperationException("There are inconsistencies in the groups."); }
                    groupsDictionary[lowercaseMembers ? member.ToLowerInvariant() : member] = pair.Key;
                }
            }

            return groupsDictionary;
        }

        public override Table Apply(Table input)
        {
            List<Inconsistency> inconsistencies = GetInconsistencies();
            if (inconsistencies.Count > 0)
            {
                throw new GroupingInconsistencyException(this, inconsistencies.ToImmutableList());
            }
            TransformationUtil.ThrowExceptionIfHeaderDoesNotUniquelyReferenceAColumnInTable(this, ColumnHeader, input);
            Dictionary<string, string> groupsDictionary = GetGroupsDictionary(lowercaseMembers: true);
            int columnIndex = input.IndexForColumnWithHeader(ColumnHeader);

            Table ret = new Table(input.Columns);
            List<MemberDetails> membersThatCannotBeGrouped = new List<MemberDetails>(); // we only fill this up if GroupForMembersThatCannotBeGrouped == null

            foreach (IRow row in input)
            {
                string newValue;

                if (groupsDictionary.ContainsKey(row[columnIndex].ToLowerInvariant()))
                {
                    newValue = groupsDictionary[row[columnIndex].ToLowerInvariant()];
                }
                else
                {
                    if (GroupForMembersThatCannotBeGrouped != null)
                    {
                        newValue = GroupForMembersThatCannotBeGrouped;
                    }
                    else
                    {
                        membersThatCannotBeGrouped.Add(new MemberDetails(row[columnIndex], row, columnIndex));
                        newValue = row[columnIndex];
                    }
                }

                ret.AddRow(row.ToImmutableList().SetItem(columnIndex, newValue));
            }

            if (membersThatCannotBeGrouped.Count > 0)
            {
                throw new CannotGroupMembersException(this, membersThatCannotBeGrouped.ToImmutableList());
            }

            return ret;
        }

        public GroupingFilter()
        { }

        public GroupingFilter(string name, bool excludeFromBatchProcessing) : base(name, excludeFromBatchProcessing)
        { }

        public override FilterTypes FilterType => FilterTypes.Grouping;
    }
}
