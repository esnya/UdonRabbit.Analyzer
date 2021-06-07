# List of Analyzers in UdonRabbit.Analyzer

## About Severity

UdonRabbit.Analyzer reports Error for those that cause compilation errors, Warning for others.  
Those that are no longer needed due to Udon or UdonSharp updates will be assigned Hidden.

## Udon

It is an analyzer to deal with the restrictions and specifications of language features due to the specifications of VRChat Udon.
If there is no problem with the specification, but unexpected behaviours occurs, a `Severify: Warning` is assigned.

| ID      | Title                                                                                             | Category | Severity |
| ------- | ------------------------------------------------------------------------------------------------- | :------: | :------: |
| URA0001 | [Method is not exposed to Udon](./URA0001.md)                                                     |   Udon   |  Error   |
| URA0002 | [Field accessor is not exposed to Udon](./URA0002.md)                                             |   Udon   |  Error   |
| URA0011 | [Try/Catch/Finally is not supported by U#](./URA0011.md)                                          |   Udon   |  Error   |
| URA0012 | [U# does not support throwing exceptions](./URA0012.md)                                           |   Udon   |  Error   |
| URA0016 | [Udon does not support the 'Awake' event, use 'Start' instead](./URA0016.md)                      |   Udon   |  Error   |
| URA0020 | [The 'is' keyword is not yet supported by U#](./URA0020.md)                                       |   Udon   |  Error   |
| URA0021 | [The 'as' keyword is not yet supported by U#](./URA0021.md)                                       |   Udon   |  Error   |
| URA0022 | [U# does not currently support type checking with the 'is' keyword](./URA0022.md)                 |   Udon   |  Error   |
| URA0026 | [Udon does not support return values of this type](./URA0026.md)                                  |   Udon   |  Error   |
| URA0027 | [Udon does not support method parameters of this type](./URA0027.md)                              |   Udon   |  Error   |
| URA0032 | [Udon does not support variables of this type](./URA0032.md)                                      |   Udon   |  Error   |
| URA0033 | [Udon does not currently support syncing of the this type](./URA0033.md)                          |   Udon   |  Error   |
| URA0034 | [Udon does not support linear interpolation of the synced type](./URA0034.md)                     |   Udon   |  Error   |
| URA0035 | [Udon does not support variable tweening when the behaviour is in Manual sync mode](./URA0035.md) |   Udon   |  Error   |
| URA0036 | [Syncing of array type is only supported in manual sync mode](./URA0036.md)                       |   Udon   |  Error   |
| URA0041 | [The method called by SendCustomNetworkEvent must be public](./URA0041.md)                        |   Udon   | Warning  |
| URA0042 | [The method specify for SendCustomNetworkEvent must be public](./URA0042.md)                      |   Udon   | Warning  |
| URA0043 | [The method called over the network cannot start with \_](./URA0043.md)                           |   Udon   | Warning  |
| URA0044 | [The method specify for SendCustomNetworkEvent cannot start with \_](./URA0044.md)                |   Udon   | Warning  |

## UdonSharp

It is a restriction of language features by the specification of UdonSharp. Basically all analyzers are reported as `Severity: Error`.

| ID          | Title                                                                                               |   Category    | Severity  |
| ----------- | --------------------------------------------------------------------------------------------------- | :-----------: | :-------: |
| URA0003     | [U# only supports single type generics](./URA0003.md)                                               |   UdonSharp   |   Error   |
| URA0004     | [U# does not currently supports static method declarations](./URA0004.md)                           |   UdonSharp   |   Error   |
| URA0005     | [U# does not yet support inheriting from interfaces](./URA0005.md)                                  |   UdonSharp   |   Error   |
| ~~URA0006~~ | [~~U# does not yet support inheriting from classes other than `UdonSharpBehaviour`~~](./URA0006.md) | ~~UdonSharp~~ | ~~Error~~ |
| URA0007     | [U# does not currently support constructors on UdonSharpBehaviour](./URA0007.md)                    |   UdonSharp   |   Error   |
| URA0008     | [User property declarations are not yet supported by U#](./URA0008.md)                              |   UdonSharp   |   Error   |
| URA0009     | [Base type calling is not yet supported by U#](./URA0009.md)                                        |   UdonSharp   |   Error   |
| URA0010     | [Default expressions are not yet supported by U#](./URA0010.md)                                     |   UdonSharp   |   Error   |
| URA0013     | [U# does not support multidimensional arrays, use jagged arrays](./URA0013.md)                      |   UdonSharp   |   Error   |
| URA0014     | [U# does not support multidimensional array accesses yet](./URA0014.md)                             |   UdonSharp   |   Error   |
| URA0015     | [U# does not currently support null conditional operators](./URA0015.md)                            |   UdonSharp   |   Error   |
| URA0017     | [U# does not yet support 'out' parameters on user-defined methods](./URA0017.md)                    |   UdonSharp   |   Error   |
| URA0018     | [U# does not yet support 'in' parameters on user-defined methods](./URA0018.md)                     |   UdonSharp   |   Error   |
| URA0019     | [U# does not yet support 'ref' parameters on user-defined methods](./URA0019.md)                    |   UdonSharp   |   Error   |
| URA0023     | [Only one class declaration per file is currently supported by U#](./URA0023.md)                    |   UdonSharp   |   Error   |
| URA0024     | [U# custom methods currently do not support default arguments or params arguments](./URA0024.md)    |   UdonSharp   |   Error   |
| URA0025     | [Static fields are not yet supported by U#](./URA0025.md)                                           |   UdonSharp   |   Error   |
| ~~URA0028~~ | [~~U# does not yet support user defined enums~~](./URA0028.md)                                      | ~~UdonSharp~~ | ~~Error~~ |
| URA0029     | [U# does not currently support using typeof on user defined types](./URA0029.md)                    |   UdonSharp   |   Error   |
| URA0030     | [U# does not yet support static using directives](./URA0030.md)                                     |   UdonSharp   |   Error   |
| URA0031     | [U# does not yet support namespace alias directives](./URA0031.md)                                  |   UdonSharp   |   Error   |
| URA0037     | [U# does not yet support goto](./URA0037.md)                                                        |   UdonSharp   |   Error   |
| URA0038     | [U# does not yet support labeled statements](./URA0038.md)                                          |   UdonSharp   |   Error   |
| URA0039     | [Object initializers are not yet supported by U#](./URA0039.md)                                     |   UdonSharp   |   Error   |
| URA0040     | [Nullable types are not currently supported by U#](./URA0040.md)                                    |   UdonSharp   |   Error   |
