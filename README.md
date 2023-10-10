# Td.CliWrap.Fsharp

Primitive bindings for F# of [CliWrap](https://github.com/Tyrrrz/CliWrap) library.

## Sample

```fsharp
let! executionResult =
    "magick"
    |> wrap
    |> withStandardErrorPipe (PipeTarget.ToStringBuilder errSb)
    |> withArguments [ inputFile; "-scale"; "512x512>"; "-liquid-rescale"; "50%"; "-scale"; "200%"; outFile ]
    |> withValidation CommandResultValidation.None
    |> executeBuffered

if executionResult.ExitCode = 0 then
    let outStream = new StreamReader(outFile)
    outStream.BaseStream.CopyTo(target)

    (target, outFile) |> Result.Ok
else
    string errSb |> Result.Error

```
