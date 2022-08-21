using OxGFrame.APICenter;

public class APICenterExample : APICenterBase<APICenterExample>
{
    #region declaration and definition API_xBASE
    public const int APIQueryTest = API_xBASE + 1;
    #endregion

    public APICenterExample()
    {
        #region Register Event
        this.Register(new APIQueryTest(APIQueryTest));
        #endregion
    }
}
