namespace OxGFrame.CoreFrame
{
    public class HidePropertiesInInspector : System.Attribute
    {
        private string[] _props;

        public HidePropertiesInInspector(params string[] props)
        {
            this._props = props;
        }

        public string[] hiddenProperties
        {
            get { return this._props; }
        }
    }
}