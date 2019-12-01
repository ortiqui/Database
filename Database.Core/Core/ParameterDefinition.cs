namespace Database.Core
{
    #region Usings

    using System.Data;

    #endregion

    public class ParameterDefinition
    {
        public ParameterDefinition(string name, object value, ParameterDirection direction)
        {
            this.Name = name;
            this.Value = value;
            this.Direction = direction;
        }

        public string Name { get; private set; }

        public object Value { get; private set; }

        public DbType Type => DatabaseCore.DbTypes[Value.GetType()];

        public ParameterDirection Direction { get; set; }
    }
}