using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class FormulaEvaluator
{
    /// <summary>
    /// Evaluates a formula string recursively.
    /// Assumes functions have comma-separated arguments and no infix operators within arguments.
    /// E.g., "DIV(1,MUL(4,SUM(3,DIF(8,6))))"
    /// </summary>
    /// <param name="formula">The formula string to evaluate.</param>
    /// <returns>The calculated double result.</returns>
    /// <exception cref="ArgumentException">Thrown for invalid formula format or unknown functions.</exception>
    /// <exception cref="DivideByZeroException">Thrown for division by zero.</exception>
    public static double EvaluateFormula(string formula)
    {
        // Trim whitespace from the formula for cleaner processing
        formula = formula.Trim();

        // Base case: If the 'formula' is a simple number, parse and return it.
        if (double.TryParse(formula, out double numericValue))
        {
            return numericValue;
        }

        // Regular expression to extract the function name and its entire arguments string.
        // Pattern: ^([A-Z]+)\((.*)\)$
        // - ^       : Matches the start of the string.
        // - ([A-Z]+): Capturing group 1 - Matches one or more uppercase letters (the function name).
        // - \(      : Matches a literal opening parenthesis.
        // - (.*)    : Capturing group 2 - Matches any character (except newline) zero or more times (the entire arguments string).
        // - \)      : Matches a literal closing parenthesis.
        // - $       : Matches the end of the string.
        var regex = new Regex(@"^([A-Z]+)\((.*)\)$");
        Match match = regex.Match(formula);

        // If the regex doesn't match, it means the input isn't a valid function call (and also not a number).
        if (!match.Success)
        {
            throw new ArgumentException($"Invalid formula format or unhandled expression: '{formula}'");
        }

        string funcName = match.Groups[1].Value;       // e.g., "DIV", "SUM"
        string argsString = match.Groups[2].Value;     // e.g., "1,MUL(4,SUM(3,DIF(8,6)))"

        // --- Logic to split arguments, correctly handling nested parentheses ---
        // This is crucial because a comma might appear inside a nested function's arguments
        // (e.g., the comma in DIF(8,6) should not split the arguments for SUM).
        var args = new List<string>();
        int balance = 0;           // Tracks parenthesis balance: increment for '(', decrement for ')'
        System.Text.StringBuilder currentArgBuilder = new System.Text.StringBuilder(); // Accumulates characters for the current argument

        for (int i = 0; i < argsString.Length; i++)
        {
            char char_ = argsString[i]; // Renamed to avoid keyword collision

            if (char_ == '(')
            {
                balance++;
            }
            else if (char_ == ')')
            {
                balance--;
            }

            // If a comma is found AND parentheses are balanced (meaning it's not inside a nested function call)
            if (char_ == ',' && balance == 0)
            {
                args.Add(currentArgBuilder.ToString().Trim()); // Add the accumulated argument (trimmed for whitespace)
                currentArgBuilder.Clear();                     // Reset for the next argument
            }
            else
            {
                currentArgBuilder.Append(char_);               // Otherwise, append the character to the current argument
            }
        }
        args.Add(currentArgBuilder.ToString().Trim()); // Add the last accumulated argument after the loop finishes

        // Recursively evaluate each argument:
        // For each string argument (e.g., "1", "MUL(4,SUM(3,DIF(8,6)))"),
        // call EvaluateFormula again. This is where the recursion happens.
        var evaluatedArgs = new List<double>();
        foreach (string arg in args)
        {
            evaluatedArgs.Add(EvaluateFormula(arg));
        }

        // Perform the operation based on the function name.
        switch (funcName.ToUpperInvariant()) // Use InvariantCulture for case-insensitive matching of function names
        {
            case "SUM":
                // SUM can take multiple arguments; LINQ's Sum extension method works perfectly.
                return evaluatedArgs.Sum();
            case "DIF": // Difference: arg1 - arg2
                if (evaluatedArgs.Count != 2)
                {
                    throw new ArgumentException($"DIF requires exactly two arguments, but received {evaluatedArgs.Count} in: '{formula}'");
                }
                return evaluatedArgs[0] - evaluatedArgs[1];
            case "MUL": // Multiplication: arg1 * arg2 * ...
                // MUL can take multiple arguments; LINQ's Aggregate for product.
                return evaluatedArgs.Aggregate(1.0, (acc, val) => acc * val);
            case "DIV": // Division: arg1 / arg2
                if (evaluatedArgs.Count != 2)
                {
                    throw new ArgumentException($"DIV requires exactly two arguments, but received {evaluatedArgs.Count} in: '{formula}'");
                }
                if (evaluatedArgs[1] == 0)
                {
                    throw new DivideByZeroException($"Division by zero encountered in: '{formula}'");
                }
                return evaluatedArgs[0] / evaluatedArgs[1];
            default:
                throw new ArgumentException($"Unknown function: '{funcName}' in formula: '{formula}'");
        }
    }

    public static void Main(string[] args)
    {
        // Example usage with your specified input format:
        string formulaInput = "DIV(1,MUL(4,SUM(3,DIF(8,6))))";

        try
        {
            double result = EvaluateFormula(formulaInput);
            Console.WriteLine($"Input Formula: {formulaInput}");
            Console.WriteLine($"Calculated Result: {result}"); // Expected: 0.05
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error evaluating formula: {ex.Message}");
        }

        // Another example:
        string simpleSum = "SUM(10,20,5)";
        try
        {
            double result = EvaluateFormula(simpleSum);
            Console.WriteLine($"\nInput Formula: {simpleSum}");
            Console.WriteLine($"Calculated Result: {result}"); // Expected: 35
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error evaluating formula: {ex.Message}");
        }

        // Example with nested structure and zero division error
        string errorFormula = "DIV(10,DIF(5,5))";
        try
        {
            double result = EvaluateFormula(errorFormula);
            Console.WriteLine($"\nInput Formula: {errorFormula}");
            Console.WriteLine($"Calculated Result: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError evaluating formula: {ex.Message}"); // Expected: Division by zero
        }
    }
}