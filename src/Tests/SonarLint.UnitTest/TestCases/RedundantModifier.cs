﻿namespace Tests.Diagnostics
{
    public class C1
    {
        public virtual void MyNotOverridenMethod() { }
    }
    internal partial class Partial1Part //Noncompliant
    {
        void Method() { }
    }
    partial struct PartialStruct //Noncompliant
    {
    }
partial interface PartialInterface //Noncompliant
    {
    }

    internal partial class Partial2Part
    {
    }

    internal partial class Partial2Part
    {
        public virtual void MyOverridenMethod() { }
        public virtual int Prop { get; set; }
    }
    internal class Override : Partial2Part
    {
        public override void MyOverridenMethod() { }
    }
    sealed class SealedClass : Partial2Part
    {
        public override sealed void MyOverridenMethod() { } //Noncompliant
        public override sealed int Prop { get; set; } //Noncompliant
    }

    internal class BaseClass<T>
    {
        public virtual string Process(string input)
        {
            return input;
        }
    }

    internal class SubClass : BaseClass<string>
    {
        public override string Process(string input)
        {
            return "Test";
        }
    }

    unsafe class UnsafeClass
    {
        int* pointer;
    }

    unsafe class UnsafeClass2 // Noncompliant
    {
        int num;
    }
    unsafe class UnsafeClass3 // Noncompliant
    {
        unsafe void M() // Noncompliant
        {

        }
    }

    class Class4
    {
        unsafe interface MyInterface
        {
            unsafe int* Method(); // Noncompliant
        }

        private unsafe delegate void MyDelegate(int* p);
        private unsafe delegate void MyDelegate2(int i); // Noncompliant

        unsafe class Inner { } // Noncompliant

        unsafe event MyDelegate MyEvent; // Noncompliant
        unsafe event MyDelegate MyEvent2
        {
            add
            {
                int* p;
            }
            remove { }
        }

        unsafe ~Class4() // Noncompliant
        {
        }
        void M()
        {
            Point pt = new Point();
            unsafe
            {
                fixed (int* p = &pt.x)
                {
                    *p = 1;
                }
            }

            unsafe
            {
                unsafe // Noncompliant
                {
                    unsafe // Noncompliant
                    {
                        var i = 1;
                        int* p = &i;
                    }
                }
            }
        }
    }

    public class Foo
    {
        public class Bar
        {
            public unsafe class Baz // Noncompliant
            {
            }
        }
    }

    public unsafe class Foo2
    {
        public unsafe class Bar // Noncompliant
        {
            private int* p;

            public unsafe class Baz // Noncompliant
            {
                private int* p2;
            }
        }
    }

    public class Checked
    {
        public static void M()
        {
            checked
            {
                checked // Noncompliant
                {
                    var z = 1 + 4;
                    var y = unchecked(1 +
                        unchecked(4)); // Noncompliant
                }
            }

            checked // Noncompliant
            {
                var f = 5.5;
                var y = unchecked(5 + 4);
            }

            checked
            {
                var f = 5.5;
                var x = 5 + 4;
                var y = unchecked(5 + 4);
            }

            checked
            {
                var f = 5.5;
                var x = 5 + 4;
                var y = unchecked(5.5 + 4); // Noncompliant
            }

            unchecked
            {
                var f = 5.5;
                var y = unchecked(5 + 4); // Noncompliant
            }

            checked
            {
                var x = (uint)10;
                var y = (int)x;
            }

            checked // Noncompliant
            {
                var x = 10;
                var y = (double)x;
            }

            checked
            {
                var x = 10;
                x += int.MaxValue;
            }
        }
    }
}
