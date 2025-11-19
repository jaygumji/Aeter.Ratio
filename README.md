# Aeter Ratio
This is a library adding a varierity of functionality.

## IL generation
There is alot of extensions to make IL generation easier to do.

## Dependency injection
An dependency injection framework used in the library. If you want to use another dependency injection framework with other classes in this library, simply make a class that implements IInstanceFactory and implement it with the desired dependency injection framework.

## Dynamic Activator
Similar to the System.Activator class it can create new instances. It generates IL code to makes this process faster with the low downside of taking a small hit on the first time. The instance of DynamicActivator should be reused.

## Serialization
There is a serialization engine, that enables serialization from .NET classes to any format that you write a visitor for. The library will take care of the class analyzis and you can focus on writing the code that manages the format you want to serialize to and/or from.

The current version have support for the following formats:
- JSON
- BSON
- A binary serializer

The serializer classes should be reused during the entire process lifetime to cache the generated classes which handles the hierarchial class traversion. Otherwise the serialization will be very slow.

The serializer engine can have an IInstanceFactory passed via the constructor. This enables you to define how the classes should be created during a deserialization. The DependencyInjectionContainer class already implements this interface and you can register the construction there.
