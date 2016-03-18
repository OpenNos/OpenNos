using System;

namespace OpenNos.Core.Networking.Communication.ScsServices.Service
{
    /// <summary>
    /// Any SCS Service interface class must has this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public class ScsServiceAttribute : Attribute
    {
        #region Instantiation

        /// <summary>
        /// Creates a new ScsServiceAttribute object.
        /// </summary>
        public ScsServiceAttribute()
        {
            Version = "NO_VERSION";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Service Version. This property can be used to indicate the code version.
        /// This value is sent to client application on an exception, so, client application can know that service version is changed.
        /// Default value: NO_VERSION.
        /// </summary>
        public string Version { get; set; }

        #endregion
    }
}