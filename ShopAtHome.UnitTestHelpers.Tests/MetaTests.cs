using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShopAtHome.UnitTestHelper;

namespace ShopAtHome.UnitTestHelpers.Tests
{
    /// <summary>
    /// Tests for the test data / stub manipulation methods
    /// </summary>
    [TestClass]
    public class MetaTests
    {
        public interface IMyInterface
        {
        }

        public interface IMyOtherInterface
        {
        }

        public class MyImplementation : IMyInterface
        {
        }

        public class MyOtherImplementation : IMyOtherInterface
        {
        }

        public class TestClass
        {
            public TestClass(IMyInterface i, IMyOtherInterface j)
            {
            }
        }

        public class TestClass2
        {
            public TestClass2(string p1, int p2) { }
        }

        public class TestClass3
        {
            public TestClass3(TestClass tc, IMyInterface i) { }
        }


        [TestMethod]
        public void CanCreateInstanceWithPrimitiveCtor()
        {
            var result = Stubs.CreateWithBareMocks<TestClass2>();
            result.Should().NotBeNull();

            result = Stubs.Build<TestClass2>().With("foo").FillGapsWithBareMocks().Finish();
            result.Should().NotBeNull();

            result = Stubs.Build<TestClass2>().WithThese(42, "bar").FillGapsWithBareMocks().Finish();
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void CanCreateInstanceWithDefaultMockedDependencies()
        {
            var result = Stubs.CreateWithBareMocks<TestClass>();
            result.Should().NotBeNull("Object instantiation should have occurred.");
        }

        [TestMethod]
        public void CanCreateInstanceWithSpecifiedConcreteDependencyAndDefaultMockedDependencies()
        {
            var dependency = Stubs.CreateWithBareMocks<TestClass>();
            var result = Stubs.Build<TestClass3>().With(dependency).FillGapsWithBareMocks().Finish();
            result.Should().NotBeNull("Object instantiation should have occurred.");
        }

        [TestMethod]
        public void CanCreateInstanceWithSpecifiedDependency()
        {
            var myImpl = new MyImplementation();
            var result = Stubs.Build<TestClass>().With(myImpl).FillGapsWithBareMocks().Finish();
            result.Should().NotBeNull("Object instantiation should have occurred.");
        }

        [TestMethod]
        public void CanCreateInstanceWithSpecifiedDependencies()
        {
            var myImpl = new MyImplementation();
            var myOtherImpl = new MyOtherImplementation();
            var result = Stubs.Build<TestClass>().WithThese(myImpl, myOtherImpl).Finish();
            result.Should().NotBeNull("Object instantiation should have occurred.");

            result = Stubs.Build<TestClass>().With(myImpl).With(myOtherImpl).Finish();
            result.Should()
                .NotBeNull("Object instantiation should have occurred (chaining With should be the same as WithThese)");

            result = Stubs.Build<TestClass>().WithThese(myOtherImpl, myImpl).Finish();
            result.Should()
                .NotBeNull("Object instantiation should have occurred (order of parameters shouldn't matter)");

            result = Stubs.Build<TestClass>().With(myOtherImpl).With(myImpl).Finish();
            result.Should()
                .NotBeNull(
                    "Object instantiation should have occurred (chaining With should be the same as WithThese and order of paremeters shouldn't matter)");
        }

        [TestMethod]
        public void ParameterComparerWorks()
        {
            var parameters = new List<Type>
            {
                typeof (IMyInterface),
                typeof (IMyOtherInterface)
            };

            var comparer = new ConstructorParametersComparer(parameters);

            var myInterfaceImpl = new MyImplementation();
            var myOtherInterfaceImpl = new MyOtherImplementation();

            var result = comparer.Compare(myInterfaceImpl, myOtherInterfaceImpl);
            result.Should()
                .BeLessThan(0, "Because the IMyInterface type comes before the IMyOtherInterface type in the list.");

            result = comparer.Compare(myOtherInterfaceImpl, myInterfaceImpl);
            result.Should()
                .BeGreaterThan(0, "Because the IMyInterface type comes before the IMyOtherInterface type in the list.");

            var sortMe = new List<object> {myOtherInterfaceImpl, myInterfaceImpl};
            sortMe.Sort(comparer);
            sortMe.First()
                .Should()
                .Be(myInterfaceImpl,
                    "Because the comparer should have sorted the list of objects to match its list of interfaces");
        }

        [TestMethod]
        public void RandomPositiveNumber_ShouldBePositive()
        {
            var result = TestData.RandomPositiveNumber();
            result.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void RandomPastDate_ShouldBeInPast()
        {
            var result = TestData.RandomPastDate();
            result.Should().BeOnOrBefore(DateTime.Today);
        }

        [TestMethod]
        public void RandomFutureDate_ShouldBeInFuture()
        {
            var result = TestData.RandomFutureDate();
            result.Should().BeOnOrAfter(DateTime.Today);
        }

        [TestMethod]
        public void RandomCase_WhenToldToFlipCase_DoesSo()
        {
            var testLowercaseString = "mylowercasestring";
            var testUppercaseString = "MYUPPERCASESTRING";
            Func<bool> fairCoin = () => true;

            var lowerCaseResult = testLowercaseString.RandomCase(fairCoin);
            var upperCaseResult = testUppercaseString.RandomCase(fairCoin);

            lowerCaseResult.ShouldBeEquivalentTo(testLowercaseString.ToUpper(),
                "Every lowercase character should have been flipped to uppercase");
            upperCaseResult.ShouldBeEquivalentTo(testUppercaseString.ToLower(),
                "Every uppercase character should have been flipped to lowercase");
        }

        [TestMethod]
        public void RandomCase_WhenToldNotToFlipCase_DoesNot()
        {
            var testLowercaseString = "mylowercasestring";
            var testUppercaseString = "MYUPPERCASESTRING";
            Func<bool> fairCoin = () => false;

            var lowerCaseResult = testLowercaseString.RandomCase(fairCoin);
            var upperCaseResult = testUppercaseString.RandomCase(fairCoin);

            lowerCaseResult.ShouldBeEquivalentTo(testLowercaseString,
                "Every lowercase character should remain lowercase");
            upperCaseResult.ShouldBeEquivalentTo(testUppercaseString,
                "Every uppercase character should remain uppercase");
        }
    }
}
