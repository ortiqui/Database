namespace Database.Core.Attribute
{
    #region Usings

    using System;
    using System.Reflection;

    #endregion

    public class ProviderAttribute : Attribute
    {
        #region Campos

        private string name;

        private string invariant;

        private string description;

        private string factoryType;

        #endregion

        #region Constructor

        public ProviderAttribute(string name, string invariant, string factoryType, string description)
        {
            this.Name = name;
            this.Invariant = invariant;
            this.FactoryType = factoryType;
            this.Description = description;
        }

        #endregion

        #region Propiedades

        public string Name
        {
            get => this.name;
            set => this.name = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Invariant
        {
            get => this.invariant;
            set => this.invariant = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Description
        {
            get => this.description;
            set => this.description = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string FactoryType
        {
            get => this.factoryType;
            set => this.factoryType = value ?? throw new ArgumentNullException(nameof(value));
        }

        #endregion
    }
}