﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class CollapsibleIfs
    {
        public void Test(bool cond1, bool cond2, bool cond3)
        {
            if (cond1)
            {
                if (cond2 || cond3) // Noncompliant
                {
                    ;
                    ;
                    ;
                }
            }
            if (cond1)
                if (cond2 || cond3) // Noncompliant
                {
                    ;
                    ;
                    ;
                }
            if (cond1)
            {
                if (cond2 || cond3)
                {
                    ;
                }
                else
                {
                    
                }
            }
            if (cond1)
            {
                ;
                ;
                ;
                if (cond2 || cond3) 
                {
                    ;
                    ;
                    ;
                }
            }

            if (cond1 && (cond2 || cond3))
            {
                
                    ;
                    ;
                    ;
            }
        }
    }
}