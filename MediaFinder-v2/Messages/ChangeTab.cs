namespace MediaFinder_v2.Messages;

public record class ChangeTab(int TabIndex)
{
    public const int AddSettingTab = 0;
    public const int ViewSettingsTab = 1;
    public const int PerformSearchTab = 2;

    public static ChangeTab ToAddSettingTab()
        => new(AddSettingTab);

    public static ChangeTab ToViewSettingsTab()
        => new(ViewSettingsTab);

    public static ChangeTab ToPerformSearchTab()
        => new(PerformSearchTab);
}
