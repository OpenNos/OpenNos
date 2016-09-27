> This document is stolen from [jittuu's Coding Guidelines](https://msdn.microsoft.com/en-us/library/ff926074.aspx).

# Coding Guidelines
Let's face it. No matter what coding guidelines we choose, we're not going to make everyone happy. While we would like to embrace everyone's individual style, working together on the same codebase would be utter chaos if we don't enforce some consistency. When it comes to coding guidelines, consistency can be even more important than being "right."

## Definitions

- [Camel case][] is a casing convention where the first letter is lower-case, words are not separated by any character but have their first letter capitalized. Example: `thisIsCamelCased`.
- [Pascal case][] is a casing convention where the first letter of each word is capitalized, and no separating character is included between words. Example: `ThisIsPascalCased`.

## C# coding conventions

### Tabs & Indenting
Tab characters `\0x09` should not be used in code. All indentation should be done with 4 space characters.

### Bracing
Open braces should always be at the beginning of the line after the statement that begins the block. Contents of the brace should be indented by 4 spaces. For example:
```csharp
if (someExpression)
{
    DoSomething();
}
else
{
    DoSomethingElse();
}
```
Braces should never be considered optional. Even for single statement blocks, you should always use braces. This increases code readability and maintainability.
```csharp
for (int i=0; i < 100; i++) { DoSomething(i); }
```

### Single line statements
Single line statements can have braces that begin and end on the same line.

```csharp
public class Foo
{
    int bar;

    public int Bar
    {
      get { return bar; }
      set { bar = value; }
    }
}
```

### Commenting
Only comment the "**Why**" and not the "What". Jeff posted a good [blog post][jeff-comment] about it.

### Naming
- :x: DO NOT use Hungarian notation
- :white_check_mark: DO use a prefix with an underscore `_` and camelCased for private fields.
- :white_check_mark: DO use camelCasing for member variables
- :white_check_mark: DO use camelCasing for parameters
- :white_check_mark: DO use camelCasing for local variables
- :white_check_mark: DO use PascalCasing for function, property, event, and class names
- :white_check_mark: DO prefix interfaces names with “I”
- :x: DO NOT prefix enums, classes, or delegates with any letter

### Region
 - :white_check_mark: DO use region where useful.

## StyleCop

We use [StyleCop](http://stylecop.codeplex.com/) with the following rules to ensure that we have the same coding style in the organization.

### Layout Rules

`SA1500-SA1518, except SA1513`

 - [SA1500][]: Curly brackets for multiline statements must not share line
 - [SA1501][]: Statement must not be on single line
 - [SA1502][]: Element must not be on single line
 - [SA1503][]: Curly brackets must not be omitted
 - [SA1504][]: All accessors must be multiline or single line
 - [SA1505][]: Opening curly brackets must not be followed by blank line
 - [SA1506][]: Element documentation headers must not be followed by blankline
 - [SA1507][]: Code must not contain multiple blank lines in a row
 - [SA1508][]: Closing curly brackets must not be preceded by blank line
 - [SA1509][]: Opening curly brackets must not be precededed by blank line
 - [SA1510][]: Chained statement blocks must not be preceded by blank line
 - [SA1511][]: While do footer must not be preceded by blank line
 - [SA1512][]: Single line comments must not be followed by blank line
 - [SA1514][]: Element documentation header must be preceded by blank line
 - [SA1515][]: Single line comment must be preceded by blank line
 - [SA1516][]: Elements must be separated by blank line
 - [SA1517][]: Code must not contain blank lines at start of file
 - [SA1518][]: Code must not contain blank lines at end of file

[SA1500]: http://www.stylecop.com/docs/SA1500.html
[SA1501]: http://www.stylecop.com/docs/SA1501.html
[SA1502]: http://www.stylecop.com/docs/SA1502.html
[SA1503]: http://www.stylecop.com/docs/SA1503.html
[SA1504]: http://www.stylecop.com/docs/SA1504.html
[SA1505]: http://www.stylecop.com/docs/SA1505.html
[SA1506]: http://www.stylecop.com/docs/SA1506.html
[SA1507]: http://www.stylecop.com/docs/SA1507.html
[SA1508]: http://www.stylecop.com/docs/SA1508.html
[SA1509]: http://www.stylecop.com/docs/SA1509.html
[SA1510]: http://www.stylecop.com/docs/SA1510.html
[SA1511]: http://www.stylecop.com/docs/SA1511.html
[SA1512]: http://www.stylecop.com/docs/SA1512.html
[SA1514]: http://www.stylecop.com/docs/SA1514.html
[SA1515]: http://www.stylecop.com/docs/SA1515.html
[SA1516]: http://www.stylecop.com/docs/SA1516.html
[SA1517]: http://www.stylecop.com/docs/SA1517.html
[SA1518]: http://www.stylecop.com/docs/SA1518.html


### Maintainability Rules

 - [SA1403]: File may only contain a single namespace
 - [SA1404]: Code analysis suppression must have justification

[SA1403]: http://www.stylecop.com/docs/SA1403.html
[SA1404]: http://www.stylecop.com/docs/SA1404.html


### Naming Rules

`SA1300-SA1311, except SA1306 and SA1309`

 - [SA1300][]: Element must begin with upper case letter
 - [SA1301][]: Element must begin with lower case letter
 - [SA1302][]: Interface names must begin with `I`
 - [SA1303][]: Const field names must begin with upper case letter
 - [SA1304][]: Non private readonly fields must begin with upper case letter
 - [SA1305][]: Field names must not use hungarian notation
 - [SA1307][]: Accessible fields must begin with upper case letter
 - [SA1308][]: Variable names must not be prefixed
 - [SA1310][]: Field names must not contain underscore
 - [SA1311][]: Static readonly fields must begin with upper case letter

[SA1300]: http://www.stylecop.com/docs/SA1300.html
[SA1301]: http://www.stylecop.com/docs/SA1301.html
[SA1302]: http://www.stylecop.com/docs/SA1302.html
[SA1303]: http://www.stylecop.com/docs/SA1303.html
[SA1304]: http://www.stylecop.com/docs/SA1304.html
[SA1305]: http://www.stylecop.com/docs/SA1305.html
[SA1307]: http://www.stylecop.com/docs/SA1307.html
[SA1308]: http://www.stylecop.com/docs/SA1308.html
[SA1310]: http://www.stylecop.com/docs/SA1310.html
[SA1311]: http://www.stylecop.com/docs/SA1311.html

### Readability Rules

`SA1106-SA1108`

 - [SA1106][]: Code must not contain empty statements
 - [SA1107][]: Code must not contain multiple statements on one line
 - [SA1108][]: Block statements must not contain embedded comments

[SA1106]: http://www.stylecop.com/docs/SA1106.html
[SA1107]: http://www.stylecop.com/docs/SA1107.html
[SA1108]: http://www.stylecop.com/docs/SA1108.html

### Spacing Rules

`SA1000-1027`

 - [SA1000][]: Keywords must be spaced correctly
 - [SA1001][]: Commas must be spaced correctly
 - [SA1002][]: Semicolons must be spaced correctly
 - [SA1003][]: Symbols must be spaced correctly
 - [SA1004][]: Documentation lines must begin with single space
 - [SA1005][]: Single line comments must begin with singe space
 - [SA1006][]: Preprocessor keywords must not be preceded by space
 - [SA1007][]: Operator keyword must be followed by space
 - [SA1008][]: Opening parenthesis must be spaced correctly
 - [SA1009][]: Closing parenthesis must be spaced correctly
 - [SA1010][]: Opening square brackets must be spaced correctly
 - [SA1011][]: Closing square brackets must be spaced correctly
 - [SA1012][]: Opening curly brackets must be spaced correctly
 - [SA1013][]: Closing curly brackets must be spaced correctly
 - [SA1014][]: Opening generic brackets must be spaced correctly
 - [SA1015][]: Closing generic brackets must be spaced correctly
 - [SA1016][]: Opening attribute brackets must be spaced correctly
 - [SA1017][]: Closing attribute brackets must be spaced correctly
 - [SA1018][]: Nullable type symbols must not be preceded by space
 - [SA1019][]: Member access symbols must be spaced correctly
 - [SA1020][]: Increment decrement symbols must be spaced correctly
 - [SA1021][]: Negative signs must be spaced correctly
 - [SA1022][]: Positive signs must be spaced correctly
 - [SA1023][]: Dereference and access of symbols must be spaced correctly
 - [SA1024][]: Colons must be spaced correctly
 - [SA1025][]: Code must not contain multiple whitespace in a row
 - [SA1026][]: Code must not contain space after new keyword in implicitly typed array allocation
 - [SA1027][]: Tabs must not be used

[SA1000]: http://www.stylecop.com/docs/SA1000.html
[SA1001]: http://www.stylecop.com/docs/SA1001.html
[SA1002]: http://www.stylecop.com/docs/SA1002.html
[SA1003]: http://www.stylecop.com/docs/SA1003.html
[SA1004]: http://www.stylecop.com/docs/SA1004.html
[SA1005]: http://www.stylecop.com/docs/SA1005.html
[SA1006]: http://www.stylecop.com/docs/SA1006.html
[SA1007]: http://www.stylecop.com/docs/SA1007.html
[SA1008]: http://www.stylecop.com/docs/SA1008.html
[SA1009]: http://www.stylecop.com/docs/SA1009.html
[SA1010]: http://www.stylecop.com/docs/SA1010.html
[SA1011]: http://www.stylecop.com/docs/SA1011.html
[SA1012]: http://www.stylecop.com/docs/SA1012.html
[SA1013]: http://www.stylecop.com/docs/SA1013.html
[SA1014]: http://www.stylecop.com/docs/SA1014.html
[SA1015]: http://www.stylecop.com/docs/SA1015.html
[SA1016]: http://www.stylecop.com/docs/SA1016.html
[SA1017]: http://www.stylecop.com/docs/SA1017.html
[SA1018]: http://www.stylecop.com/docs/SA1018.html
[SA1019]: http://www.stylecop.com/docs/SA1019.html
[SA1020]: http://www.stylecop.com/docs/SA1020.html
[SA1021]: http://www.stylecop.com/docs/SA1021.html
[SA1022]: http://www.stylecop.com/docs/SA1022.html
[SA1023]: http://www.stylecop.com/docs/SA1023.html
[SA1024]: http://www.stylecop.com/docs/SA1024.html
[SA1025]: http://www.stylecop.com/docs/SA1025.html
[SA1026]: http://www.stylecop.com/docs/SA1026.html
[SA1027]: http://www.stylecop.com/docs/SA1027.html

## FxCop

We will use [FxCop](http://en.wikipedia.org/wiki/FxCop) as code analysis tool to enforce coding best practices. The ruleset includes **ALL** rules as _Error_ - **EXCEPT** the following rules:

 - [CA1002][]: Do not expose generic lists
 - [CA1006][]: Do not nest generic types in member signatures
 - [CA1014][]: Mark assemblies with `CLSCompliantAttribute`
 - [CA1017][]: Mark assemblies with `ComVisibleAttribute`
 - [CA1020][]: Avoid namespaces with few types
 - [CA1024][]: Use properties where appropriate
 - [CA1026][]: Default parameters should not be used
 - [CA1062][]: Validate arguments of public methods
 - [CA1303][]: Do not pass literals as localized parameters
 - [CA1305][]: Specify `IFormatProvider`
 - [CA1307][]: Specify `StringComparison`
 - [CA1309][]: Use ordinal StringComparison
 - [CA1726][]: Use preferred terms
 - [CA1819][]: Properties should not return arrays
 - [CA1820][]: Test for empty strings using string length
 - [CA2204][]: Literals should be spelled correctly
 - [CA2210][]: Assemblies should have valid strong names

[CA1002]: http://msdn.microsoft.com/en-us/library/ms182142.aspx
[CA1006]: http://msdn.microsoft.com/en-us/library/ms182144.aspx
[CA1014]: http://msdn.microsoft.com/en-us/library/ms182156.aspx
[CA1017]: http://msdn.microsoft.com/en-us/library/ms182157.aspx
[CA1020]: http://msdn.microsoft.com/en-us/library/ms182130.aspx
[CA1024]: http://msdn.microsoft.com/en-us/library/ms182181.aspx
[CA1026]: http://msdn.microsoft.com/en-us/library/ms182135.aspx
[CA1062]: http://msdn.microsoft.com/en-us/library/ms182182.aspx
[CA1303]: http://msdn.microsoft.com/en-us/library/ms182187.aspx
[CA1305]: http://msdn.microsoft.com/en-us/library/ms182190.aspx
[CA1307]: http://msdn.microsoft.com/en-us/library/bb386080.aspx
[CA1309]: http://msdn.microsoft.com/en-us/library/bb385972.aspx
[CA1726]: http://msdn.microsoft.com/en-us/library/ms182258.aspx
[CA1819]: http://msdn.microsoft.com/en-us/library/0fss9skc.aspx
[CA1820]: http://msdn.microsoft.com/en-us/library/ms182279.aspx
[CA2204]: http://msdn.microsoft.com/en-us/library/bb264488.aspx
[CA2210]: http://msdn.microsoft.com/en-us/library/ms182127.aspx

Ruleset

```xml
<?xml version="1.0" encoding="utf-8"?>
<RuleSet Name="FxCop rules for software development" Description="This rule set contains the rules for software development." ToolsVersion="12.0">
  <IncludeAll Action="Error" />
  <Rules AnalyzerId="Microsoft.Analyzers.ManagedCodeAnalysis" RuleNamespace="Microsoft.Rules.Managed">
    <Rule Id="CA1002" Action="None" />
    <Rule Id="CA1006" Action="None" />
    <Rule Id="CA1014" Action="None" />
    <Rule Id="CA1017" Action="None" />
    <Rule Id="CA1020" Action="None" />
    <Rule Id="CA1024" Action="None" />
    <Rule Id="CA1026" Action="None" />
    <Rule Id="CA1062" Action="None" />
    <Rule Id="CA1303" Action="None" />
    <Rule Id="CA1305" Action="None" />
    <Rule Id="CA1307" Action="None" />
    <Rule Id="CA1309" Action="None" />
    <Rule Id="CA1726" Action="None" />
    <Rule Id="CA1819" Action="None" />
    <Rule Id="CA1820" Action="None" />
    <Rule Id="CA2204" Action="None" />
    <Rule Id="CA2210" Action="None" />
  </Rules>
</RuleSet>
```

[nuget]: http://docs.nuget.org/docs/contribute/Coding-Guidelines
[ms-coding-guidelines]: http://blogs.msdn.com/b/brada/archive/2005/01/26/361363.aspx
[Camel case]: http://en.wikipedia.org/wiki/CamelCase
[Pascal case]: http://c2.com/cgi/wiki?PascalCase
[jeff-comment]: http://blog.codinghorror.com/coding-without-comments/
