namespace Simplz.Hue.Models;

internal record Room(Guid Id, string Name, List<Guid> LightIds);