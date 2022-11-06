using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Util 
{

    public static List<int> getFactors(int number){
        var factors = new List<int>();

        for (int i = 1; i < number; i++){
            if (number % i == 0){
                factors.Add(i);
            }
        }

        factors.Add(number);

        return factors;
    }
   public static List<int> Factor(int number) 
{
    var factors = new List<int>();
    int max = (int)Math.Sqrt(number);  // Round down

    for (int factor = 1; factor <= max; ++factor) // Test from 1 to the square root, or the int below it, inclusive.
    {  
        if (number % factor == 0) 
        {
            factors.Add(factor);
            if (factor != number/factor) // Don't add the square root twice!  Thanks Jon
                factors.Add(number/factor);
        }
    }
    return factors;
}
}
