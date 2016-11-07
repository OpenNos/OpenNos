namespace OpenNos.Data
{
    public class MappingBaseDTO
    {
        /// <summary>
        /// Intializes the GameObject, will be injected by AutoMapper after Entity -> GO mapping
        /// Needs to be override in inherited GameObject.
        /// </summary>
        public virtual void Initialize()
        {
            //TODO override in GO
        }
    }
}