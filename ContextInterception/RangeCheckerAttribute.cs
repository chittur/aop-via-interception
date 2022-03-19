/**************************************************************************************************
 * Filename    = RangeCheckerAttribute.cs
 *
 * Author      = Ramaswamy Krishnan-Chittur
 *
 * Product     = AspectOrientedProgramming
 * 
 * Project     = ContextInterception
 *
 * Description = Defines the interception classes required for context-bound interception.
 *************************************************************************************************/

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;

namespace ContextInterception
{
    /// <summary>
    /// The custom attribute class to enable range checking using context interception.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct,
                    Inherited = false,
                    AllowMultiple = false)]
    public class RangeCheckerAttribute : ContextAttribute
    {
        /// <summary>
        /// Creates a new instance of the RangeCheck class.
        /// </summary>
        /// <param name="apply">Turn on range checking?</param>
        public RangeCheckerAttribute(bool apply) : base("RangeChecker")
        {
            this._apply = apply;
        }

        /// <summary>
        /// Forces a new context there by enabling interception, if range check is required.
        /// </summary>
        /// <param name="context">The context to check against the current context attribute</param>
        /// <param name="constructionMessage">The construction call, parameters of which need to be checked against the current context.</param>
        /// <returns></returns>
        public override bool IsContextOK(Context context, IConstructionCallMessage constructionMessage)
        {
            // return false to enable the range checker. true otherwise.
            return !this._apply;
        }

        /// <summary>
        /// Gets properties for the new context.
        /// </summary>
        /// <param name="constructionMessage">The IConstructionCallMessage to which to add the context properties.</param>
        public override void GetPropertiesForNewContext(IConstructionCallMessage constructionMessage)
        {
            constructionMessage.ContextProperties.Add(new RangeCheckerContextProperty());
        }

        private readonly bool _apply; // Should apply range check?
    }

    /// <summary>
    /// The context properties for the RangeChecker.
    /// </summary>
    class RangeCheckerContextProperty : IContextProperty, IContributeObjectSink
    {
        /// <summary>
        /// Is the new context ok?
        /// </summary>
        /// <param name="newContext">The new context in which the property has been created.</param>
        /// <returns></returns>
        public bool IsNewContextOK(Context newContext)
        {
            return true;
        }

        /// <summary>
        /// Freeze the context so that no more properties can be added.
        /// </summary>
        /// <param name="newContext">The context to freeze.</param>
        public void Freeze(Context newContext) { }

        /// <summary>
        /// Gets the name of the property under which it will be added to the context.
        /// </summary>
        public string Name => "RangeCheckContextProperty";

        /// <summary>
        /// Gets the interception sink.
        /// </summary>
        /// <param name="obj">The server object which provides the message sink that is to be chained in front of the given chain.</param>
        /// <param name="nextSink">The chain of sinks composed so far.</param>
        /// <returns>The range checker interceptor sink.</returns>
        public IMessageSink GetObjectSink(MarshalByRefObject obj, IMessageSink nextSink)
        {
            return new RangeCheckerSink(nextSink);
        }
    }

    /// <summary>
    /// The interception sink for the RangeChecker.
    /// </summary>
    class RangeCheckerSink : IMessageSink
    {
        /// <summary>
        /// Creates a new instance of RangeCheckerSink.
        /// </summary>
        /// <param name="nextSink"> The next sink in the chain.</param>
        public RangeCheckerSink(IMessageSink nextSink)
        {
            this._nextSink = nextSink; // The next message sink in the chain.
        }

        /// <summary>
        /// Gets the next sink in chain.
        /// </summary>
        public IMessageSink NextSink => this._nextSink;

        /// <summary>
        /// Processes asynchronous method calls.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <param name="replySink">The reply sink for the reply message.</param>
        /// <returns></returns>
        public IMessageCtrl AsyncProcessMessage(IMessage message, IMessageSink replySink)
        {
            // We do not do any custom processing for async methods. Just pass them over to the next sink.
            return _nextSink.AsyncProcessMessage(message, replySink);
        }

        /// <summary>
        /// Processes synchronous method calls. Intercepts calls for range checking.
        /// </summary>
        /// <param name="message">The method call message or the constructor call message.</param>
        /// <returns>The processed message.</returns>
        public IMessage SyncProcessMessage(IMessage message)
        {
            IMessage returnMessage;

            // If not a method call message, return after default processing.
            if (!(message is IMethodCallMessage methodCallMessage))
            {
                returnMessage = _nextSink.SyncProcessMessage(message);
            }
            else
            {
                // Do range check on the arguments.
                CheckRangeOnParameters(methodCallMessage);

                // Let the method invocation carry on.
                returnMessage = _nextSink.SyncProcessMessage(message);

                // Check range on the return value.
                CheckRangeOnReturnValue(returnMessage, methodCallMessage);

            }

            return returnMessage;
        }

        /// <summary>
        /// Performs range check on the parameters.
        /// </summary>
        /// <param name="message">The method call message</param>
        private void CheckRangeOnParameters(IMethodCallMessage message)
        {
            // Get the parameters.
            MethodBase method = message.MethodBase;
            ParameterInfo[] parameterInfo = method.GetParameters();
            for (int count = 0; count < parameterInfo.Length; ++count)
            {
                ParameterInfo parameter = parameterInfo[count];
                if (parameter.ParameterType.Equals(typeof(double)))
                {
                    // Get the range attribute if present. Multiple range attributes are not allowed. So get the first/only one if present.
                    if ((parameter.GetCustomAttributes(typeof(RangeAttribute), false).FirstOrDefault() is RangeAttribute range) && range.Enabled)
                    {
                        double value = Convert.ToDouble(message.GetArg(count)); // Argument of the parameter.
                        if ((range.CheckLower && (value < range.Lower)) || (range.CheckUpper && (value > range.Upper)))
                        {
                            string error = $"{method.Name} invoked with argument for parameter {parameter.Name} in position {parameter.Position} being out of range with value {value}";
                            throw new ArgumentOutOfRangeException(error);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Performs range check on the return message.
        /// </summary>
        /// <param name="methodReturnMessage">The return message on which the range check needs be performed.</param>
        /// <param name="methodCallMessage">The method call message.</param>
        private void CheckRangeOnReturnValue(IMessage methodReturnMessage, IMethodCallMessage methodCallMessage)
        {
            // Do no further processing if this is not a return message, or if the return message already carries an exception.
            if ((!(methodReturnMessage is IMethodReturnMessage returnMessage)) || (returnMessage.Exception != null))
            {
                return;
            }

            MethodBase method = methodCallMessage.MethodBase;
            if ((method is MethodInfo) && ((method as MethodInfo).ReturnType.Equals(typeof(double))))
            {
                // Get the range attribute if present.
                if ((method.GetCustomAttributes(typeof(RangeAttribute), false).FirstOrDefault() is RangeAttribute range) && range.Enabled)
                {
                    double value = Convert.ToDouble(returnMessage.ReturnValue); // Return value.
                    if ((range.CheckLower && (value < range.Lower)) || (range.CheckUpper && (value > range.Upper)))
                    {
                        string error = $"{method.Name} returned out of range value of {value}";
                        throw new ArgumentOutOfRangeException(error);
                    }
                }
            }
        }

        private readonly IMessageSink _nextSink; // The next message sink in the chain.
    }
}
