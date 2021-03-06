﻿using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class MyClass
    {
        class Nested
        {
            private int field; // Noncompliant, shouldn't it be initialized? This way the value is always default(int), 0.
            private int field2;
            private static int field3; // Noncompliant
            private static int field4;

            private static int field5; //reported by unused member rule

            private static int field6 = 42;
            private readonly int field7; // Noncompliant
            private int Property { get; }  // Noncompliant
            private int Property2 { get; }
            private int Property3 { get; } = 42; // Unused, S1144 reports on it

            private int Property4 { get; set; }  // Noncompliant
            private int Property5 { get; set; } = 42;
            private int Property6 { get { return 42; } set { } }

            public Nested()
            {
                Property2 = 42;
            }

            public void Print()
            {
                Console.WriteLine((field)); //Will always print 0
                Console.WriteLine((field6));
                Console.WriteLine((field7));
                Console.WriteLine((Property));
                Console.WriteLine((Property4));
                Console.WriteLine((Property6));

                Console.WriteLine(this.field); //Will always print 0
                Console.WriteLine(MyClass.Nested.field3); //Will always print 0
                new MyClass().M(ref MyClass.Nested.field4);

                this.field2 = 10;
                field2 = 10;
            }
        }

        public void M(ref int f) { }
    }
}
