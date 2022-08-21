namespace OxGFrame.APICenter
{
    public abstract class APIBase
    {
        private int _fundId = 0;
        public int GetFuncId() { return this._fundId; }

        public APIBase(int funcId)
        {
            this._fundId = funcId;
        }
    }
}