## Smx.SharpIO
SharpIO is a library that makes it easier to work with binary files

The main feature is the ability to create `Memory<byte>`, and therefore `Span<byte>`, from a `MemoryMappedFile`.

This effectively enables true random access files in C# without having to use intermediate copies or buffered streams.


