﻿using System.Windows.Automation.Provider;
using System.Windows.Markup;

using MediaFinder_v2.DataAccessLayer.Models;
using MediaFinder_v2.Messages;
using MediaFinder_v2.Services.Export;

namespace MediaFinder_v2.Helpers;

public class EnumBindingSourceExtension : MarkupExtension
{
    public Type EnumType {  get; private set; }

    public EnumBindingSourceExtension(Type enumType)
    {
        if (enumType is null || !enumType.IsEnum)
        {
            throw new ArgumentException("Argument was not an enum type.", nameof(enumType));
        }

        EnumType = enumType;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
        => EnumType == typeof(MultiMediaType) ? MultiMediaTypeExtensions.GetValues()
            : EnumType == typeof(ExportType) ? ExportTypeExtensions.GetValues()
            : EnumType == typeof(NavigateDirection) ? NavigationDirectionExtensions.GetValues()
            : EnumType == typeof(TriStateBoolean) ? TriStateBooleanExtensions.GetValues()
            : (object)Enum.GetValues(EnumType);
}
