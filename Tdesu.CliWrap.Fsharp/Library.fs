module Tdesu.CliWrap.Fsharp

open System
open System.IO
open System.Threading
open System.Threading.Tasks

/// <summary>
///     Instructions for running a process.
/// </summary>
type CliCommand = CliWrap.Command

/// <summary>
///     Represents a pipe for the process's standard input stream.
/// </summary>
type CliPipeSource = CliWrap.PipeSource

/// <summary>
///     Represents a pipe for the process's standard output or standard error stream.
/// </summary>
type CliPipeTarget = CliWrap.PipeTarget

/// <summary>
///     Creates a new command that targets the specified command-line executable, batch file, or script.
/// </summary>
let wrap target =
    CliWrap.Cli.Wrap(target)

/// <summary>
///     Creates a copy of this command, setting the arguments to the value
///     obtained by formatting the specified sequence of arguments. Arguments won't be escaped.
/// </summary>
let withArguments (args: string seq) (command: CliCommand) =
    command.WithArguments(args, false)

/// <summary>
///     Creates a copy of this command, setting the argument to the value.
///     Arguments won't be escaped.
/// </summary>
let withArgument (arg: string) (command: CliCommand) =
    command.WithArguments(Array.singleton arg, false)

/// <summary>
///     Creates a copy of this command, setting the arguments to the value
///     obtained by formatting the specified sequence of arguments. Arguments will be escaped.
/// </summary>
let withEscapedArguments (args: string seq) (command: CliCommand) =
    command.WithArguments(args, true)

/// <summary>
///     Creates a copy of this command, setting the arguments to the value
///     obtained by formatting the specified sequence of arguments.
/// </summary>
let withArgumentsEscape (args: string seq) escape (command: CliCommand) =
    command.WithArguments(args, escape)

type PipeSource =
    /// <summary>
    ///     Pipe source that does not provide any data.
    ///     Functionally equivalent to a null device.
    /// </summary>
    | Null

    /// <summary>
    ///     Creates an anonymous pipe source with the (Stream -> Unit) signature.
    /// </summary>
    | Create of action: (Stream -> Unit)

    /// <summary>
    ///     Creates a pipe source that reads from the specified memory buffer.
    /// </summary>
    | FromBytes of data: byte array

    /// <summary>
    ///     Creates a pipe source that reads from the standard output of the specified command.
    /// </summary>
    | FromCommand of command: CliWrap.Command

    /// <summary>
    ///     Creates a pipe source that reads from the specified file.
    /// </summary>
    | FromFile of filePath: string

    /// <summary>
    ///     Creates a pipe source that reads from the specified stream.
    /// </summary>
    | FromStream of stream: Stream

    /// <summary>
    ///     Creates a pipe source that reads from the specified stream.
    /// </summary>
    | FromString of str: string

    /// <summary>
    ///     Creates a pipe source that reads from the specified memory buffer.
    /// </summary>
    | FromMemory of data: ReadOnlyMemory<byte>

/// <summary>
///     Creates a copy of this command, setting the standard input pipe to the specified source.
/// </summary>
let withStandardInputPipe (source: PipeSource) (command: CliCommand) =
    let pipeSource =
        match source with
        | Null ->
            CliPipeSource.Null
        | Create action ->
            CliPipeSource.Create(action)
        | FromBytes data ->
            CliPipeSource.FromBytes(data)
        | FromCommand command ->
            CliPipeSource.FromCommand(command)
        | FromFile filePath ->
            CliPipeSource.FromFile(filePath)
        | FromStream stream ->
            CliPipeSource.FromStream(stream)
        | FromString str ->
            CliPipeSource.FromString(str)
        | FromMemory data ->
            CliPipeSource.FromBytes(data)

    command.WithStandardInputPipe(pipeSource)

type PipeTarget =
    /// <summary>
    ///     Pipe target that discards all data.
    ///     Functionally equivalent to a null device.
    /// </summary>
    /// <remarks>
    ///     Using this target results in the corresponding stream (standard output or standard error)
    ///     not being opened for the underlying process at all.
    ///     In the vast majority of cases, this behavior should be functionally equivalent to piping
    ///     to a null stream, but without the performance overhead of consuming and discarding unneeded data.
    /// </remarks>
    | Null

    /// <summary>
    ///     Creates an anonymous pipe target with (Stream -> Unit) signature.
    /// </summary>
    | Create of pipeHandler: (Stream -> unit)

    /// <summary>
    ///     Creates an pipe target with (Stream -> CancellationToken -> Task) signature.
    ///     Like `Create` but asynchronous.
    /// </summary>
    | CreateAsync of pipeHandler: (Stream -> CancellationToken -> Task)

    /// <summary>
    ///     Creates a pipe target that writes to the specified file.
    /// </summary>
    | ToFile of path: string

    /// <summary>
    ///     Creates a pipe target that writes to the specified stream.
    /// </summary>
    | ToStream of stream: Stream * flush: bool voption

    /// <summary>
    ///     Creates a pipe target that writes to the specified string builder.
    /// </summary>
    | ToStringBuilder of sb: System.Text.StringBuilder

/// <summary>
///     Creates a copy of this command, setting the standard error pipe to the specified target.
/// </summary>
let withStandardErrorPipe (target: PipeTarget) (command: CliCommand) =
    let target =
        match target with
        | Null ->
            CliPipeTarget.Null
        | Create pipeHandler ->
            CliPipeTarget.Create(pipeHandler)
        | CreateAsync pipeHandler ->
            CliPipeTarget.Create(pipeHandler)
        | ToFile path ->
            CliPipeTarget.ToFile(path)
        | ToStream(stream, ValueNone) ->
            CliPipeTarget.ToStream(stream)
        | ToStream(stream, ValueSome flush) ->
            CliPipeTarget.ToStream(stream, flush)
        | ToStringBuilder sb ->
            CliPipeTarget.ToStringBuilder(sb)

    command.WithStandardErrorPipe(target)

/// <summary>
///     Creates a copy of this command, setting the standard output pipe to the specified target.
/// </summary>
let withStandardOutputPipe (target: PipeTarget) (command: CliCommand) =
    let target =
        match target with
        | Null ->
            CliPipeTarget.Null
        | Create pipeHandler ->
            CliPipeTarget.Create(pipeHandler)
        | CreateAsync pipeHandler ->
            CliPipeTarget.Create(pipeHandler)
        | ToFile path ->
            CliPipeTarget.ToFile(path)
        | ToStream(stream, ValueNone) ->
            CliPipeTarget.ToStream(stream)
        | ToStream(stream, ValueSome flush) ->
            CliPipeTarget.ToStream(stream, flush)
        | ToStringBuilder sb ->
            CliPipeTarget.ToStringBuilder(sb)

    command.WithStandardOutputPipe(target)

open CliWrap.Buffered

/// <summary>
///     Strategy used for validating the result of a command execution.
/// </summary>
type CliCommandResultValidation = CliWrap.CommandResultValidation

type CommandResultValidation =
    /// Ensure that the command returned a zero exit code.
    | ZeroExitCode

    /// No validation.
    | None

/// <summary>
///     Creates a copy of this command, setting the validation options to the specified value.
/// </summary>
let withValidation (validation: CommandResultValidation) (command: CliCommand) =
    let validation =
        match validation with
        | ZeroExitCode ->
            CliCommandResultValidation.ZeroExitCode
        | None ->
            CliCommandResultValidation.None
    command.WithValidation(validation)

/// <summary>
///     Executes the command asynchronously with buffering.
///     Data written to the standard output and standard error streams is decoded as text
///     and returned as part of the result object.
/// </summary>
/// <remarks>
///     Synchronous version of <see cref="executeBufferedAsync"/>
/// </remarks>
let executeBuffered (command: CliCommand) =
    command.ExecuteBufferedAsync().ConfigureAwait(false).GetAwaiter().GetResult()

/// <summary>
///     Executes the command asynchronously with buffering.
///     Data written to the standard output and standard error streams is decoded as text
///     and returned as part of the result object.
///     Uses <see cref="Console.OutputEncoding" /> for decoding.
/// </summary>
/// <remarks>
///     Could be awaited.
/// </remarks>
let executeBufferedAsync (encoding: System.Text.Encoding) (command: CliCommand) =
    command.ExecuteBufferedAsync(encoding)
