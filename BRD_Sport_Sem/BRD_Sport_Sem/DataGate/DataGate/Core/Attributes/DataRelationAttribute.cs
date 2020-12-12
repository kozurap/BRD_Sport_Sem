using System;

namespace DataGate.Core.Attributes
{
    public class DataRelationAttribute : Attribute
    {
        public string DbIdentifier;

        public DataRelationAttribute(string dbIdentifier)
        {
            DbIdentifier = dbIdentifier;
        }
    }
}