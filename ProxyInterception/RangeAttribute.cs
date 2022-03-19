/**************************************************************************************************
 * Filename    = RangeAttribute.cs
 *
 * Author      = Ramaswamy Krishnan-Chittur
 *
 * Product     = AspectOrientedProgramming
 * 
 * Project     = ProxyInterception
 *
 * Description = Defines the custom range attribute for parameters and return values.
 *************************************************************************************************/

namespace ProxyInterception
{
    /// <summary>
    /// Defines a range checker.
    /// </summary>
    internal interface IRangeChecker
    {
        /// <summary>
        /// Checks whether the given argument is within a range specified in the instance of the implementing class.
        /// </summary>
        /// <param name="arg">The argument which is being checked for range.</param>
        /// <returns>A value indicating whether the given argument falls within the given range.</returns>
        bool CheckRange(object? arg);
    }

    /// <summary>
    /// Attribute that defines the range for parameters and return values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter,
                    Inherited = false,
                    AllowMultiple = false)]
    public class RangeAttribute<T> : Attribute, IRangeChecker where T : IComparable<T>
    {
        /// <summary>
        /// Creates an instance of the Range attribute.
        /// </summary>
        /// <param name="enable">Enable the Range attribute?</param>
        public RangeAttribute(bool enable = true)
        {
            this.Enabled = enable;

            this._lower = default!;
            this._upper = default!;

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
        public T Lower
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
        public T Upper
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

        private T _lower; // The lower range.
        private T _upper; // The upper range.

        /// <summary>
        /// Checks whether the given argument is within a range specified.
        /// </summary>
        /// <param name="arg">The argument which is being checked for range.</param>
        /// <returns>A value indicating whether the given argument falls within the given range.</returns>
        public bool CheckRange(object? arg)
        {
            if (!Enabled)
            {
                return true;
            }

            if (arg is not IComparable<T> value)
            {
                return true;
            }

            bool failed = ((CheckLower && (value.CompareTo(Lower) < 0)) || (CheckUpper && (value.CompareTo(Upper) > 0)));
            return !failed;
        }
    }
}
