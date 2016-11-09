namespace OpenNos.Data
{
    public class MappingBaseDTO
    {
        #region Methods

        /// <summary>
        /// Intializes the GameObject, will be injected by AutoMapper after Entity -&gt; GO mapping
        /// Needs to be override in inherited GameObject.
        /// </summary>
        public virtual void Initialize()
        {
            //TODO override in GO
        }

        #endregion
    }
}