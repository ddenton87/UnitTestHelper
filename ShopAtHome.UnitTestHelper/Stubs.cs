using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Telerik.JustMock;

namespace ShopAtHome.UnitTestHelper
{
    /// <summary>
    /// Provides access to easily-created stub implementations
    /// </summary>
    public class Stubs
    {
        /// <summary>
        /// Returns an empty mock implementation of T with no behavior.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T BareMock<T>()
        {
            var tType = typeof (T);
            return tType.IsPrimitive || tType == typeof(string) ? (T)GetPrimitiveDefault(tType) : Mock.Create<T>();
        }

        private static object GetPrimitiveDefault(Type primitiveType)
        {
            if (primitiveType == typeof (string))
            {
                return string.Empty;
            }
            var gpd = typeof(Stubs).GetMethod(nameof(GetPrimitiveDefaultValue), BindingFlags.Static | BindingFlags.NonPublic);
            return gpd.MakeGenericMethod(primitiveType).Invoke(null, null);
        }

        private static TPrimitive GetPrimitiveDefaultValue<TPrimitive>() where TPrimitive : struct
        {
            return default(TPrimitive);
        }

        /// <summary>
        /// Creates T with all of its dependencies barely mocked
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateWithBareMocks<T>() where T : class
        {
            return Build<T>().FillGapsWithBareMocks().Finish();
        }

        /// <summary>
        /// Initializes the build process of a stub T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static StubBuilder<T> Build<T>() where T : class
        {
            return new StubBuilder<T>();
        }
    }

    /// <summary>
    /// A builder class that handles instantiating T with stubbed dependencies
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StubBuilder<T> where T : class
    {
        private readonly List<object> _constructorParameters;
        private readonly HashSet<Type> _filledParamTypes;
        private readonly List<Type> _constructorParameterTypes;
        private readonly ConstructorInfo _targetConstructor;

        /// <summary>
        /// Initializes building
        /// </summary>
        public StubBuilder()
        {
            var tType = typeof(T);
            // TODO: Allow client to specify which constructor they want to use, if more than one valid constructor is defined
            _targetConstructor =
                tType.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .FirstOrDefault(ci => ci.GetParameters()
                    .Select(p => p.ParameterType)
                    .All(pt => pt.IsInterface || pt.IsAbstract || pt.IsPrimitive || pt == typeof(string)));
            if (_targetConstructor == null)
            {
                // TODO: Need to be able to use constructors with concrete types, only throw if we can't auto-mock an un-filled dependency
                // Probably means we need to pick a ctor to use at the end... Maybe take a page out of depedency injection container book
                throw new InvalidOperationException($"Cannot construct type {tType.FullName} because it has no constructors for which all parameters can be mocked");
            }
            _constructorParameters = new List<object>();
            _filledParamTypes = new HashSet<Type>();
            _constructorParameterTypes = _targetConstructor.GetParameters().Select(p => p.ParameterType).ToList();
        }

        /// <summary>
        /// Uses the provided object as a constructor parameter on T
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public StubBuilder<T> With(object parameter)
        {
            var paramType = parameter.GetType();
            if (_filledParamTypes.Contains(paramType))
            {
                throw new InvalidOperationException($"Already using a {paramType.FullName} to build this object!");
            }

            var validParamTypes = _targetConstructor.GetParameters().Select(p => p.ParameterType);
            if (validParamTypes.All(t => !t.IsAssignableFrom(paramType)))
            {
                throw new InvalidOperationException(
                    $"Cannot build {typeof (T).FullName} with {paramType.FullName} because {paramType.Name} is not one of the parameters for the chosen constructor. " +
                    $"Valid parameters are: {string.Join(", ", validParamTypes.Select(t => t.FullName))}");
            }

            _constructorParameters.Add(parameter);
            _filledParamTypes.Add(paramType);
            return this;
        }

        /// <summary>
        /// Uses the provided objects as constructor parameters on T
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <remarks>This is basically just a wrapper around chaining With() calls</remarks>
        public StubBuilder<T> WithThese(params object[] parameters)
        {
            foreach (var param in parameters)
            {
                With(param);
            }

            return this;
        }

        /// <summary>
        /// Fills any un-assigned ctor dependencies with bare mock implementations
        /// </summary>
        /// <returns></returns>
        public StubBuilder<T> FillGapsWithBareMocks()
        {
            var remainingParameters = _constructorParameterTypes.Where(p => !_filledParamTypes.Any(p.IsAssignableFrom));
            var mockMaker = typeof(Stubs).GetMethod(nameof(Stubs.BareMock), BindingFlags.Static | BindingFlags.Public);

            foreach (var parameterType in remainingParameters)
            {
                _constructorParameters.Add(mockMaker.MakeGenericMethod(parameterType).Invoke(null, null));
            }

            return this;
        }

        /// <summary>
        /// Builds the object
        /// </summary>
        /// <returns></returns>
        public T Finish()
        {
            if (_constructorParameterTypes.Count != _constructorParameters.Count)
            {
                throw new NotImplementedException(
                    $"Constructor parameters not filled! Count of required parameters: {_constructorParameters.Count} \n" +
                    $" Count of filled parameters: {_constructorParameters.Count} \n" +
                    " If you wish to fill unfilled parameters with default (empty) mocks, call FillWithBareMocks before invoking Finish."
                    );
            }

            // No guarantee that the client put their ctor params in here in the correct order, so let's make sure
            _constructorParameters.Sort(new ConstructorParametersComparer(_constructorParameterTypes));

            return _targetConstructor.Invoke(_constructorParameters.ToArray()) as T;
        }

        public static implicit operator T(StubBuilder<T> builder)
        {
            return builder.Finish();
        }
    }

    /// <summary>
    /// Compares constructor parameters in the order that they appear in the signature
    /// </summary>
    public class ConstructorParametersComparer : IComparer<object>
    {
        private readonly List<Type> _constructorParameterTypes;

        /// <summary>
        /// Creates the comparer with the provided, ordered list of constructor parameters 
        /// </summary>
        /// <param name="constructorParameterTypes"></param>
        public ConstructorParametersComparer(List<Type> constructorParameterTypes)
        {
            _constructorParameterTypes = constructorParameterTypes;
        }

        /// <summary>
        /// Compares two parameters by their index in the constructor list
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(object x, object y)
        {
            var xType = x.GetType();
            var yType = y.GetType();
            var xImpl = _constructorParameterTypes.First(t => t.IsAssignableFrom(xType));
            var yImpl = _constructorParameterTypes.First(t => t.IsAssignableFrom(yType));
            return _constructorParameterTypes.IndexOf(xImpl) - _constructorParameterTypes.IndexOf(yImpl);
        }
    }
}
