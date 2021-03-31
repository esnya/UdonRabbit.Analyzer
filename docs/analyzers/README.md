# List of Analyzers in UdonRabbit.Analyzer

## About Severity

UdonRabbit.Analyzer reports Error for those that cause compilation errors, Warning for others.  
Those that are no longer needed due to Udon or UdonSharp updates will be assigned Hidden.

## Udon / UdonSharp

| ID          | Title                                                                                               |   Category    | Severity  |
| ----------- | --------------------------------------------------------------------------------------------------- | :-----------: | :-------: |
| URA0001     | [Method is not exposed to Udon](./URA0001.md)                                                       |     Udon      |   Error   |
| URA0002     | [Field accessor is not exposed to Udon](./URA0002.md)                                               |     Udon      |   Error   |
| URA0003     | [U# only supports single type generics](./URA0003.md)                                               |   UdonSharp   |   Error   |
| URA0004     | [U# does not currently supports static method declarations](./URA0004.md)                           |   UdonSharp   |   Error   |
| URA0005     | [U# does not yet support inheriting from interfaces](./URA0005.md)                                  |   UdonSharp   |   Error   |
| ~~URA0006~~ | [~~U# does not yet support inheriting from classes other than `UdonSharpBehaviour`~~](./URA0006.md) | ~~UdonSharp~~ | ~~Error~~ |
| URA0007     | [U# does not currently support constructors on UdonSharpBehaviour](./URA0007.md)                    |   UdonSharp   |   Error   |
| URA0008     | [User property declarations are not yet supported by U#](./URA0008.md)                              |   UdonSharp   |   Error   |
| URA0009     | [Base type calling is not yet supported by U#](./URA0009.md)                                        |   UdonSharp   |   Error   |
| URA0010     | [Default expressions are not yet supported by U#](./URA0010.md)                                     |   UdonSharp   |   Error   |
| URA0011     | [Try/Catch/Finally is not supported by U#](./URA0011.md)                                            |     Udon      |   Error   |
| URA0012     | [U# does not support throwing exceptions](./URA0012.md)                                             |     Udon      |   Error   |
| URA0013     | [U# does not support multidimensional arrays, use jagged arrays](./URA0013.md)                      |   UdonSharp   |   Error   |
| URA0014     | [U# does not support multidimensional array accesses yet](./URA0014.md)                             |   UdonSharp   |   Error   |
| URA0015     | [U# does not currently support null conditional operators](./URA0015.md)                            |   UdonSharp   |   Error   |
| URA0016     | [Udon does not support the 'Awake' event, use 'Start' instead](./URA0016.md)                        |     Udon      |   Error   |
| URA0017     | [U# does not yet support 'out' parameters on user-defined methods](./URA0017.md)                    |   UdonSharp   |   Error   |
| URA0018     | [U# does not yet support 'in' parameters on user-defined methods](./URA0018.md)                     |   UdonSharp   |   Error   |
| URA0019     | [U# does not yet support 'ref' parameters on user-defined methods](./URA0019.md)                    |   UdonSharp   |   Error   |
| URA0020     | [The 'is' keyword is not yet supported by U#](./URA0020.md)                                         |     Udon      |   Error   |
| URA0021     | [The 'as' keyword is not yet supported by U#](./URA0021.md)                                         |     Udon      |   Error   |
| URA0022     | [U# does not currently support type checking with the 'is' keyword](./URA0022.md)                   |     Udon      |   Error   |
| URA0023     | [Only one class declaration per file is currently supported by U#](./URA0023.md)                    |   UdonSharp   |   Error   |
