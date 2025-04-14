#pragma warning disable CS9113 // Parameter is unread.
using System.Diagnostics.CodeAnalysis;
using Barrel.Utils;

namespace BarrelTest.Units.Utils;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "EmptyConstructor")]
[SuppressMessage("ReSharper", "UnusedParameter.Local")]
public class ArgumentLessConstructorCheckTest
{
    [Fact]
    public void PrivateConstructor_ReturnsFalse()
    {
        var result = ArgumentLessConstructorChecker.HasArgumentLessConstructor(typeof(PrivateConstructorClass));

        Assert.False(result);
    }

    [Fact]
    public void ProtectedConstructor_ReturnsFalse()
    {
        var result = ArgumentLessConstructorChecker.HasArgumentLessConstructor(typeof(ProtectedConstructorClass));

        Assert.False(result);
    }

    [Fact]
    public void InternalConstructor_ReturnsFalse()
    {
        var result = ArgumentLessConstructorChecker.HasArgumentLessConstructor(typeof(InternalConstructorClass));

        Assert.False(result);
    }

    [Fact]
    public void WithArguments_ReturnsFalse()
    {
        var result = ArgumentLessConstructorChecker.HasArgumentLessConstructor(typeof(ParametersClass));

        Assert.False(result);
    }

    [Fact]
    public void WithDefaultValuesArguments_ReturnsTrue()
    {
        var result = ArgumentLessConstructorChecker.HasArgumentLessConstructor(typeof(DefaultValueArgumentsClass));

        Assert.True(result);
    }

    [Fact]
    public void WithNoParameter_ReturnsTrue()
    {
        var result = ArgumentLessConstructorChecker.HasArgumentLessConstructor(typeof(NoParameterClass));

        Assert.True(result);
    }

    [Fact]
    public void WithMultipleConstructors_ReturnsTrue()
    {
        var result = ArgumentLessConstructorChecker.HasArgumentLessConstructor(typeof(MultipleConstructorsClass));

        Assert.True(result);
    }

    private class PrivateConstructorClass
    {
        private PrivateConstructorClass(){}
    }

    private class ProtectedConstructorClass
    {
        protected ProtectedConstructorClass(){}
    }

    private class InternalConstructorClass
    {
        internal InternalConstructorClass(){}
    }

    private class ParametersClass(int _);

    private class DefaultValueArgumentsClass(int _ = 0);

    private class NoParameterClass;

    private class MultipleConstructorsClass
    {
        private MultipleConstructorsClass(bool _, bool __){}
        internal MultipleConstructorsClass(int _){}
        public MultipleConstructorsClass(){}
    }
}
