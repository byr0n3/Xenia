# Xenia

<img src="./assets/logo.png" alt="Xenia Logo">

A simple, minimalistic HTTP/1.1 server written in C#.

## Packages

Xenia is made up from multiple packages, all of which being optional (with of course the exception to the core package).

### Xenia

The core package, containing the base server and request/response logic.

[Read more](Xenia/README.md)

### Xenia.JSON

Package with utilities that make it easier to work with JSON content.

[Read more](Xenia.JSON/README.md)

### Xenia.Encoding

Provides utilities for encoding HTTP responses based on the request's preferred encoding.

[Read more](Xenia.Encoding/README.md)

### Xenia.Caching

Utilities for dealing with caching content.

### Xenia.Tests

Unit tests for different functionalities.

### Xenia.Example

Example usages for reading requests and returning responses.

## Examples

For examples on how to use Xenia, you should check the following resources:

- [Xenia Quickstart guide](Xenia/README.md#quickstart) – Fastest way to start using Xenia
- [Xenia.Example project](Xenia.Example) – More concrete and functional example server
- [Xenia.Tests unit tests](Xenia.Tests) – The best way to see how different parts of Xenia work and what the expected
  results are.
