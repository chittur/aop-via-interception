/**************************************************************************************************
 * Filename    = RangeAttribute.cs
 *
 * Author      = Ramaswamy Krishnan-Chittur
 *
 * Product     = AspectOrientedProgramming
 * 
 * Project     = ContextInterception
 *
 * Description = Defines the custom range attribute for parameters and return values.
 *************************************************************************************************/

using System;

namespace ContextInterception
{
    /// <summary>
    /// Attribute that defines the range for parameters and return values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter,
                    Inherited = false,
                    AllowMultiple = false)]
    public class RangeAttribute : Attribute
    {
        /// <summary>
        /// Creates an instance of the Range attribute.
        /// </summary>
        /// <param name="enable">Enable the Range attribute?</param>
        public RangeAttribute(bool enable = true)
        {
            this.Enabled = enable;

            this._lower = 0.0;
            this._upper = 0.0;
            this.CheckLower = false;
            this.CheckUpper = false;
        }

        /// <summary>
        /// Gets a value indicating whether the Range attribute is enabled.
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// Need to check lower range?
        /// </summary>
        public bool CheckLower { get; private set; }

        /// <summary>
        /// Need to check upper range?
        /// </summary>
        public bool CheckUpper { get; private set; }

        /// <summary>
        /// Gets or sets the lower range.
        /// </summary>
        public double Lower
        {
            get
            {
                return this._lower;
            }
            set
            {
                this._lower = value;
                this.CheckLower = true;
            }
        }

        /// <summary>
        /// Gets or sets the upper range.
        /// </summary>
        public double Upper
        {
            get
            {
                return this._upper;
            }
            set
            {
                this._upper = value;
                this.CheckUpper = true;
            }
        }

        private double _lower; // The lower range.
        private double _upper; // The upper range.
    }
}
