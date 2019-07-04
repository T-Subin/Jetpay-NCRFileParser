using System;

namespace JetPayFileParser.Model.Attribute
{
    /// <summary>
    /// Maps a property to a Database Column or XML Element
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DataColumnAttribute : System.Attribute
    {

        #region Declarations

        private string _columnName;
        private System.Type _dbType;

        #endregion

        #region Properties

        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        public System.Type ColumnType
        {
            get { return _dbType; }
            set { _dbType = value; }
        }

        #endregion

        #region Constructors

        private DataColumnAttribute()
        {
            _columnName = string.Empty;
            _dbType = null;
        }

        /// <summary>
        /// Map a property to a database column or XML element
        /// </summary>
        /// <param name="name">Name of the column or element to map</param>
        /// <param name="type">Underlying DbType of the column</param>
        public DataColumnAttribute(string name, System.Type type)
            : this()
        {
            _columnName = name;
            _dbType = type;
        }
        #endregion
    }
}
