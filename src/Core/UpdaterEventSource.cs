using System.Diagnostics.Tracing;

namespace Upgradier.Core;

public class UpdaterEventSource : EventSource
{
    public static class Keywords
    {
        public const EventKeywords Startup = (EventKeywords)0b_000_001;
        public const EventKeywords End = (EventKeywords)0b_000_010;
        public const EventKeywords Database = (EventKeywords)0b_000_100;
    }
    public static class Identifiers
    {
        public const int UpdateStart = 1;
        public const int UpdateStop = 2;
        public const int DatabaseStart = 3;
        public const int DatabaseLocked = 4;
        public const int DatabaseSkipped = 5;
        public const int DatabaseStop = 6;
    }
    public static UpdaterEventSource Log { get; } = new UpdaterEventSource();
    public UpdaterEventSource() : base(nameof(UpdaterEventSource))
    {
    }

    [Event(eventId: Identifiers.UpdateStart, Level = EventLevel.Informational, Keywords = Keywords.Startup, Opcode = EventOpcode.Start)]
    public void UpdateStart() => WriteEvent(Identifiers.UpdateStart);

    [Event(eventId: Identifiers.UpdateStop, Level = EventLevel.Informational, Keywords = Keywords.End, Opcode = EventOpcode.Stop)]
    public void UpdateStop() => WriteEvent(Identifiers.UpdateStop);

    [Event(eventId: Identifiers.DatabaseStart, Level = EventLevel.Informational, Keywords = Keywords.Database, Opcode = EventOpcode.Info)]
    public void DatabaseStart(string server, string database, string version) => WriteEvent(Identifiers.DatabaseStart, $"{server}/{database}/{version}");

    [Event(eventId: Identifiers.DatabaseStop, Level = EventLevel.Informational, Keywords = Keywords.Database, Opcode = EventOpcode.Info)]
    public void DatabaseStop(string server, string database, string version) => WriteEvent(Identifiers.DatabaseStop, $"{server}/{database}/{version}");

    [Event(eventId: Identifiers.DatabaseLocked, Level = EventLevel.Informational, Keywords = Keywords.Database, Opcode = EventOpcode.Info)]
    public void DatabaseLocked(string server, string database, long lockValue) => WriteEvent(Identifiers.DatabaseStart, $"{server}/{database}/{lockValue}");

    [Event(eventId: Identifiers.DatabaseSkipped, Level = EventLevel.Informational, Keywords = Keywords.Database, Opcode = EventOpcode.Info)]
    public void DatabaseSkipped(string server, string database) => WriteEvent(Identifiers.DatabaseStart, $"{server}/{database}");
}