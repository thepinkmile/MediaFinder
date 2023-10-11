using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace MediaFinder_v2.Helpers;

public class AdvancedBindingList<T> : BindingList<T>
{
    protected override bool SupportsSortingCore => true;

    protected override bool IsSortedCore => _isSorted;
    private bool _isSorted = false;

    protected override ListSortDirection SortDirectionCore => _sortDirection;
    private ListSortDirection _sortDirection = ListSortDirection.Ascending;

    protected override PropertyDescriptor? SortPropertyCore => _sortProperty;
    private PropertyDescriptor? _sortProperty;

    private static readonly Dictionary<Type, IComparer?> ComparerCache = new();

    protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
    {
        ((List<T>)Items).Sort(new Comparison<T>((x, y) =>
        {
            IComparer? comp;
            if (!ComparerCache.ContainsKey(property.PropertyType))
            {
                var compType = typeof(Comparer<>).MakeGenericType(property.PropertyType);
                var compField = compType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public);
                comp = compField?.GetValue(null) as IComparer;
            }
            else
            {
                comp = ComparerCache[property.PropertyType];
            }

            int? result = direction == ListSortDirection.Ascending
                ? (int?)(comp?.Compare(property.GetValue(x), property.GetValue(y)))
                : (int?)(comp?.Compare(property.GetValue(y), property.GetValue(x)));
            return result ?? 0;
        }));

        _isSorted = true;
        _sortProperty = property;
        _sortDirection = direction;
    }
}
