using System.Text;

namespace Qwaitumin.Logging;

public readonly struct Message
{
  public enum Level { INFO, WARN, ERROR, DEBUG }

  private static readonly string ANSI_NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
  private static readonly Dictionary<Level, string> BBCodeColors = new()
  {
    {Level.INFO, "00BFFF"},
    {Level.WARN, "FFA500"},
    {Level.ERROR, "FF0000"},
    {Level.DEBUG, "800080"}
  };
  private static readonly Dictionary<Level, string> AnsiColors = new()
  {
    {Level.INFO, "\u001b[38;2;0;191;255m"},
    {Level.WARN, "\u001b[38;2;255;166;0m"},
    {Level.ERROR, "\u001b[38;2;255;0;0m"},
    {Level.DEBUG, "\u001b[38;2;128;0;128m"}
  };

  private readonly LogSettings logSettings;

  public readonly Level Type;
  public readonly string TypeStr;
  public readonly string Time;
  public readonly string ClassName;
  public readonly string ThreadId;
  public readonly string Text;

  private Message(Level type, string className, string text, LogSettings logSettings)
  {
    this.logSettings = logSettings;
    Time = DateTime.Now.ToString(logSettings.TimeFormat);
    ThreadId = Environment.CurrentManagedThreadId.ToString("X")
      .PadLeft((int)logSettings.ThreadIdSize)[..(int)logSettings.ThreadIdSize];
    TypeStr = type.ToString().PadLeft(5)[..5];
    Type = type;
    ClassName = className.PadLeft((int)logSettings.ClassNameSize)[..(int)logSettings.ClassNameSize];
    Text = text;
  }

  public string GetAsString(bool disableColor = false)
  {
    var builder = new StringBuilder();
    builder.Append(GetContext(disableColor)).Append(" : ");
    builder.Append(Text);
    return builder.ToString();
  }

  public override string ToString()
    => GetAsString();

  private string GetContext(bool disableColor)
  {
    string finalTypeStr = TypeStr;
    if (logSettings.ColorType == ColorType.BBCODE && !disableColor)
      finalTypeStr = AddBBCodeToString(finalTypeStr, BBCodeColors[Type]);
    if (logSettings.ColorType == ColorType.ANSI && !disableColor)
      finalTypeStr = AddAnsiColorToString(finalTypeStr, AnsiColors[Type]);

    var contextBuilder = new StringBuilder();
    contextBuilder.Append($"{Time} {finalTypeStr} [{ClassName}]");
    if (logSettings.LogThread)
      contextBuilder.Append($" [{ThreadId}]");

    return contextBuilder.ToString();
  }

  private static string AddBBCodeToString(string msg, string color)
    => $"[color={color}]{msg}[/color]";

  private static string AddAnsiColorToString(string msg, string color)
    => $"{color}{msg}{ANSI_NORMAL}";

  public static Message GetInfo(string className, string text, LogSettings logSettings)
    => new(Level.INFO, className, text, logSettings);
  public static Message GetWarning(string className, string text, LogSettings logSettings)
    => new(Level.WARN, className, text, logSettings);
  public static Message GetError(string className, string text, LogSettings logSettings)
    => new(Level.ERROR, className, text, logSettings);
  public static Message GetDebug(string className, string text, LogSettings logSettings)
    => new(Level.DEBUG, className, text, logSettings);
}