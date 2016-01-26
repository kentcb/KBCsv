// taken from https://github.com/kentcb/TheHelperTrinity/

namespace Kent.Boogaart.KBCsv.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    internal sealed class ExceptionHelper
    {
        private const string typeAttributeName = "type";
        private static readonly IDictionary<ExceptionInfoKey, XDocument> exceptionInfos = new Dictionary<ExceptionInfoKey, XDocument>();
        private static readonly object exceptionInfosLock = new object();
        private readonly Type forType;
        private readonly string resourceName;

        public ExceptionHelper(Type forType)
            : this(forType, null, 0)
        {
        }

        public ExceptionHelper(Type forType, string resourceName)
            : this(forType, resourceName, 0)
        {
            resourceName.AssertNotNullOrWhiteSpace("resource");
        }

        private ExceptionHelper(Type forType, string resourceName, int dummy)
        {
            forType.AssertNotNull("forType");
            this.forType = forType;

            if (resourceName != null)
            {
                this.resourceName = resourceName;
            }
            else
            {
                // here we determine the default name for the resource
                // NOTE: PCL does not have Assembly.GetName()
                this.resourceName = string.Concat(new AssemblyName(forType.GetTypeInfo().Assembly.FullName).Name, ".Properties.ExceptionHelper.xml");
            }
        }

        public Exception Resolve(string exceptionKey, params object[] messageArgs)
        {
            return this.Resolve(exceptionKey, null, null, messageArgs);
        }

        public Exception Resolve(string exceptionKey, Exception innerException, params object[] messageArgs)
        {
            return this.Resolve(exceptionKey, null, innerException, messageArgs);
        }

        public Exception Resolve(string exceptionKey, object[] constructorArgs, Exception innerException)
        {
            return this.Resolve(exceptionKey, constructorArgs, innerException, null);
        }

        public Exception Resolve(string exceptionKey, object[] constructorArgs, params object[] messageArgs)
        {
            return this.Resolve(exceptionKey, constructorArgs, null, messageArgs);
        }

        public Exception Resolve(string exceptionKey, object[] constructorArgs, Exception innerException, params object[] messageArgs)
        {
            exceptionKey.AssertNotNull("exceptionKey");

            var forTypeInfo = forType.GetTypeInfo();
            var exceptionInfo = GetExceptionInfo(forTypeInfo.Assembly, this.resourceName);
            var exceptionNode = (from exceptionGroup in exceptionInfo.Element("exceptionHelper").Elements("exceptionGroup")
                                 from exception in exceptionGroup.Elements("exception")
                                 where string.Equals(exceptionGroup.Attribute("type").Value, this.forType.FullName, StringComparison.Ordinal) && string.Equals(exception.Attribute("key").Value, exceptionKey, StringComparison.Ordinal)
                                 select exception).FirstOrDefault();

            if (exceptionNode == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The exception details for key '{0}' could not be found at /exceptionHelper/exceptionGroup[@type'{1}']/exception[@key='{2}'].", exceptionKey, this.forType, exceptionKey));
            }

            var typeAttribute = exceptionNode.Attribute(typeAttributeName);

            if (typeAttribute == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The '{0}' attribute could not be found for exception with key '{1}'", typeAttributeName, exceptionKey));
            }

            var type = Type.GetType(typeAttribute.Value);
            var typeInfo = type.GetTypeInfo();

            if (type == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' could not be loaded for exception with key '{1}'", typeAttribute.Value, exceptionKey));
            }

            if (!typeof(Exception).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' for exception with key '{1}' does not inherit from '{2}'", type.FullName, exceptionKey, typeof(Exception).FullName));
            }

            var message = exceptionNode.Value.Trim();

            if ((messageArgs != null) && (messageArgs.Length > 0))
            {
                message = string.Format(CultureInfo.InvariantCulture, message, messageArgs);
            }

            var constructorArgsList = new List<object>();

            // message is always first
            constructorArgsList.Add(message);

            // next, any additional constructor args
            if (constructorArgs != null)
            {
                constructorArgsList.AddRange(constructorArgs);
            }

            // finally, the inner exception, if any
            if (innerException != null)
            {
                constructorArgsList.Add(innerException);
            }

            // find the most suitable constructor given the parameters and available constructors
            var constructorArgsArr = constructorArgsList.ToArray();
            var constructor = (from candidateConstructor in typeInfo.DeclaredConstructors
                               where candidateConstructor.IsPublic && !candidateConstructor.IsStatic
                               let rank = RankArgumentsAgainstParameters(constructorArgsArr, candidateConstructor.GetParameters())
                               where rank > 0
                               orderby rank descending
                               select candidateConstructor).FirstOrDefault();

            if (constructor == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "An appropriate constructor could not be found for exception type '{0}, for exception with key '{1}'", type.FullName, exceptionKey));
            }

            return (Exception)constructor.Invoke(constructorArgsArr);
        }

        public void ResolveAndThrowIf(bool condition, string exceptionKey, params object[] messageArgs)
        {
            if (condition)
            {
                throw this.Resolve(exceptionKey, messageArgs);
            }
        }

        public void ResolveAndThrowIf(bool condition, string exceptionKey, Exception innerException, params object[] messageArgs)
        {
            if (condition)
            {
                throw this.Resolve(exceptionKey, innerException, messageArgs);
            }
        }

        public void ResolveAndThrowIf(bool condition, string exceptionKey, object[] constructorArgs, Exception innerException)
        {
            if (condition)
            {
                throw this.Resolve(exceptionKey, constructorArgs, innerException);
            }
        }

        public void ResolveAndThrowIf(bool condition, string exceptionKey, object[] constructorArgs, params object[] messageArgs)
        {
            if (condition)
            {
                throw this.Resolve(exceptionKey, constructorArgs, messageArgs);
            }
        }

        public void ResolveAndThrowIf(bool condition, string exceptionKey, object[] constructorArgs, Exception innerException, params object[] messageArgs)
        {
            if (condition)
            {
                throw this.Resolve(exceptionKey, constructorArgs, innerException, messageArgs);
            }
        }

        private static XDocument GetExceptionInfo(Assembly assembly, string resourceName)
        {
            var retVal = (XDocument)null;
            var exceptionInfoKey = new ExceptionInfoKey(assembly, resourceName);

            lock (exceptionInfosLock)
            {
                if (exceptionInfos.ContainsKey(exceptionInfoKey))
                {
                    retVal = exceptionInfos[exceptionInfoKey];
                }
                else
                {
                    var stream = assembly.GetManifestResourceStream(resourceName);

                    if (stream == null)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "XML resource file '{0}' could not be found in assembly '{1}'.", resourceName, assembly.FullName));
                    }

                    using (var streamReader = new StreamReader(stream))
                    {
                        retVal = XDocument.Load(streamReader);
                    }

                    exceptionInfos[exceptionInfoKey] = retVal;
                }
            }

            return retVal;
        }

        // the higher the rank, the more suited the arguments are to fitting into the given parameters
        // a rank of zero means completely unsuitable and should be ignored
        private static int RankArgumentsAgainstParameters(object[] arguments, ParameterInfo[] parameters)
        {
            if (arguments.Length != parameters.Length)
            {
                return 0;
            }

            var runningRank = 0;

            for (var i = 0; i < arguments.Length; ++i)
            {
                var parameterRank = RankArgumentAgainstParameter(arguments[i], parameters[i]);

                if (parameterRank == 0)
                {
                    return 0;
                }

                runningRank += parameterRank;
            }

            return runningRank;
        }

        // rank an individual argument's suitability to fit the given parameter
        // a rank of zero means completely unsuitable
        private static int RankArgumentAgainstParameter(object argument, ParameterInfo parameter)
        {
            var parameterTypeInfo = parameter.ParameterType.GetTypeInfo();

            if (argument == null)
            {
                // limited what we can do when we have no type for the argument
                if (parameterTypeInfo.IsValueType && Nullable.GetUnderlyingType(parameter.ParameterType) == null)
                {
                    // parameter is not nullable, but argument is null
                    return 0;
                }

                // null fits into this parameter
                return 1;
            }

            if (!parameterTypeInfo.IsAssignableFrom(argument.GetType().GetTypeInfo()))
            {
                // argument is not assignable to parameter type
                return 0;
            }

            return 2;
        }

        private struct ExceptionInfoKey : IEquatable<ExceptionInfoKey>
        {
            private readonly Assembly assembly;
            private readonly string resourceName;

            public ExceptionInfoKey(Assembly assembly, string resourceName)
            {
                this.assembly = assembly;
                this.resourceName = resourceName;
            }

            public bool Equals(ExceptionInfoKey other)
            {
                return other.assembly.Equals(this.assembly) && string.Equals(other.resourceName, this.resourceName, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ExceptionInfoKey))
                {
                    return false;
                }

                return this.Equals((ExceptionInfoKey)obj);
            }

            public override int GetHashCode()
            {
                var hash = 17;
                hash = (hash * 23) + this.assembly.GetHashCode();
                hash = (hash * 23) + this.resourceName.GetHashCode();
                return hash;
            }
        }
    }
}