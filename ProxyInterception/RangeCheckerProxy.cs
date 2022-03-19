/**************************************************************************************************
 * Filename    = RangeCheckerProxy.cs
 *
 * Author      = Ramaswamy Krishnan-Chittur
 *
 * Product     = AspectOrientedProgramming
 * 
 * Project     = ProxyInterception
 *
 * Description = The Proxy class that intercepts and checks range for parameters and return values.
 *************************************************************************************************/

using System.Reflection;

namespace ProxyInterception
{
    public class RangeCheckerProxy<T> : DispatchProxy where T : class
    {
        /// <summary>
        /// Exposes the target object as a read-only property so that users can access
        /// fields or other implementation-specific details not available through the interface.
        /// </summary>
        public T Target { get; private set; }

        /// <summary>
        /// DispatchProxy's parameterless constructor is called when a new proxy instance is created.
        /// </summary>
        public RangeCheckerProxy() : base() { }

        /// <summary>
        /// Calls DispatchProxy's Create method to retrieve the proxy implementation of the target interface.
        /// </summary>
        /// <param name="target">The target object for which the proxy is being created.</param>
        /// <returns></returns>
        public static T Decorate(T target)
        {
            // DispatchProxy.Create creates the proxy object.
            var proxy = Create<T, RangeCheckerProxy<T>>() as RangeCheckerProxy<T>;

            // If the proxy wraps an underlying object, it must be supplied after creating the proxy.
            proxy.Target = target;

            return proxy as T;
        }

        /// <summary>
        /// Intercepts all method calls of the proxy here.
        /// </summary>
        /// <param name="targetMethod">The target method being invoked.</param>
        /// <param name="args">The arguments passed to the method being invoked.</param>
        /// <returns>The object returned from the target method invocation.</returns>
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            try
            {
                // Validate that the parameter arguments are within range.
                CheckRangeOnParameters(targetMethod, args);

                // Call the original method.
                var result = targetMethod.Invoke(Target, args);

                // Validate that the return value is within range.
                CheckRangeOnReturnValue(targetMethod, result);

                return result;
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException ?? exception;
            }
        }

        /// <summary>
        /// Performs range check on the parameters.
        /// </summary>
        /// <param name="method">The target method being invoked.</param>
        /// <param name="args">The arguments passed to the method being invoked.</param>
        private static void CheckRangeOnParameters(MethodInfo method, object[] args)
        {
            // Get the parameters.
            ParameterInfo[] parameterInfo = method.GetParameters();
            for (int count = 0; count < parameterInfo.Length; ++count)
            {
                ParameterInfo parameter = parameterInfo[count];

                // Get the range attribute if present.
                object[] attributes = parameter.GetCustomAttributes(false);
                foreach (object attribute in attributes)
                {
                    if (attribute is IRangeChecker range)
                    {
                        object arg = args[count]; // Argument of the parameter.
                        bool check = range.CheckRange(arg);
                        if (!check)
                        {
                            string error = $"{method.Name} invoked with argument for parameter {parameter.Name} in position {parameter.Position} being out of range with value {arg}";
                            throw new ArgumentOutOfRangeException(error);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Performs range check on the return value.
        /// </summary>
        /// <param name="methodReturnMessage">The return message on which the range check needs be performed.</param>
        /// <param name="methodCallMessage">The method call message.</param>
        private static void CheckRangeOnReturnValue(MethodInfo method, object? result)
        {
            // Get the range attribute if present.
            object[] attributes = method.GetCustomAttributes(false);
            foreach (object attribute in attributes)
            {
                if (attribute is IRangeChecker range)
                {
                    bool check = range.CheckRange(result); // return value.
                    if (!check)
                    {
                        string error = $"{method.Name} returned out of range value of {result}";
                        throw new ArgumentOutOfRangeException(error);
                    }
                }
            }
        }
    }
}