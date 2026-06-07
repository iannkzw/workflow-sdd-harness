Console.InputEncoding = new System.Text.UTF8Encoding(false);
Console.OutputEncoding = new System.Text.UTF8Encoding(false);

string mode = args.Length > 0 ? args[0] : "echo";

switch (mode)
{
    case "echo":
        var stdin = Console.In.ReadToEnd();
        var escaped = System.Text.Json.JsonSerializer.Serialize(stdin);
        Console.WriteLine($"{{\"result\": {escaped}, \"usage\": {{\"input_tokens\": 1, \"output_tokens\": 1}}}}");
        break;
    case "fixed":
        Console.WriteLine("{\"result\": \"Olá, ação! ç ã é.\", \"usage\": {\"input_tokens\": 5, \"output_tokens\": 5}}");
        break;
    case "exit1":
        Console.WriteLine("{\"result\": \"error output\"}");
        Console.Error.WriteLine("error on stderr");
        Environment.Exit(1);
        break;
    case "empty":
        break;
    case "garbage":
        Console.WriteLine("not-json-at-all");
        break;
    case "sleep":
        int ms = args.Length > 1 ? int.Parse(args[1]) : 1000;
        await Task.Delay(ms);
        Console.WriteLine("{\"result\": \"done\", \"usage\": {\"input_tokens\": 1, \"output_tokens\": 1}}");
        break;
}
