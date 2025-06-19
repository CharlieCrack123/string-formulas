function evaluateFormula(formula) {debugger
    // Base case: If the 'formula' is a simple number, parse and return it.
    // This handles the deepest parts of the recursion (e.g., '1', '4', '3', '8', '6').
    if (!isNaN(formula) && !isNaN(parseFloat(formula))) {
        return parseFloat(formula);
    }

    // Regular expression to extract the function name and its entire arguments string.
    // ^([A-Z]+)\((.*)\)$
    // - ^       : Matches the start of the string.
    // - ([A-Z]+): Capturing group 1 - Matches one or more uppercase letters (the function name, e.g., "DIV", "SUM").
    // - \(      : Matches a literal opening parenthesis.
    // - (.*)    : Capturing group 2 - Matches any character (except newline) zero or more times (the entire arguments string).
    // - \)      : Matches a literal closing parenthesis.
    // - $       : Matches the end of the string.
    const match = formula.match(/^([A-Z]+)\((.*)\)$/);

    // If the regex doesn't match, it means the input isn't a valid function call (and also not a number).
    if (!match) {
        throw new Error("Invalid formula format or unhandled expression: " + formula);
    }

    const funcName = match[1];      // e.g., "DIV", "MUL", "SUM", "DIF"
    let argsString = match[2];      // e.g., "1,MUL(4,SUM(3,DIF(8,6)))"

    // --- Logic to split arguments, correctly handling nested parentheses ---
    // This is crucial because a comma might appear inside a nested function's arguments
    // (e.g., the comma in DIF(8,6) should not split the arguments for SUM).
    const args = [];
    let balance = 0;           // Tracks parenthesis balance: increment for '(', decrement for ')'
    let currentArg = '';       // Accumulates characters for the current argument

    for (let i = 0; i < argsString.length; i++) {
        const char = argsString[i];
        if (char === '(') {
            balance++;
        } else if (char === ')') {
            balance--;
        }

        // If a comma is found AND parentheses are balanced (meaning it's not inside a nested function call)
        if (char === ',' && balance === 0) {
            args.push(currentArg.trim()); // Add the accumulated argument (trimmed for whitespace)
            currentArg = '';             // Reset for the next argument
        } else {
            currentArg += char;          // Otherwise, append the character to the current argument
        }
    }
    args.push(currentArg.trim()); // Add the last accumulated argument after the loop finishes

    // Recursively evaluate each argument:
    // For each string argument (e.g., "1", "MUL(4,SUM(3,DIF(8,6)))"),
    // call evaluateFormula again. This is where the recursion happens.
    const evaluatedArgs = args.map(arg => evaluateFormula(arg));

    // Perform the operation based on the function name.
    switch (funcName) {
        case 'SUM':
            // SUM can take multiple arguments; reduce sums them all.
            return evaluatedArgs.reduce((acc, val) => acc + val, 0);
        case 'DIF':
            // DIF (Difference) assumes two arguments: arg1 - arg2.
            if (evaluatedArgs.length !== 2) {
                throw new Error("DIF requires exactly two arguments, but received " + evaluatedArgs.length + " in: " + formula);
            }
            return evaluatedArgs[0] - evaluatedArgs[1];
        case 'MUL':
            // MUL (Multiply) can take multiple arguments; reduce multiplies them all.
            return evaluatedArgs.reduce((acc, val) => acc * val, 1);
        case 'DIV':
            // DIV (Divide) assumes two arguments: arg1 / arg2.
            if (evaluatedArgs.length !== 2) {
                throw new Error("DIV requires exactly two arguments, but received " + evaluatedArgs.length + " in: " + formula);
            }
            if (evaluatedArgs[1] === 0) {
                throw new Error("Division by zero encountered in: " + formula);
            }
            return evaluatedArgs[0] / evaluatedArgs[1];
        default:
            throw new Error("Unknown function: " + funcName + " in formula: " + formula);
    }
}

// Example usage with your specified input format:
// const formulaInput = "DIV(1,MUL(4,SUM(3,DIF(8,6))))";
// const result = evaluateFormula(formulaInput);

// console.log("Input Formula:", formulaInput);
// console.log("Calculated Result:", result);

// Let's trace the calculation:
// 1. evaluateFormula("DIV(1,MUL(4,SUM(3,DIF(8,6))))")
//    - funcName: "DIV", argsString: "1,MUL(4,SUM(3,DIF(8,6)))"
//    - args: ["1", "MUL(4,SUM(3,DIF(8,6)))"]
//    - evaluates args:
//      - evaluateFormula("1") -> 1 (base case)
//      - evaluateFormula("MUL(4,SUM(3,DIF(8,6)))")
//        - funcName: "MUL", argsString: "4,SUM(3,DIF(8,6)))"
//        - args: ["4", "SUM(3,DIF(8,6)))"]
//        - evaluates args:
//          - evaluateFormula("4") -> 4 (base case)
//          - evaluateFormula("SUM(3,DIF(8,6)))")
//            - funcName: "SUM", argsString: "3,DIF(8,6)))"
//            - args: ["3", "DIF(8,6)))"]
//            - evaluates args:
//              - evaluateFormula("3") -> 3 (base case)
//              - evaluateFormula("DIF(8,6)))")
//                - funcName: "DIF", argsString: "8,6"
//                - args: ["8", "6"]
//                - evaluates args:
//                  - evaluateFormula("8") -> 8 (base case)
//                  - evaluateFormula("6") -> 6 (base case)
//                - Returns DIF(8,6) = 8 - 6 = 2
//            - Returns SUM(3,2) = 3 + 2 = 5
//        - Returns MUL(4,5) = 4 * 5 = 20
//    - Returns DIV(1,20) = 1 / 20 = 0.05

document.querySelector('#submitButton').onclick = ()=>{
    const formula = document.querySelector('#formulaInput').value;
    const value = evaluateFormula(formula)

    console.log("Input Formula:", formula);
    console.log("Calculated Result:", value);
    alert("Calculated Result: " + value);
};