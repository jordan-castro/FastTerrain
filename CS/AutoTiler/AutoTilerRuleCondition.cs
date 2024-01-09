namespace FastTerrain;

public class AutoTilerRuleCondition : DirectionMap<string>
{
    public string Result = null;

    public AutoTilerRuleCondition(
        string result, 
        string N, 
        string E, 
        string S, 
        string W
    ) : base(N, E, S, W)
    {
        Result = result;
    }
}