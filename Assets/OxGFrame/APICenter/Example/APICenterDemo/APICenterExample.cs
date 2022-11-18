using OxGFrame.APICenter;

public class APICenterExample : APICenterBase<APICenterExample>
{
    #region declaration and definition xBASE
    public const int APIQueryTest = xBASE + 1;
    #endregion

    public APICenterExample()
    {
        #region Register Event
        this.Register(new APIQueryTest(APIQueryTest));
        #endregion
    }
}
