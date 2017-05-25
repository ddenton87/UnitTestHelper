# API Documentation
## Test Data
The API for generating test data has two components: the TestData static class, and the TestDataBuilder fluent interface.

### Generating Random Value Types
The static TestData class exposes specific methods that return randomized value-type data. This API should be self-discoverable, but here is an example:

	using ShopAtHome.UnitTestHelper;
	var randomNumber = TestData.RandomNumber(9, 99); // Optional params for bounding the result
	var randomString = TestData.RandomString(); // A new unicode string
### Generating Random Complex Types
The library uses GenFu (https://github.com/MisterJames/GenFu)  internally to populate object properties. You can specify specific properties that you want to have pre-determined values.  This functionality is useful because it allows to ensure that your specific testing conditions are enforced, while still checking your methods for cases outside of uninitialized object values. The builder class is implicitly convertible to the underlying T.
	
	using ShopAtHome.UnitTestHelper;
	var allGeneratedData = TestData.Build<ExampleClass>().Finish(); // This object will have every property filled by GenFu
	var someSpecificData = TestData.Build<ExampleClass>().With(example => example.Foo = 24).Finish(); // This object will have every property except for Foo filled by GenFu. Foo will always be 24
	var moreSpecificData = TestData.Build<ExampleClass>().With(example => 
	{
		example.Foo = 24;
		example.Bar = "Hello world!";
	});
## Mocking Dependencies
When testing class functionality, it is important that dependencies of that class are mocked so that their behavior can be controlled during test scenarios. Frequently, this is accomplished by creating mocks of the dependent interfaces using a library like Moq or Telerik's JustMock, and newing-up your class under test, like so:

	var mockDAL = Mock.Create<IDAL>();
	var mockLogger = Mock.Create<ILogger>();
	var myClassUnderTest = new ExampleTest(mockDAL, mockLogger);
	
This has two problems, however:
* It forces you to initialize mocks of dependencies which may not be relevant to your testing scenario
* It is brittle in the face of constructor signature changes. If I add a new dependency to my ExampleTest class - say an IEventRecorder - then all of my test classes will fail to compile.

This library provides an alternatively solution in the Stubs class. It exposes methods that will construct an object using automatically mocked dependencies, and a subset of the total list of dependencies can be explicitly mocked with locally-scoped test mocks. This allows you ignore dependencies which are not relevant for your test cases while still controlling those that are. 

	var mockDAL = Mock.Create<IDAL>();
	var myClassUnderTest = Stubs.Build<ExampleTest>() // Initiates construction of a class with mocked dependencies
							.With(mockDAL) // Tells the builder to use this implementation of a dependency (it locates it by interface type)
							.FillGapsWithBareMocks() // Tells the builder to fill any dependencies that have not been explicitly set with standard Mocks
Using this method, your tests will survive constructor signature changes, and you only need to create mocks for dependencies that are relevant to your test cases. As with the TestBuilder, the StubBuilder is implicitly convertible to the underlying T.

### Mocking library support
This library has dependencies on both JustMock and Moq and ships with implementations for both. You can change the implementation (or substitute your own) by setting the static Stubs.Mocker probperty.