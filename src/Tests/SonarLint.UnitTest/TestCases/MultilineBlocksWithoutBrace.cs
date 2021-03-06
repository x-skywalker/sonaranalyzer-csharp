﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class MultilineBlocksWithoutBrace
    {
        public static void Tests() { throw new Exception();}
        public static int SomeMethod(int a)
        {
            if (a == 1)
                a++;
                return a; // Noncompliant

            if (true)
            { }
            else if (a == 2)
            {

            }
            else if (a == 1)
                a *= 3;

                return a; // Noncompliant

            while (true)
                while (true)
                    a++; /*comment */
    /**/            return a; // Noncompliant


//            String empty = "";
            return a + 10;
        }

        public void Test()
        {
            while (true)
                Tests();
                Tests(); // Noncompliant

            while (true)
Tests();
Tests(); // Noncompliant

            if (true)
                Tests();
            Tests();

            while (true)
            {
                Tests();
            }
            Tests();

            if (true)
                Tests();

                Tests(); // Noncompliant

            if (true)
                Tests();
            else
                Tests();
                Tests(); // Noncompliant

            while (true)
                Tests();
   /*comment*/  Tests(); // Noncompliant

            while (true)
                Tests();
            /*comment*/
                Tests(); // Noncompliant
        }

        public void Test2()
        {
            int i = 1;
            if (true)
                i = 2;
            else
            if (false)
            {
                i = 3;
            }
            i = 4; // Compliant

            if (b)
                i = 2;
            else
            if (b)
                i = 2;
            else
            if (b)
                i = 2;
            else
            if (b)
                i = 2;
            i = 4; // Compliant

            if (i==45)
            {
                ;
            }
            else
            foreach(var j in new[] { 1 })
            {
                    ; ; ;
            }

            if (x) // Compliant
            {
                ; ;
            }

            if (i==45)
            {
                ;
            }
            else
            foreach(var j in new[] { 1 })
            ;

            if (x)  // Noncompliant, but should report only once
            {
                ; ;
            }

            if (true)
                ;
            else
            if (false)
                ;
            ; // Compliant
        }
    }
}
