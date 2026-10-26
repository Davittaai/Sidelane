using System.IO.Pipes;
using System.Text;
using Sidelane.Services;

// Chrome native messaging is a length-prefixed UTF-8 protocol over stdin/stdout.
// Never write diagnostics to stdout.
using var input = Console.OpenStandardInput();
using var output = Console.OpenStandardOutput();
var header = new byte[4];
while (await ReadFullyAsync(input, header))
{
    var length = BitConverter.ToInt32(header);
    if (length < 1 || length > 1024) break;
    var data = new byte[length];
    if (!await ReadFullyAsync(input, data)) break;
    var json = Encoding.UTF8.GetString(data);
    var accepted = false;
    if (AiSignalPolicy.Parse(json) is not null)
    {
        try
        {
            using var pipe = new NamedPipeClientStream(".", AiStatusServerPipeName,
                PipeDirection.Out, PipeOptions.Asynchronous);
            await pipe.ConnectAsync(400);
            var message = Encoding.UTF8.GetBytes(json + "\n");
            await pipe.WriteAsync(message);
            await pipe.FlushAsync();
            accepted = true;
        }
        catch (Exception ex) when (ex is IOException or TimeoutException) { }
    }
    var response = Encoding.UTF8.GetBytes(accepted ? "{\"ok\":true}" : "{\"ok\":false}");
    await output.WriteAsync(BitConverter.GetBytes(response.Length));
    await output.WriteAsync(response);
    await output.FlushAsync();
}

const string AiStatusServerPipeName = "Sidelane.BrowserState.V1";

static async Task<bool> ReadFullyAsync(Stream stream, byte[] bytes)
{
    var offset = 0;
    while (offset < bytes.Length)
    {
        var count = await stream.ReadAsync(bytes.AsMemory(offset));
        if (count == 0) return false;
        offset += count;
    }
    return true;
}
