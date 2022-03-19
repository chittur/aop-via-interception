# Aspect Oriented Programming in .NET via Interception

In 1997, Gregor Kiczales, et al, published a seminal paper on what they called “the young idea of Aspect Oriented Programming” (AOP) at Proc-Europe Conference on object oriented programming [*Aspect-Oriented Programming; Gregor Kiczales, John Lamping, Anurag Mendhekar, ChrisMaeda, Cristina Videira Lopes, Jean-Marc Loingtier andJohn Irwin - Xerox Palo Alto Research Center; European Conference on Object-Oriented Programming (ECOOP), 1997*]. In this paper, they introduce the concept of aspects: In commonly employed code, there are elements that are secondary to the primary functionality of the code. These elements though non-primary are vital to the proper execution of the code. Furthermore, they may be so scattered throughout the system that they contribute to the complexity of the code. These elements are called aspects. Examples of aspects include *security*, *fault-tolerance* and *synchronization*. **Aspect Oriented Programming (AOP)** tries to isolate aspects into separate modules so that the resultant client-code is more purpose-specific, more reusable, and less tangled. In C#, we accomplish this by a process of interception.

Consider an example with a method in a class called Geometry that finds the area of a sector of a circle. We pass the radius and the angle of the sector as parameters; the returned value of area should be half of the square of radius times angle. Again, the method has to make sure that the radius and angle passed as parameters are non-negative; here, we make an assumption that the angle is between 0 and 2 PI. Otherwise, the method has to throw an 
exception. Let us see how we implement the method in the conventional way:

```csharp
public double FindSectorArea(double radius, double angle) 
{ 
  if (radius < 0.0) 
    throw new ArgumentOutOfRangeException("radius", "The radius is out of range."); 
   if ( (angle < 0.0) || (angle > 6.2832) ) 
    throw new ArgumentOutOfRangeException("angle", "The angle is out of range"); 
   return (radius * radius * angle / 2); 
}
```

Here, the actual function of the method is just: 
```csharp
return (radius * radius * angle / 2); 
```

The remaining piece of code is orthogonal to the primary purpose of the method. Isolating the range-checking aspect from the code would, clearly, make the code more readable and purpose-specific, and less complex. This is nicely achieved by the AOP code, which is shown below:

```csharp
[method: Range(true, Lower = 0.0)] 
public double FindSectorArea ([Range(true, Lower = 0.0)] double radius,
                              [Range(true, Lower = 0.0, Upper = 6.28)] double angle) 
{
  return (radius * radius * angle / 2); 
}
```

The code clearly describes its function. The parameter radius has a range-checker attribute, which specifies its lower range as 0. The angle parameter has a range that lies between 0 and 2 PI. Again the method attribute to the method restricts the lower range of the return value to 0. This project here contains functioning code to achieve this in .NET.

## Types of Interception in .NET
.NET supports two kinds of interception: Context interception and Proxy interception. The project here demonstrates both of these kinds. Here are a few drawbacks of each approach.

### Context Interception:
- Requires the object to inherit from ContextBoundObject.
- Adds significant performance hit due to making the objects context bound.
- Uses Channel and Messaging classes from System.Runtime.Remoting which is available only in .NET framework, and deprecated in .NET Core (.NET Core is the way forward).

### Proxy interception:
- Requires that the object be accessed through a proxy for its methods to be intercepted.
- Requires that any attributes be applied to the interface/virtual method, not in the overridden method in the actual class, for interception to reflect on it at runtime.

## Environment:
- Implemented in C# with Visual Studio 2022 Community Edition.

## Project details:
- ***ContextInterception***: Demonstrates context-bound interception.
- ***ContextInterceptionUnitTest***: Unit tests for context-bound interception.
- ***ProxyInterception***: Demonstrates proxy-based interception.
- ***ProxyInterceptionUnitTest***: Unit tests for proxy-based interception.

## Appendix
For more detailed analysis, please see [here](https://ecs.syr.edu/faculty/fawcett/handouts/webpages/AspectOrientedProgramming.htm "here").
